using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Reports;

/// <summary>
/// Key/value table "Configuration" (legacy Configuration.Get/GetLong/GetInt): BaseSalary, WorkDaysInMonth, CustomerFormat, ...
/// </summary>
[ApiController]
[Route("api/configuration")]
[Authorize(Policy = "Admin")]
public class ConfigurationController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConfigurationController(AppDbContext db) => _db = db;

    public record ConfigurationDto(string Name, string? Value);

    /// <summary>Well-known keys shown with a label in the UI.</summary>
    public static readonly Dictionary<string, string> KnownKeys = new()
    {
        ["BaseSalary"] = "Lương cơ bản mặc định",
        ["WorkDaysInMonth"] = "Số ngày công chuẩn trong tháng",
        ["CustomerFormat"] = "Định dạng mã khách hàng",
    };

    [HttpGet]
    public async Task<List<ConfigurationDto>> List() =>
        await _db.Configurations.OrderBy(c => c.Name).Select(c => new ConfigurationDto(c.Name, c.Value)).ToListAsync();

    [HttpGet("{name}")]
    public async Task<ActionResult<ConfigurationDto>> Get(string name)
    {
        var c = await _db.Configurations.FindAsync(name);
        if (c is null) throw new NotFoundException("Không tìm thấy cấu hình " + name);
        return new ConfigurationDto(c.Name, c.Value);
    }

    /// <summary>Upsert one key.</summary>
    [HttpPut("{name}")]
    public async Task<ConfigurationDto> Set(string name, ConfigurationDto input)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ApiException("Tên cấu hình không được để trống");
        var c = await _db.Configurations.FindAsync(name);
        if (c is null)
        {
            c = new Configuration { Name = name };
            _db.Configurations.Add(c);
        }
        c.Value = input.Value;
        await _db.SaveChangesAsync();
        return new ConfigurationDto(c.Name, c.Value);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        var c = await _db.Configurations.FindAsync(name) ?? throw new NotFoundException("Không tìm thấy cấu hình " + name);
        _db.Configurations.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
