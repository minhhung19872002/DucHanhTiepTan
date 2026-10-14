using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>"Mã VIP" – thẻ thành viên (legacy CreateMememberShipDialog + membership grid). Id is the card code (upper-case).</summary>
[ApiController]
[Route("api/memberships")]
[Authorize]
public class MembershipsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MembershipsController(AppDbContext db) => _db = db;

    public record MembershipDto(string Id, DateTime CreatedDate, DateTime? ExpiredDate, string? Description, int GroupId, string GroupName,
        bool IsFixed, int? Percent, int UsedCount);
    public record MembershipInput(string Id, DateTime CreatedDate, DateTime? ExpiredDate, string? Description, int GroupId);
    public record MembershipUpdateInput(DateTime CreatedDate, DateTime? ExpiredDate, string? Description);

    private static IQueryable<MembershipDto> Project(IQueryable<Membership> q) => q.Select(m => new MembershipDto(
        m.Id, m.CreatedDate, m.ExpiredDate, m.Description, m.GroupId, m.Group!.Name, m.Group.IsFixed,
        m.Group.IsFixed ? (int?)Math.Round(m.Group.FixedValue * 100) : null, m.Prescriptions.Count));

    /// <summary>List cards, optionally by group and/or keyword (code or description, accent-insensitive).</summary>
    [HttpGet]
    public async Task<List<MembershipDto>> List([FromQuery] int? groupId, [FromQuery] string? keyword)
    {
        var q = _db.Memberships.AsQueryable();
        if (groupId.HasValue) q = q.Where(m => m.GroupId == groupId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = "%" + VietnameseText.Normalize(keyword) + "%";
            q = q.Where(m => EF.Functions.ILike(EF.Functions.Unaccent(m.Id), pattern) || (m.Description != null && EF.Functions.ILike(EF.Functions.Unaccent(m.Description), pattern)));
        }
        return await Project(q.OrderBy(m => m.Id)).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<MembershipDto> Get(string id) =>
        await Project(_db.Memberships.Where(m => m.Id == id.ToUpperInvariant())).FirstOrDefaultAsync() ?? throw new NotFoundException("Không tìm thấy mã thẻ");

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<MembershipDto>> Create(MembershipInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Id)) throw new ApiException("Vui lòng điền mã thẻ");
        var created = input.CreatedDate.Date;
        var expired = input.ExpiredDate?.Date;
        if (expired.HasValue && expired.Value <= created) throw new ApiException("Hạn dùng phải sau ngày tạo.");
        if (!await _db.MemberShipGroups.AnyAsync(g => g.Id == input.GroupId)) throw new ApiException("Nhóm thẻ không hợp lệ");
        var id = input.Id.Trim().ToUpperInvariant();
        if (id.Length > 30) throw new ApiException("Mã thẻ tối đa 30 ký tự");
        if (await _db.Memberships.AnyAsync(m => m.Id == id)) throw new ApiException("Mã thẻ đã được sử dụng");
        _db.Memberships.Add(new Membership { Id = id, GroupId = input.GroupId, CreatedDate = created, ExpiredDate = expired, Description = input.Description });
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id }, await Get(id));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(string id, MembershipUpdateInput input)
    {
        var m = await _db.Memberships.FindAsync(id.ToUpperInvariant()) ?? throw new NotFoundException("Không tìm thấy mã thẻ");
        var created = input.CreatedDate.Date;
        var expired = input.ExpiredDate?.Date;
        if (expired.HasValue && expired.Value < created) throw new ApiException("Hạn dùng phải sau ngày tạo.");
        m.CreatedDate = created; m.ExpiredDate = expired; m.Description = input.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var m = await _db.Memberships.FindAsync(id.ToUpperInvariant()) ?? throw new NotFoundException("Không tìm thấy mã thẻ");
        if (await _db.Prescriptions.AnyAsync(p => p.MembershipId == m.Id)) throw new ApiException("Không thể xóa giá trị này, có thể nó đang được sử dụng.");
        _db.Memberships.Remove(m);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
