using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>Tồn kho tài sản / vật tư theo chi nhánh (legacy EquipmentManagerControl with ShowBranch; EquimentInBranch).</summary>
[ApiController]
[Route("api/equipment-stock")]
[Authorize]
public class EquipmentStockController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public EquipmentStockController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<EquipmentStockDto> Query(int scope, bool? isVarious, string? keyword, string searchBy, bool onlyExisting)
    {
        var q = _db.Equipments.AsQueryable();
        if (isVarious is not null) q = q.Where(e => e.IsVarious == isVarious.Value);
        q = searchBy switch
        {
            "serial" => q.WhereUnaccentLike(e => e.Serial, keyword),
            "usage" => q.WhereUnaccentLike(e => e.Usage, keyword),
            _ => q.WhereUnaccentLike(e => e.Name, keyword),
        };
        var projected = scope == 0
            ? q.Select(e => new EquipmentStockDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.IsVarious ? "Hao mòn" : "Cố định", e.Vendor,
                e.Position, e.Usage, e.Note, 0, _db.EquimentInBranches.Where(s => s.EquipmentId == e.Id).Sum(s => (int?)s.Quantity) ?? 0))
            : q.Select(e => new EquipmentStockDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.IsVarious ? "Hao mòn" : "Cố định", e.Vendor,
                e.Position, e.Usage, e.Note, scope, _db.EquimentInBranches.Where(s => s.EquipmentId == e.Id && s.BranchId == scope).Sum(s => (int?)s.Quantity) ?? 0));
        if (onlyExisting) projected = projected.Where(x => x.Quantity > 0);
        return projected;
    }

    /// <summary>
    /// Stock list. isVarious: false = Tài sản, true = Vật tư. onlyExisting mirrors the legacy "chỉ hiện có trong kho" checkbox
    /// (unchecked = all catalog items with their quantity).
    /// </summary>
    [HttpGet]
    public async Task<List<EquipmentStockDto>> List(int? branchId, bool? isVarious, string? keyword, string searchBy = "name", bool onlyExisting = true, int limit = 500)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        return await Query(scope, isVarious, keyword, searchBy, onlyExisting).OrderBy(x => x.Name).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
    }

    /// <summary>Set the stock quantity of an equipment in a branch (legacy inline edit of the Quantity column).</summary>
    [HttpPut("{equipmentId:int}")]
    public async Task<EquipmentStockDto> Adjust(int equipmentId, StockAdjustInput input)
    {
        var branchId = InventoryHelpers.RequireBranch(_user, input.BranchId);
        if (input.Quantity < 0) throw new ApiException("Số lượng không được âm");
        var e = await _db.Equipments.FindAsync(equipmentId) ?? throw new NotFoundException("Không tìm thấy thiết bị");
        var row = await _db.EquimentInBranches.FindAsync(branchId, equipmentId);
        if (row is null) _db.EquimentInBranches.Add(new EquimentInBranch { BranchId = branchId, EquipmentId = equipmentId, Quantity = input.Quantity });
        else row.Quantity = input.Quantity;
        await _db.SaveChangesAsync();
        return new EquipmentStockDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, InventoryHelpers.EquipmentType(e.IsVarious), e.Vendor,
            e.Position, e.Usage, e.Note, branchId, input.Quantity);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, bool? isVarious, string? keyword, string searchBy = "name", bool onlyExisting = true)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var rows = await Query(scope, isVarious, keyword, searchBy, onlyExisting).OrderBy(x => x.Name).ToListAsync();
        var bytes = ExcelExporter.Build("Ton kho thiet bi", rows, new ExcelExporter.Column<EquipmentStockDto>[]
        {
            new("Tên", r => r.Name),
            new("Serial", r => r.Serial),
            new("Loại", r => r.EquipmentType),
            new("Nhà cung cấp", r => r.Vendor),
            new("Đơn vị", r => r.Unit),
            new("Giá", r => r.Price),
            new("Tồn kho", r => r.Quantity),
            new("Vị trí", r => r.Position),
            new("Công dụng", r => r.Usage),
            new("Ghi chú", r => r.Note),
        }, isVarious == true ? "Tồn kho vật tư" : isVarious == false ? "Tồn kho tài sản" : "Tồn kho thiết bị");
        return File(bytes, ExcelExporter.ContentType, "ton-kho-thiet-bi.xlsx");
    }

    [HttpGet("lookup/branches")]
    public async Task<List<object>> LookupBranches() =>
        await _db.Branches.OrderBy(b => b.Id).Select(b => (object)new { b.Id, b.Name }).ToListAsync();
}
