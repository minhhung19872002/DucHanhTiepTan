using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Inventory;

/// <summary>
/// Hóa đơn xuất thuốc: UsedReceipt/ContainedUsedReceipt created while examining a patient (drugs sold).
/// The legacy "Các hóa đơn xuất" button had no implementation; this lists those receipts read-only.
/// </summary>
[ApiController]
[Route("api/drug-receipts")]
[Authorize]
public class DrugReceiptsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public DrugReceiptsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<DrugReceiptDto> Query(int? branchId, DateTime? from, DateTime? to, string? keyword, long? prescriptionId = null, int? appointmentNo = null)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var (f, t) = InventoryHelpers.Range(from, to);
        var q = from r in _db.UsedReceipts
                join p in _db.Prescriptions on r.PrescriptionId equals p.Id
                join c in _db.Customers on p.CustomerId equals c.Id
                join b in _db.Branches on c.BranchId equals b.Id
                select new { r, c, b };
        if (scope != 0) q = q.Where(x => x.c.BranchId == scope);
        if (f is not null) q = q.Where(x => x.r.CreatedDate >= f && x.r.CreatedDate < t);
        if (prescriptionId is not null) q = q.Where(x => x.r.PrescriptionId == prescriptionId && x.r.AppointmentNo == appointmentNo);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = "%" + VietnameseText.Normalize(keyword) + "%";
            q = q.Where(x => EF.Functions.ILike(EF.Functions.Unaccent(x.c.Name), pattern) || (x.c.Phone != null && x.c.Phone.Contains(keyword)));
        }
        return q.OrderByDescending(x => x.r.CreatedDate).ThenByDescending(x => x.r.PrescriptionId).Select(x => new DrugReceiptDto(
            x.r.PrescriptionId, x.r.AppointmentNo, x.r.CreatedDate, x.c.Id, x.c.Name, x.c.VisiableId, x.b.Id, x.b.Name,
            _db.ContainedUsedReceipts.Count(l => l.PrescriptionId == x.r.PrescriptionId && l.AppointmentNo == x.r.AppointmentNo),
            _db.ContainedUsedReceipts.Where(l => l.PrescriptionId == x.r.PrescriptionId && l.AppointmentNo == x.r.AppointmentNo).Sum(l => (long?)(l.Price * l.Quantity)) ?? 0));
    }

    [HttpGet]
    public async Task<ListResult<DrugReceiptDto>> List(int? branchId, DateTime? from, DateTime? to, string? keyword, int limit = 200)
    {
        var q = Query(branchId, from, to, keyword);
        var count = await q.CountAsync();
        var items = await q.Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
        return new ListResult<DrugReceiptDto>(items, items.Sum(i => i.Total), count);
    }

    [HttpGet("{prescriptionId:long}/{appointmentNo:int}")]
    public async Task<DrugReceiptDetailDto> Get(long prescriptionId, int appointmentNo)
    {
        var receipt = await Query(null, null, null, null, prescriptionId, appointmentNo).FirstOrDefaultAsync()
            ?? throw new NotFoundException("Không tìm thấy hóa đơn xuất");
        var lines = await _db.ContainedUsedReceipts.Where(l => l.PrescriptionId == prescriptionId && l.AppointmentNo == appointmentNo)
            .Select(l => new DrugReceiptLineDto(l.DrugId, l.Drug!.Name, l.Drug.Unit, l.Price, l.Quantity, l.Price * l.Quantity)).ToListAsync();
        return new DrugReceiptDetailDto(receipt, lines);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int? branchId, DateTime? from, DateTime? to, string? keyword)
    {
        var rows = await Query(branchId, from, to, keyword).ToListAsync();
        var bytes = ExcelExporter.Build("Hoa don xuat thuoc", rows, new ExcelExporter.Column<DrugReceiptDto>[]
        {
            new("Phiếu khám", r => r.PrescriptionId),
            new("Lần khám", r => r.AppointmentNo),
            new("Ngày", r => r.CreatedDate),
            new("Mã KH", r => r.CustomerVisiableId),
            new("Khách hàng", r => r.CustomerName),
            new("Chi nhánh", r => r.BranchName),
            new("Số mặt hàng", r => r.Quantity),
            new("Tổng tiền", r => r.Total),
        }, "Hóa đơn xuất thuốc");
        return File(bytes, ExcelExporter.ContentType, "hoa-don-xuat-thuoc.xlsx");
    }
}
