using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>Drug catalog ("Dữ liệu thuốc") – legacy DrugManagerControl / DeclareDrug.</summary>
[ApiController]
[Route("api/drugs")]
[Authorize]
public class DrugsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DrugsController(AppDbContext db) => _db = db;

    public record DrugDto(int Id, string Name, string Unit, long Price, long OutPrice, string? Description);
    public record DrugInput(string Name, string Unit, long Price, long? OutPrice, string? Description, bool FixedOutPrice = false);

    private static DrugDto ToDto(Drug d) => new(d.Id, d.Name, d.Unit, d.Price, d.OutPrice, d.Description);

    /// <summary>GET api/drugs?keyword=&amp;fromPrice=&amp;toPrice=&amp;limit=</summary>
    [HttpGet]
    public async Task<List<DrugDto>> List([FromQuery] string? keyword, [FromQuery] long? fromPrice, [FromQuery] long? toPrice, [FromQuery] int limit = 0)
    {
        var q = _db.Drugs.AsNoTracking().AsQueryable();
        if (fromPrice.HasValue) q = q.Where(d => d.Price >= fromPrice.Value);
        if (toPrice.HasValue) q = q.Where(d => d.Price <= toPrice.Value);
        q = q.WhereUnaccentLike(d => d.Name, keyword).OrderBy(d => d.Name);
        if (limit > 0) q = q.Take(limit);
        return await q.Select(d => new DrugDto(d.Id, d.Name, d.Unit, d.Price, d.OutPrice, d.Description)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DrugDto>> Get(int id)
    {
        var d = await _db.Drugs.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy thuốc");
        return ToDto(d);
    }

    [HttpPost]
    public async Task<ActionResult<DrugDto>> Create(DrugInput input)
    {
        Validate(input);
        var d = new Drug();
        Apply(d, input);
        _db.Drugs.Add(d);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = d.Id }, ToDto(d));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DrugDto>> Update(int id, DrugInput input)
    {
        Validate(input);
        var d = await _db.Drugs.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy thuốc");
        Apply(d, input);
        await _db.SaveChangesAsync();
        return ToDto(d);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Drugs.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy thuốc");
        var inUse = await _db.ContainedInvoices.AnyAsync(x => x.DrugId == id)
            || await _db.ContainedReceipts.AnyAsync(x => x.DrugId == id)
            || await _db.ContainedUsedReceipts.AnyAsync(x => x.DrugId == id)
            || await _db.DrugInBranches.AnyAsync(x => x.DrugId == id && x.Quantity != 0);
        if (inUse) throw new ApiException("Không thể xóa thuốc này được, có thể nó đang được sử dụng.");
        _db.DrugInBranches.RemoveRange(_db.DrugInBranches.Where(x => x.DrugId == id));
        _db.Drugs.Remove(d);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Validate(DrugInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên thuốc.");
        if (string.IsNullOrWhiteSpace(input.Unit)) throw new ApiException("Vui lòng điền đơn vị tính của thuốc.");
        if (input.Price < 0) throw new ApiException("Giá nhập không hợp lệ.");
    }

    private static void Apply(Drug d, DrugInput input)
    {
        d.Name = input.Name.Trim();
        d.Unit = input.Unit.Trim();
        d.Price = input.Price;
        // Legacy dialog: "Bằng 120% của giá nhập" option
        d.OutPrice = input.FixedOutPrice || input.OutPrice is null ? (long)(input.Price * 1.2) : input.OutPrice.Value;
        d.Description = input.Description;
    }
}
