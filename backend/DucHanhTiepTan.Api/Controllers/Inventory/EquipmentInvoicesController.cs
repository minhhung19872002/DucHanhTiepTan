using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>Hóa đơn nhập tài sản / vật tư (legacy DeclareEquipmentInvoiceDialog + EquipmentInvoiceManagerControl).</summary>
[ApiController]
[Route("api/equipment-invoices")]
[Authorize]
public class EquipmentInvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public EquipmentInvoicesController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<EquipmentInvoiceView> Query(int? branchId, DateTime? from, DateTime? to, bool? isVarious)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var (f, t) = InventoryHelpers.Range(from, to);
        var q = _db.EquipmentInvoiceViews.AsQueryable();
        if (scope != 0) q = q.Where(r => r.BranchId == scope);
        if (f is not null) q = q.Where(r => r.CreatedDate >= f && r.CreatedDate < t);
        if (isVarious is not null)
        {
            // An invoice is "tài sản" or "vật tư" by the kind of items it contains (the dialogs only allow one kind).
            var v = isVarious.Value;
            q = q.Where(r => _db.ContainedEquipmentInvoices.Any(c => c.InvoiceId == r.Id && c.Equipment!.IsVarious == v));
        }
        return q;
    }

    private static EquipmentInvoiceDto Map(EquipmentInvoiceView r) => new(r.Id, r.CreatedDate, r.BranchId, r.BranchName, r.Quantity, r.Total);

    [HttpGet]
    public async Task<ListResult<EquipmentInvoiceDto>> List(int? branchId, DateTime? from, DateTime? to, bool? isVarious, int limit = 200)
    {
        var q = Query(branchId, from, to, isVarious);
        var total = await q.SumAsync(r => (long?)r.Total) ?? 0;
        var count = await q.CountAsync();
        var items = await q.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
        return new ListResult<EquipmentInvoiceDto>(items.Select(Map).ToList(), total, count);
    }

    [HttpGet("{id:int}")]
    public async Task<EquipmentInvoiceDetailDto> Get(int id)
    {
        var inv = await _db.EquipmentInvoiceViews.FirstOrDefaultAsync(r => r.Id == id) ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        var lines = await _db.ContainedEquipmentInvoices.Where(c => c.InvoiceId == id)
            .Select(c => new EquipmentInvoiceLineDto(c.EquipmentId, c.Equipment!.Name, c.Equipment.Serial,
                c.Equipment.IsVarious ? "Hao mòn" : "Cố định", c.Equipment.Vendor, c.Equipment.Unit, c.Price, c.Quantity, c.Price * c.Quantity))
            .ToListAsync();
        return new EquipmentInvoiceDetailDto(Map(inv), lines);
    }

    /// <summary>Create an import invoice; increases EquimentInBranch stock of the branch.</summary>
    [HttpPost]
    public async Task<ActionResult<EquipmentInvoiceDetailDto>> Create(CreateInvoiceInput input)
    {
        InventoryHelpers.ValidateLines(input.Lines, l => l.ItemId, l => l.Quantity, l => l.Price);
        var branchId = InventoryHelpers.RequireBranch(_user, input.BranchId);
        var ids = input.Lines.Select(l => l.ItemId).ToList();
        var known = await _db.Equipments.Where(e => ids.Contains(e.Id)).Select(e => e.Id).ToListAsync();
        if (known.Count != ids.Count) throw new ApiException("Có thiết bị không tồn tại trong danh mục");

        var invoice = new EquipmentInvoice { CreatedDate = (input.CreatedDate ?? DateTime.Now).Date, BranchId = branchId };
        foreach (var l in input.Lines)
        {
            invoice.ContainedEquipmentInvoices.Add(new ContainedEquipmentInvoice { EquipmentId = l.ItemId, Quantity = l.Quantity, Price = l.Price });
            await InventoryHelpers.AdjustEquipmentStockAsync(_db, branchId, l.ItemId, l.Quantity, validate: false);
        }
        _db.EquipmentInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, await Get(invoice.Id));
    }

    /// <summary>Delete an invoice and revert the stock it added (admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.EquipmentInvoices.Include(i => i.ContainedEquipmentInvoices).FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        foreach (var l in invoice.ContainedEquipmentInvoices)
            await InventoryHelpers.AdjustEquipmentStockAsync(_db, invoice.BranchId, l.EquipmentId, -l.Quantity, validate: true);
        _db.ContainedEquipmentInvoices.RemoveRange(invoice.ContainedEquipmentInvoices);
        _db.EquipmentInvoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, DateTime? from, DateTime? to, bool? isVarious)
    {
        var rows = await Query(branchId, from, to, isVarious).OrderByDescending(r => r.CreatedDate).ToListAsync();
        var bytes = ExcelExporter.Build("Hoa don nhap thiet bi", rows, new ExcelExporter.Column<EquipmentInvoiceView>[]
        {
            new("Số HĐ", r => r.Id),
            new("Ngày", r => r.CreatedDate),
            new("Chi nhánh", r => r.BranchName),
            new("Số mặt hàng", r => r.Quantity),
            new("Tổng tiền", r => r.Total),
        }, "Hóa đơn nhập thiết bị");
        return File(bytes, ExcelExporter.ContentType, "hoa-don-nhap-thiet-bi.xlsx");
    }

    /// <summary>Equipment lookup for the line editor (isVarious: false = tài sản, true = vật tư).</summary>
    [HttpGet("lookup/equipment")]
    public async Task<List<EquipmentLookupDto>> LookupEquipment(bool? isVarious, string? keyword, int? branchId)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.Equipments.AsQueryable();
        if (isVarious is not null) q = q.Where(e => e.IsVarious == isVarious.Value);
        q = q.WhereUnaccentLike(e => e.Name, keyword);
        return await q.OrderBy(e => e.Name).Select(e => new EquipmentLookupDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.Vendor,
                e.Position, e.Usage, e.Note,
                scope == 0 ? (_db.EquimentInBranches.Where(s => s.EquipmentId == e.Id).Sum(s => (int?)s.Quantity) ?? 0)
                           : (_db.EquimentInBranches.Where(s => s.EquipmentId == e.Id && s.BranchId == scope).Sum(s => (int?)s.Quantity) ?? 0)))
            .ToListAsync();
    }
}
