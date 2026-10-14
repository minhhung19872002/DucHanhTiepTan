using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>Hóa đơn xuất tài sản / vật tư (legacy CreateExportInvoiceDialog + ExportInvoiceManagerControl).</summary>
[ApiController]
[Route("api/equipment-exports")]
[Authorize]
public class EquipmentExportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public EquipmentExportsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<ExportInvoiceView> Query(int? branchId, DateTime? from, DateTime? to, bool? isVarious)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var (f, t) = InventoryHelpers.Range(from, to);
        var q = _db.ExportInvoiceViews.AsQueryable();
        if (scope != 0) q = q.Where(r => r.BranchId == scope);
        if (f is not null) q = q.Where(r => r.CreatedDate >= f && r.CreatedDate < t);
        if (isVarious is not null)
        {
            var v = isVarious.Value;
            q = q.Where(r => _db.ContainedEquipmentExportInvoices.Any(c => c.EquipmentExportInvoiceId == r.Id && c.Equipment!.IsVarious == v));
        }
        return q;
    }

    private static ExportInvoiceDto Map(ExportInvoiceView r) => new(r.Id, r.CreatedDate, r.BranchId, r.BranchName, r.Quantity);

    [HttpGet]
    public async Task<ListResult<ExportInvoiceDto>> List(int? branchId, DateTime? from, DateTime? to, bool? isVarious, int limit = 200)
    {
        var q = Query(branchId, from, to, isVarious);
        var count = await q.CountAsync();
        var items = await q.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
        return new ListResult<ExportInvoiceDto>(items.Select(Map).ToList(), 0, count);
    }

    [HttpGet("{id:int}")]
    public async Task<ExportInvoiceDetailDto> Get(int id)
    {
        var inv = await _db.ExportInvoiceViews.FirstOrDefaultAsync(r => r.Id == id) ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        var lines = await _db.ContainedEquipmentExportInvoices.Where(c => c.EquipmentExportInvoiceId == id)
            .Select(c => new ExportInvoiceLineDto(c.EquipmentId, c.Equipment!.Name, c.Equipment.Serial,
                c.Equipment.IsVarious ? "Hao mòn" : "Cố định", c.Equipment.Vendor, c.Equipment.Unit, c.Quantity))
            .ToListAsync();
        return new ExportInvoiceDetailDto(Map(inv), lines);
    }

    /// <summary>Create an export voucher; decreases EquimentInBranch stock (must not go below zero).</summary>
    [HttpPost]
    public async Task<ActionResult<ExportInvoiceDetailDto>> Create(CreateExportInput input)
    {
        InventoryHelpers.ValidateLines(input.Lines, l => l.EquipmentId, l => l.Quantity, null);
        var branchId = InventoryHelpers.RequireBranch(_user, input.BranchId);
        var ids = input.Lines.Select(l => l.EquipmentId).ToList();
        var known = await _db.Equipments.Where(e => ids.Contains(e.Id)).Select(e => e.Id).ToListAsync();
        if (known.Count != ids.Count) throw new ApiException("Có thiết bị không tồn tại trong danh mục");

        var invoice = new EquipmentExportInvoice { CreatedDate = (input.CreatedDate ?? DateTime.Now).Date, BranchId = branchId };
        foreach (var l in input.Lines)
        {
            invoice.ContainedEquipmentExportInvoices.Add(new ContainedEquipmentExportInvoice { EquipmentId = l.EquipmentId, Quantity = l.Quantity });
            await InventoryHelpers.AdjustEquipmentStockAsync(_db, branchId, l.EquipmentId, -l.Quantity, validate: true);
        }
        _db.EquipmentExportInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, await Get(invoice.Id));
    }

    /// <summary>Delete an export voucher and return the stock (admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.EquipmentExportInvoices.Include(i => i.ContainedEquipmentExportInvoices).FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Không tìm thấy hóa đơn");
        foreach (var l in invoice.ContainedEquipmentExportInvoices)
            await InventoryHelpers.AdjustEquipmentStockAsync(_db, invoice.BranchId, l.EquipmentId, l.Quantity, validate: false);
        _db.ContainedEquipmentExportInvoices.RemoveRange(invoice.ContainedEquipmentExportInvoices);
        _db.EquipmentExportInvoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, DateTime? from, DateTime? to, bool? isVarious)
    {
        var rows = await Query(branchId, from, to, isVarious).OrderByDescending(r => r.CreatedDate).ToListAsync();
        var bytes = ExcelExporter.Build("Hoa don xuat thiet bi", rows, new ExcelExporter.Column<ExportInvoiceView>[]
        {
            new("Số HĐ", r => r.Id),
            new("Ngày", r => r.CreatedDate),
            new("Chi nhánh", r => r.BranchName),
            new("Số mặt hàng", r => r.Quantity),
        }, "Hóa đơn xuất thiết bị");
        return File(bytes, ExcelExporter.ContentType, "hoa-don-xuat-thiet-bi.xlsx");
    }

    /// <summary>Equipment that currently exists in the branch (legacy Equipment.Query(existed: true)), searchable by name/serial/usage.</summary>
    [HttpGet("lookup/equipment")]
    public async Task<List<EquipmentLookupDto>> LookupExisting(bool? isVarious, string? keyword, string searchBy = "name", int? branchId = null)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.Equipments.AsQueryable();
        if (isVarious is not null) q = q.Where(e => e.IsVarious == isVarious.Value);
        q = searchBy switch
        {
            "serial" => q.WhereUnaccentLike(e => e.Serial, keyword),
            "usage" => q.WhereUnaccentLike(e => e.Usage, keyword),
            _ => q.WhereUnaccentLike(e => e.Name, keyword),
        };
        var rows = await q.OrderBy(e => e.Name).Select(e => new EquipmentLookupDto(e.Id, e.Name, e.Serial, e.Unit, e.Price, e.IsVarious, e.Vendor,
                e.Position, e.Usage, e.Note,
                scope == 0 ? (_db.EquimentInBranches.Where(s => s.EquipmentId == e.Id).Sum(s => (int?)s.Quantity) ?? 0)
                           : (_db.EquimentInBranches.Where(s => s.EquipmentId == e.Id && s.BranchId == scope).Sum(s => (int?)s.Quantity) ?? 0)))
            .ToListAsync();
        return rows.Where(r => r.Quantity > 0).ToList();
    }
}
