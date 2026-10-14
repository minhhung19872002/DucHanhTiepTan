using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>Hóa đơn nhập thuốc (legacy DeclareInvoiceDialog + InvoiceManagerControl).</summary>
[ApiController]
[Route("api/drug-invoices")]
[Authorize]
public class DrugInvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public DrugInvoicesController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<InvoiceView> Query(int? branchId, DateTime? from, DateTime? to)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var (f, t) = InventoryHelpers.Range(from, to);
        var q = _db.InvoiceViews.AsQueryable();
        if (scope != 0) q = q.Where(r => r.BranchId == scope);
        if (f is not null) q = q.Where(r => r.CreatedDate >= f && r.CreatedDate < t);
        return q;
    }

    private static DrugInvoiceDto Map(InvoiceView r) => new(r.Id, r.CreatedDate, r.UserName, r.FullName, r.BranchId, r.BranchName, r.Quantity, r.Total);

    /// <summary>List invoices. Legacy filter: all / day / month / range + row limit; header shows the grand total.</summary>
    [HttpGet]
    public async Task<ListResult<DrugInvoiceDto>> List(int? branchId, DateTime? from, DateTime? to, int limit = 200)
    {
        var q = Query(branchId, from, to);
        var total = await q.SumAsync(r => (long?)r.Total) ?? 0;
        var count = await q.CountAsync();
        var items = await q.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
        return new ListResult<DrugInvoiceDto>(items.Select(Map).ToList(), total, count);
    }

    [HttpGet("{id:int}")]
    public async Task<DrugInvoiceDetailDto> Get(int id)
    {
        var inv = await _db.InvoiceViews.FirstOrDefaultAsync(r => r.Id == id) ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        if (!BranchScope.Matches(BranchScope.Resolve(_user, null), inv.BranchId) && !_user.IsAdministrator) throw new NotFoundException("Không tìm thấy hóa đơn");
        var lines = await _db.ContainedInvoices.Where(c => c.InvoiceId == id)
            .Select(c => new DrugInvoiceLineDto(c.DrugId, c.Drug!.Name, c.Drug.Unit, c.Price, c.Quantity, c.Price * c.Quantity))
            .ToListAsync();
        return new DrugInvoiceDetailDto(Map(inv), lines);
    }

    /// <summary>Create an import invoice; increases DrugInBranch stock of the branch.</summary>
    [HttpPost]
    public async Task<ActionResult<DrugInvoiceDetailDto>> Create(CreateInvoiceInput input)
    {
        InventoryHelpers.ValidateLines(input.Lines, l => l.ItemId, l => l.Quantity, l => l.Price);
        var branchId = InventoryHelpers.RequireBranch(_user, input.BranchId);
        var drugIds = input.Lines.Select(l => l.ItemId).ToList();
        var known = await _db.Drugs.Where(d => drugIds.Contains(d.Id)).Select(d => d.Id).ToListAsync();
        if (known.Count != drugIds.Count) throw new ApiException("Có thuốc không tồn tại trong danh mục");

        var invoice = new Invoice
        {
            CreatedDate = (input.CreatedDate ?? DateTime.Now).Date,
            UserName = _user.UserName,
            BranchId = branchId,
        };
        foreach (var l in input.Lines)
        {
            invoice.ContainedInvoices.Add(new ContainedInvoice { DrugId = l.ItemId, Quantity = l.Quantity, Price = l.Price });
            await InventoryHelpers.AdjustDrugStockAsync(_db, branchId, l.ItemId, l.Quantity);
        }
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, await Get(invoice.Id));
    }

    /// <summary>Delete an invoice and revert the stock it added (admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Invoices.Include(i => i.ContainedInvoices).FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        foreach (var l in invoice.ContainedInvoices)
            await InventoryHelpers.AdjustDrugStockAsync(_db, invoice.BranchId, l.DrugId, -l.Quantity);
        _db.ContainedInvoices.RemoveRange(invoice.ContainedInvoices);
        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, DateTime? from, DateTime? to)
    {
        var rows = await Query(branchId, from, to).OrderByDescending(r => r.CreatedDate).ToListAsync();
        var bytes = ExcelExporter.Build("Hoa don nhap thuoc", rows, new ExcelExporter.Column<InvoiceView>[]
        {
            new("Số HĐ", r => r.Id),
            new("Ngày", r => r.CreatedDate),
            new("Chi nhánh", r => r.BranchName),
            new("Người lập", r => r.FullName ?? r.UserName),
            new("Số mặt hàng", r => r.Quantity),
            new("Tổng tiền", r => r.Total),
        }, "Hóa đơn nhập thuốc");
        return File(bytes, ExcelExporter.ContentType, "hoa-don-nhap-thuoc.xlsx");
    }

    [HttpGet("lookup/drugs")]
    public async Task<List<DrugLookupDto>> LookupDrugs(string? keyword)
    {
        var q = _db.Drugs.WhereUnaccentLike(d => d.Name, keyword);
        return await q.OrderBy(d => d.Name).Select(d => new DrugLookupDto(d.Id, d.Name, d.Unit, d.Price, d.OutPrice, d.Description)).ToListAsync();
    }
}
