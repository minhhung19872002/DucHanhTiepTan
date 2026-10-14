using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>Tồn kho thuốc theo chi nhánh (DrugInBranch).</summary>
[ApiController]
[Route("api/drug-stock")]
[Authorize]
public class DrugStockController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public DrugStockController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<DrugStockDto> Query(int scope, string? keyword, bool onlyExisting)
    {
        var drugs = _db.Drugs.WhereUnaccentLike(d => d.Name, keyword);
        var q = drugs.Select(d => new
        {
            Drug = d,
            Quantity = scope == 0
                ? (_db.DrugInBranches.Where(s => s.DrugId == d.Id).Sum(s => (int?)s.Quantity) ?? 0)
                : (_db.DrugInBranches.Where(s => s.DrugId == d.Id && s.BranchId == scope).Sum(s => (int?)s.Quantity) ?? 0),
        });
        if (onlyExisting) q = q.Where(x => x.Quantity > 0);
        return q.OrderBy(x => x.Drug.Name)
            .Select(x => new DrugStockDto(x.Drug.Id, x.Drug.Name, x.Drug.Unit, x.Drug.Price, x.Drug.OutPrice, x.Drug.Description, scope, x.Quantity));
    }

    /// <summary>Stock per drug for the selected branch (0 = sum of all branches, admin only).</summary>
    [HttpGet]
    public async Task<List<DrugStockDto>> List(int? branchId, string? keyword, bool onlyExisting = false, int limit = 500)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        return await Query(scope, keyword, onlyExisting).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
    }

    /// <summary>Set the stock quantity of a drug in a branch (manual adjustment).</summary>
    [HttpPut("{drugId:int}")]
    public async Task<DrugStockDto> Adjust(int drugId, StockAdjustInput input)
    {
        var branchId = InventoryHelpers.RequireBranch(_user, input.BranchId);
        if (input.Quantity < 0) throw new ApiException("Số lượng không được âm");
        var drug = await _db.Drugs.FindAsync(drugId) ?? throw new NotFoundException("Không tìm thấy thuốc");
        var row = await _db.DrugInBranches.FindAsync(branchId, drugId);
        if (row is null) _db.DrugInBranches.Add(new DrugInBranch { BranchId = branchId, DrugId = drugId, Quantity = input.Quantity });
        else row.Quantity = input.Quantity;
        await _db.SaveChangesAsync();
        return new DrugStockDto(drug.Id, drug.Name, drug.Unit, drug.Price, drug.OutPrice, drug.Description, branchId, input.Quantity);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, string? keyword, bool onlyExisting = false)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var rows = await Query(scope, keyword, onlyExisting).ToListAsync();
        var bytes = ExcelExporter.Build("Ton kho thuoc", rows, new ExcelExporter.Column<DrugStockDto>[]
        {
            new("Thuốc", r => r.Name),
            new("Đơn vị", r => r.Unit),
            new("Giá nhập", r => r.Price),
            new("Giá bán", r => r.OutPrice),
            new("Tồn kho", r => r.Quantity),
            new("Mô tả", r => r.Description),
        }, "Tồn kho thuốc");
        return File(bytes, ExcelExporter.ContentType, "ton-kho-thuoc.xlsx");
    }

    [HttpGet("lookup/drugs")]
    public async Task<List<DrugLookupDto>> LookupDrugs(string? keyword) =>
        await _db.Drugs.WhereUnaccentLike(d => d.Name, keyword).OrderBy(d => d.Name)
            .Select(d => new DrugLookupDto(d.Id, d.Name, d.Unit, d.Price, d.OutPrice, d.Description)).ToListAsync();

    [HttpGet("lookup/branches")]
    public async Task<List<object>> LookupBranches() =>
        await _db.Branches.OrderBy(b => b.Id).Select(b => (object)new { b.Id, b.Name }).ToListAsync();
}
