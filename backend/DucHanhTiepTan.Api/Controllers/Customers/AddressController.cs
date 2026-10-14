using DucHanhTiepTan.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Customers;

/// <summary>Tỉnh / Huyện / Xã lookups (port of ProvinceCollector over the ADDRESS database).</summary>
[ApiController]
[Route("api/address")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly AppDbContext _db;

    public AddressController(AppDbContext db) => _db = db;

    [HttpGet("provinces")]
    public async Task<List<string>> Provinces() =>
        await _db.Provinces.AsNoTracking().OrderBy(p => p.Name).Select(p => p.Name).ToListAsync();

    [HttpGet("districts")]
    public async Task<List<string>> Districts([FromQuery] string province) =>
        await _db.Districts.AsNoTracking().Where(d => d.ProvinceName == province).OrderBy(d => d.Name).Select(d => d.Name).ToListAsync();

    [HttpGet("wards")]
    public async Task<List<string>> Wards([FromQuery] string province, [FromQuery] string district) =>
        await _db.Wards.AsNoTracking().Where(w => w.ProvinceName == province && w.DistrictName == district).OrderBy(w => w.Name).Select(w => w.Name).ToListAsync();
}
