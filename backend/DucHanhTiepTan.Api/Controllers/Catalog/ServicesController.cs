using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>Services ("Dịch vụ") – legacy ServiceManagerControl / DeclareServiceDialog.</summary>
[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServicesController(AppDbContext db) => _db = db;

    public record ServiceDto(int Id, string Name, long Price, string? Description, int GroupId, string? GroupName);
    public record ServiceInput(string Name, long Price, string? Description, int GroupId);

    /// <summary>GET api/services?keyword=&amp;groupId=&amp;fromPrice=&amp;toPrice=</summary>
    [HttpGet]
    public async Task<List<ServiceDto>> List([FromQuery] string? keyword, [FromQuery] int? groupId, [FromQuery] long? fromPrice, [FromQuery] long? toPrice, [FromQuery] int limit = 0)
    {
        var q = _db.Services.AsNoTracking().AsQueryable();
        if (groupId.HasValue) q = q.Where(s => s.GroupId == groupId.Value);
        if (fromPrice.HasValue) q = q.Where(s => s.Price >= fromPrice.Value);
        if (toPrice.HasValue) q = q.Where(s => s.Price <= toPrice.Value);
        q = q.WhereUnaccentLike(s => s.Name, keyword).OrderBy(s => s.Name);
        if (limit > 0) q = q.Take(limit);
        return await q.Select(s => new ServiceDto(s.Id, s.Name, s.Price, s.Description, s.GroupId, s.Group!.Name)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceDto>> Get(int id)
    {
        var s = await _db.Services.Include(x => x.Group).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy dịch vụ");
        return new ServiceDto(s.Id, s.Name, s.Price, s.Description, s.GroupId, s.Group?.Name);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create(ServiceInput input)
    {
        await ValidateAsync(input);
        var s = new Service { Name = input.Name.Trim(), Price = input.Price, Description = input.Description, GroupId = input.GroupId };
        _db.Services.Add(s);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = s.Id }, (await Get(s.Id)).Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceDto>> Update(int id, ServiceInput input)
    {
        await ValidateAsync(input);
        var s = await _db.Services.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy dịch vụ");
        s.Name = input.Name.Trim(); s.Price = input.Price; s.Description = input.Description; s.GroupId = input.GroupId;
        await _db.SaveChangesAsync();
        return (await Get(id)).Value!;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Services.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy dịch vụ");
        var inUse = await _db.ContainedTreatments.AnyAsync(x => x.ServiceId == id)
            || await _db.ContainedConsultings.AnyAsync(x => x.ServiceId == id)
            || await _db.ContainedDiscounts.AnyAsync(x => x.ServiceId == id)
            || await _db.ServiceDiscounts.AnyAsync(x => x.ServiceId == id);
        if (inUse) throw new ApiException("Không thể xóa dịch vụ này được, có thể nó đang được sử dụng.");
        _db.Services.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ValidateAsync(ServiceInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên dịch vụ.");
        if (input.Price < 0) throw new ApiException("Giá không hợp lệ.");
        if (!await _db.ServiceGroups.AnyAsync(g => g.Id == input.GroupId)) throw new ApiException("Vui lòng chọn nhóm dịch vụ.");
    }
}

/// <summary>Service groups ("Nhóm dịch vụ"). Groups 1, 2, 3 are built-in and cannot be deleted.</summary>
[ApiController]
[Route("api/service-groups")]
[Authorize]
public class ServiceGroupsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServiceGroupsController(AppDbContext db) => _db = db;

    public record GroupDto(int Id, string? Name, int ServiceCount, bool IsBuiltIn);
    public record GroupInput(string Name);

    private static bool IsBuiltIn(int id) => id is 1 or 2 or 3;

    [HttpGet]
    public async Task<List<GroupDto>> List() =>
        await _db.ServiceGroups.AsNoTracking().OrderBy(g => g.Id)
            .Select(g => new GroupDto(g.Id, g.Name, g.Services.Count, g.Id == 1 || g.Id == 2 || g.Id == 3)).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<GroupDto>> Create(GroupInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên nhóm dịch vụ.");
        var g = new ServiceGroup { Name = input.Name.Trim() };
        _db.ServiceGroups.Add(g);
        await _db.SaveChangesAsync();
        return new GroupDto(g.Id, g.Name, 0, IsBuiltIn(g.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GroupInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên nhóm dịch vụ.");
        var g = await _db.ServiceGroups.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy nhóm dịch vụ");
        g.Name = input.Name.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.ServiceGroups.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy nhóm dịch vụ");
        if (IsBuiltIn(id)) throw new ApiException($"Dịch vụ {g.Name} này là dịch vụ dựng sẵn, không thể xóa được.");
        if (await _db.Services.AnyAsync(s => s.GroupId == id)) throw new ApiException("Không thể xóa nhóm dịch vụ này được, có thể nó đang được sử dụng.");
        _db.ServiceGroups.Remove(g);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
