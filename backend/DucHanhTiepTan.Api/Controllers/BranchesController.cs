using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers;

/// <summary>
/// Branches ("Chi nhánh"). Branch id 0 is the virtual "Tất cả" entry used by the legacy BranchFilter.
/// Reference implementation of the controller conventions used by every module.
/// </summary>
[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public BranchesController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public record BranchDto(int Id, string Name, string? Address, string? Tel);
    public record BranchInput(string Name, string? Address, string? Tel);

    [HttpGet]
    public async Task<List<BranchDto>> List() =>
        await _db.Branches.OrderBy(b => b.Id).Select(b => new BranchDto(b.Id, b.Name, b.Address, b.Tel)).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BranchDto>> Get(int id)
    {
        var b = await _db.Branches.FindAsync(id);
        if (b is null) throw new NotFoundException("Không tìm thấy chi nhánh");
        return new BranchDto(b.Id, b.Name, b.Address, b.Tel);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<BranchDto>> Create(BranchInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Tên chi nhánh không được để trống");
        var b = new Branch { Name = input.Name.Trim(), Address = input.Address, Tel = input.Tel };
        _db.Branches.Add(b);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = b.Id }, new BranchDto(b.Id, b.Name, b.Address, b.Tel));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(int id, BranchInput input)
    {
        if (id == 0) throw new ApiException("Không thể sửa chi nhánh 'Tất cả'");
        var b = await _db.Branches.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy chi nhánh");
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Tên chi nhánh không được để trống");
        b.Name = input.Name.Trim(); b.Address = input.Address; b.Tel = input.Tel;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == 0) throw new ApiException("Không thể xóa chi nhánh 'Tất cả'");
        var b = await _db.Branches.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy chi nhánh");
        var inUse = await _db.Customers.AnyAsync(c => c.BranchId == id) || await _db.Employees.AnyAsync(e => e.BranchId == id) || await _db.Accounts.AnyAsync(a => a.BranchId == id);
        if (inUse) throw new ApiException("Chi nhánh đang được sử dụng, không thể xóa");
        _db.Branches.Remove(b);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
