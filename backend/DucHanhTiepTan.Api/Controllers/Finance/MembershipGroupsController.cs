using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>
/// "Mã VIP" – nhóm thẻ (legacy MemberShipManagerControl + CreateMememberShipGroupDialog).
/// IsFixed = true: one percentage for every service (FixedValue 0..1). IsFixed = false: per-service ServiceDiscount rows.
/// </summary>
[ApiController]
[Route("api/membership-groups")]
[Authorize(Policy = "Admin")]
public class MembershipGroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MembershipGroupsController(AppDbContext db) => _db = db;

    public record ServiceDiscountDto(int ServiceId, string ServiceName, int Percent);
    public record GroupDto(int Id, string Name, int BranchId, string BranchName, bool IsFixed, int? Percent, string? Description, string Services, int MemberCount);
    public record GroupDetailDto(int Id, string Name, int BranchId, string BranchName, bool IsFixed, int? Percent, string? Description, List<ServiceDiscountDto> Discounts);
    public record GroupInput(string Name, int BranchId, bool IsFixed, int Percent, string? Description, List<ServiceDiscountInput>? Discounts);
    public record ServiceDiscountInput(int ServiceId, int Percent);
    public record ServiceLookupDto(int Id, string Name, long Price, string? GroupName);

    private static string TypeName(bool isFixed) => isFixed ? "Tất cả dịch vụ" : "Dịch vụ chỉ định";

    [HttpGet]
    public async Task<List<GroupDto>> List()
    {
        var groups = await _db.MemberShipGroups.Include(g => g.Branch).OrderBy(g => g.Name).ToListAsync();
        var services = await _db.ServiceDiscounts.Include(s => s.Service).OrderBy(s => s.Service!.Name)
            .GroupBy(s => s.MemberShipGroupId).Select(g => new { g.Key, Names = g.Select(s => s.Service!.Name).ToList() }).ToListAsync();
        var counts = await _db.Memberships.GroupBy(m => m.GroupId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
        return groups.Select(g => new GroupDto(g.Id, g.Name, g.BranchId, g.Branch?.Name ?? string.Empty, g.IsFixed,
            g.IsFixed ? (int)Math.Round(g.FixedValue * 100) : null, g.Description,
            g.IsFixed ? TypeName(true) : string.Join(", ", services.FirstOrDefault(s => s.Key == g.Id)?.Names ?? new List<string>()),
            counts.FirstOrDefault(c => c.Key == g.Id)?.Count ?? 0)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<GroupDetailDto> Get(int id)
    {
        var g = await _db.MemberShipGroups.Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy nhóm thẻ");
        var discounts = await _db.ServiceDiscounts.Where(s => s.MemberShipGroupId == id)
            .Select(s => new ServiceDiscountDto(s.ServiceId, s.Service!.Name, (int)Math.Round(s.Value * 100))).ToListAsync();
        return new GroupDetailDto(g.Id, g.Name, g.BranchId, g.Branch?.Name ?? string.Empty, g.IsFixed, g.IsFixed ? (int)Math.Round(g.FixedValue * 100) : null, g.Description, discounts);
    }

    /// <summary>Services list for the per-service discount grid ("Tải tất cả dịch vụ").</summary>
    [HttpGet("lookups/services")]
    public async Task<List<ServiceLookupDto>> Services() =>
        await _db.Services.OrderBy(s => s.Name).Select(s => new ServiceLookupDto(s.Id, s.Name, s.Price, s.Group != null ? s.Group.Name : null)).ToListAsync();

    private static List<ServiceDiscountInput> Validate(GroupInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên");
        if (input.IsFixed && input.Percent <= 0) throw new ApiException("Giá trị khuyến mãi phải lớn hơn 0.");
        if (input.Percent < 0 || input.Percent > 100) throw new ApiException("Giá trị khuyến mãi phải từ 0 đến 100 (%)");
        var discounts = (input.Discounts ?? new()).Where(d => d.Percent > 0).ToList();
        if (!input.IsFixed)
        {
            if (discounts.Count == 0) throw new ApiException("Vui lòng chọn các dịch vụ được giảm giá");
            if (discounts.Any(d => d.Percent > 100)) throw new ApiException("Giảm giá tối đa 100%");
            var dup = discounts.GroupBy(d => d.ServiceId).FirstOrDefault(g => g.Count() > 1);
            if (dup is not null) throw new ApiException("Dịch vụ bị trùng");
        }
        return discounts;
    }

    [HttpPost]
    public async Task<ActionResult<GroupDetailDto>> Create(GroupInput input)
    {
        var discounts = Validate(input);
        if (!await _db.Branches.AnyAsync(b => b.Id == input.BranchId)) throw new ApiException("Chi nhánh không hợp lệ");
        var g = new MemberShipGroup
        {
            Name = input.Name.Trim(), BranchId = input.BranchId, IsFixed = input.IsFixed,
            FixedValue = input.IsFixed ? input.Percent / 100.0 : 0, Description = input.Description,
        };
        if (!input.IsFixed)
            foreach (var d in discounts) g.ServiceDiscounts.Add(new ServiceDiscount { ServiceId = d.ServiceId, Value = d.Percent / 100.0 });
        _db.MemberShipGroups.Add(g);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = g.Id }, await Get(g.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GroupInput input)
    {
        var discounts = Validate(input);
        var g = await _db.MemberShipGroups.Include(x => x.ServiceDiscounts).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy nhóm thẻ");
        if (!await _db.Branches.AnyAsync(b => b.Id == input.BranchId)) throw new ApiException("Chi nhánh không hợp lệ");
        g.Name = input.Name.Trim(); g.BranchId = input.BranchId; g.IsFixed = input.IsFixed;
        g.FixedValue = input.IsFixed ? input.Percent / 100.0 : 0; g.Description = input.Description;
        _db.ServiceDiscounts.RemoveRange(g.ServiceDiscounts);
        g.ServiceDiscounts.Clear();
        if (!input.IsFixed)
            foreach (var d in discounts) g.ServiceDiscounts.Add(new ServiceDiscount { MemberShipGroupId = id, ServiceId = d.ServiceId, Value = d.Percent / 100.0 });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.MemberShipGroups.Include(x => x.ServiceDiscounts).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy nhóm thẻ");
        if (await _db.Memberships.AnyAsync(m => m.GroupId == id)) throw new ApiException("Không thể xóa giá trị này, có thể nó đang được sử dụng.");
        _db.ServiceDiscounts.RemoveRange(g.ServiceDiscounts);
        _db.MemberShipGroups.Remove(g);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
