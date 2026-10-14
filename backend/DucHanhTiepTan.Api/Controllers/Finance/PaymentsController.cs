using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>"Lịch sử thu" (legacy PaymentHistoryControl): payments by date range / branch.</summary>
[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public PaymentsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public record PaymentDto(long Id, DateTime CreatedDate, string BranchName, string CustomerName, long CustomerId, long CustomerVisiableId,
        long Amount, long PrescriptionId, string ReceptionistName, string? ReceptionistFullName);
    public record PaymentListDto(List<PaymentDto> Items, long Sum, int Count);

    private IQueryable<Entities.Payment> Query(int? branchId, DateTime? from, DateTime? to, string? receptionist)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.Payments.AsQueryable();
        if (scope != 0) q = q.Where(r => r.Receptionist!.BranchId == scope);
        if (from.HasValue) { var f = from.Value.Date; q = q.Where(r => r.CreatedDate >= f); }
        if (to.HasValue) { var t = to.Value.Date.AddDays(1); q = q.Where(r => r.CreatedDate < t); }
        if (!string.IsNullOrWhiteSpace(receptionist)) q = q.Where(r => r.ReceptionistName == receptionist);
        return q;
    }

    private static IQueryable<PaymentDto> Project(IQueryable<Entities.Payment> q) => q.Select(p => new PaymentDto(
        p.Id, p.CreatedDate, p.Prescription!.Customer!.Branch!.Name, p.Prescription.Customer.Name, p.Prescription.CustomerId,
        p.Prescription.Customer.VisiableId, p.Amount, p.PrescriptionId, p.ReceptionistName, p.Receptionist!.FullName));

    /// <summary>from/to: yyyy-MM-dd (omit both = all). limit mirrors the legacy "số dòng" spinner.</summary>
    [HttpGet]
    public async Task<PaymentListDto> List([FromQuery] int? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? receptionist, [FromQuery] int limit = 500)
    {
        var q = Query(branchId, from, to, receptionist);
        var sum = await q.SumAsync(r => (long?)r.Amount) ?? 0;
        var count = await q.CountAsync();
        var items = await Project(q.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id).Take(Math.Clamp(limit, 1, 5000))).ToListAsync();
        return new PaymentListDto(items, sum, count);
    }

    /// <summary>Payments of one prescription (used by detail views).</summary>
    [HttpGet("by-prescription/{prescriptionId:long}")]
    public async Task<List<PaymentDto>> ByPrescription(long prescriptionId) =>
        await Project(_db.Payments.Where(r => r.PrescriptionId == prescriptionId).OrderBy(r => r.CreatedDate).ThenBy(r => r.Id)).ToListAsync();

    /// <summary>Receptionist accounts for the filter dropdown.</summary>
    [HttpGet("receptionists")]
    public async Task<List<object>> Receptionists([FromQuery] int? branchId)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        return await _db.Accounts.Where(a => scope == 0 || a.BranchId == scope).OrderBy(a => a.UserName)
            .Select(a => (object)new { a.UserName, a.FullName }).ToListAsync();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? receptionist)
    {
        var items = await Project(Query(branchId, from, to, receptionist).OrderBy(r => r.CreatedDate).ThenBy(r => r.Id)).ToListAsync();
        var bytes = ExcelExporter.Build("Lich su thu", items, new ExcelExporter.Column<PaymentDto>[]
        {
            new("Ngày", r => r.CreatedDate),
            new("Chi nhánh", r => r.BranchName),
            new("Khách hàng", r => r.CustomerName),
            new("Mã KH", r => r.CustomerVisiableId),
            new("Số tiền", r => r.Amount),
            new("Mã phiếu khám", r => r.PrescriptionId),
            new("Tiếp tân thu", r => r.ReceptionistFullName ?? r.ReceptionistName),
        }, "Lịch sử thu tiền");
        return File(bytes, ExcelExporter.ContentType, "lich-su-thu.xlsx");
    }
}
