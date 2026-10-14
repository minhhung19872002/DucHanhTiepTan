using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>
/// Equipment catalog – legacy EquipmentManagerControl / DeclareEquipmentDialog.
/// IsVarious = false → "Tài sản" (Cố định), IsVarious = true → "Vật tư" (Tiêu hao).
/// </summary>
[ApiController]
[Route("api/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;
    public EquipmentController(AppDbContext db, ICurrentUser user) { _db = db; _user = user; }

    public record EquipmentDto(int Id, string Name, string Serial, string? Unit, long Price, bool IsVarious, string Vendor,
        string? Position, string? Usage, string? Note, int Quantity);
    public record EquipmentInput(string Name, string Serial, string? Unit, long Price, bool IsVarious, string Vendor, string? Position, string? Usage, string? Note);

    /// <summary>
    /// GET api/equipment?isVarious=&amp;keyword=&amp;searchBy=name|serial|usage&amp;branchId=&amp;showEmpty=&amp;limit=
    /// Quantity = stock in the selected branch (branchId 0 / admin "Tất cả" = total of all branches).
    /// </summary>
    [HttpGet]
    public async Task<List<EquipmentDto>> List([FromQuery] bool? isVarious, [FromQuery] string? keyword, [FromQuery] string searchBy = "name",
        [FromQuery] int? branchId = null, [FromQuery] bool showEmpty = true, [FromQuery] int limit = 0)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.Equipments.AsNoTracking().AsQueryable();
        if (isVarious.HasValue) q = q.Where(e => e.IsVarious == isVarious.Value);
        q = searchBy switch
        {
            "serial" => q.WhereUnaccentLike(e => e.Serial, keyword),
            "usage" => q.WhereUnaccentLike(e => e.Usage, keyword),
            _ => q.WhereUnaccentLike(e => e.Name, keyword),
        };
        if (!showEmpty)
            q = q.Where(e => e.EquimentInBranches.Any(b => (scope == 0 || b.BranchId == scope) && b.Quantity > 0));
        q = q.OrderBy(e => e.Name);
        if (limit > 0) q = q.Take(limit);
        return await q.Select(e => new EquipmentDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.Vendor, e.Position, e.Usage, e.Note,
            e.EquimentInBranches.Where(b => scope == 0 || b.BranchId == scope).Sum(b => (int?)b.Quantity) ?? 0)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentDto>> Get(int id, [FromQuery] int? branchId = null)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var dto = await _db.Equipments.AsNoTracking().Where(e => e.Id == id)
            .Select(e => new EquipmentDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.Vendor, e.Position, e.Usage, e.Note,
                e.EquimentInBranches.Where(b => scope == 0 || b.BranchId == scope).Sum(b => (int?)b.Quantity) ?? 0)).FirstOrDefaultAsync();
        return dto ?? throw new NotFoundException("Không tìm thấy thiết bị");
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentDto>> Create(EquipmentInput input)
    {
        Validate(input);
        var e = new Equipment();
        Apply(e, input);
        _db.Equipments.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.Id }, (await Get(e.Id)).Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EquipmentDto>> Update(int id, EquipmentInput input)
    {
        Validate(input);
        var e = await _db.Equipments.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy thiết bị");
        Apply(e, input);
        await _db.SaveChangesAsync();
        return (await Get(id)).Value!;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Equipments.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy thiết bị");
        var inUse = await _db.ContainedEquipmentInvoices.AnyAsync(x => x.EquipmentId == id)
            || await _db.ContainedEquipmentExportInvoices.AnyAsync(x => x.EquipmentId == id)
            || await _db.ContainedUsedEquipments.AnyAsync(x => x.EquipmentId == id)
            || await _db.EquimentInBranches.AnyAsync(x => x.EquipmentId == id && x.Quantity != 0);
        if (inUse) throw new ApiException("Không thể xóa thiết bị này được, có thể nó đang được sử dụng.");
        _db.EquimentInBranches.RemoveRange(_db.EquimentInBranches.Where(x => x.EquipmentId == id));
        _db.Equipments.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Validate(EquipmentInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên.");
        if (string.IsNullOrWhiteSpace(input.Serial)) throw new ApiException("Vui lòng điền số Serial.");
        if (string.IsNullOrWhiteSpace(input.Vendor)) throw new ApiException("Vui lòng điền nhà sản xuất.");
        if (input.Price < 0) throw new ApiException("Giá nhập không hợp lệ.");
    }

    private static void Apply(Equipment e, EquipmentInput input)
    {
        e.Name = input.Name.Trim();
        e.Serial = input.Serial.Trim();
        e.Vendor = input.Vendor.Trim();
        e.Unit = input.Unit;
        e.Price = input.Price;
        e.IsVarious = input.IsVarious;
        e.Position = input.Position;
        e.Usage = input.Usage;
        e.Note = input.Note;
    }
}
