using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>Labo types ("Chủng loại Labo") – legacy LaboManagerControl. Disassembly: true = "Tháo lắp", false = "Cố định".</summary>
[ApiController]
[Route("api/labo-types")]
[Authorize]
public class LaboTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public LaboTypesController(AppDbContext db) => _db = db;

    public record LaboTypeDto(int Id, string Name, string? Description, bool Disassembly);
    public record LaboTypeInput(string Name, string? Description, bool Disassembly);

    [HttpGet]
    public async Task<List<LaboTypeDto>> List([FromQuery] string? keyword) =>
        await _db.LaboTypes.AsNoTracking().WhereUnaccentLike(t => t.Name, keyword).OrderBy(t => t.Name)
            .Select(t => new LaboTypeDto(t.Id, t.Name, t.Description, t.Disassembly)).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<LaboTypeDto>> Create(LaboTypeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên chủng loại Labo.");
        var t = new LaboType { Name = input.Name.Trim(), Description = input.Description, Disassembly = input.Disassembly };
        _db.LaboTypes.Add(t);
        await _db.SaveChangesAsync();
        return new LaboTypeDto(t.Id, t.Name, t.Description, t.Disassembly);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LaboTypeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên chủng loại Labo.");
        var t = await _db.LaboTypes.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy chủng loại Labo");
        t.Name = input.Name.Trim(); t.Description = input.Description; t.Disassembly = input.Disassembly;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.LaboTypes.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy chủng loại Labo");
        var inUse = await _db.ContainedLabos.AnyAsync(x => x.LaboTypeId == id) || await _db.ContainedConsultingLabos.AnyAsync(x => x.LaboTypeId == id);
        if (inUse) throw new ApiException("Không thể xóa giá trị này, có thể nó đang được sử dụng.");
        _db.LaboTypes.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>Labo vendors ("Nhà cung cấp Labo").</summary>
[ApiController]
[Route("api/labo-vendors")]
[Authorize]
public class LaboVendorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LaboVendorsController(AppDbContext db) => _db = db;

    public record LaboVendorDto(int Id, string Name, string? Address, string? Tel, string? Description);
    public record LaboVendorInput(string Name, string? Address, string? Tel, string? Description);

    [HttpGet]
    public async Task<List<LaboVendorDto>> List([FromQuery] string? keyword) =>
        await _db.LaboVendors.AsNoTracking().WhereUnaccentLike(v => v.Name, keyword).OrderBy(v => v.Name)
            .Select(v => new LaboVendorDto(v.Id, v.Name, v.Address, v.Tel, v.Description)).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<LaboVendorDto>> Create(LaboVendorInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên nhà cung cấp Labo.");
        var v = new LaboVendor { Name = input.Name.Trim(), Address = input.Address, Tel = input.Tel, Description = input.Description };
        _db.LaboVendors.Add(v);
        await _db.SaveChangesAsync();
        return new LaboVendorDto(v.Id, v.Name, v.Address, v.Tel, v.Description);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LaboVendorInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên nhà cung cấp Labo.");
        var v = await _db.LaboVendors.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy nhà cung cấp Labo");
        v.Name = input.Name.Trim(); v.Address = input.Address; v.Tel = input.Tel; v.Description = input.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.LaboVendors.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy nhà cung cấp Labo");
        var inUse = await _db.ContainedLabos.AnyAsync(x => x.LaboVendorId == id) || await _db.ContainedConsultingLabos.AnyAsync(x => x.LaboVendorId == id);
        if (inUse) throw new ApiException("Không thể xóa giá trị này, có thể nó đang được sử dụng.");
        _db.LaboVendors.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
