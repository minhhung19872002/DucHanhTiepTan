using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Customers;

/// <summary>
/// Lịch sử khám – port of PrescriptionSearcherControl (read-only search over the PrescriptionAdapter view).
/// The examination module owns the editing of a prescription (api/examination).
/// </summary>
[ApiController]
[Route("api/prescriptions")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public PrescriptionsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private IQueryable<PrescriptionAdapter> Scoped(int? branchId)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        return _db.PrescriptionAdapters.AsNoTracking().Where(p => scope == 0 || p.BranchId == scope);
    }

    private static PrescriptionRowDto ToRow(PrescriptionAdapter p) => new(
        p.Id, p.BranchId, p.BranchName, p.CustomerId, p.CustomerName, p.CustomerVisiableId, p.CustomerPhone,
        p.HasDone, p.Treatment, p.Diagnostic, p.Symptoms, p.CreatedDate,
        p.LastAppointmentDate, p.AppointmentDate, p.AppointmentReason, p.AppointmentAdvised,
        p.AppointmentNo, p.Remain + p.Paid, p.Paid, p.Remain);

    /// <summary>
    /// Search history. customerSearchBy: name | customerId | prescriptionId. diseaseField: symptoms | treatment | diagnostic.
    /// </summary>
    [HttpGet]
    public async Task<PagedResult<PrescriptionRowDto>> List(
        [FromQuery] int? branchId, [FromQuery] long? customerId,
        [FromQuery] string? customerKeyword, [FromQuery] string customerSearchBy = "name",
        [FromQuery] string? diseaseKeyword = null, [FromQuery] string diseaseField = "symptoms",
        [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null,
        [FromQuery] DateTime? lastFrom = null, [FromQuery] DateTime? lastTo = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = Scoped(branchId);
        if (customerId.HasValue) query = query.Where(p => p.CustomerId == customerId.Value);
        if (createdFrom.HasValue && createdTo.HasValue)
        {
            var f = createdFrom.Value.Date; var t = createdTo.Value.Date.AddDays(1);
            query = query.Where(p => p.CreatedDate >= f && p.CreatedDate < t);
        }
        if (lastFrom.HasValue && lastTo.HasValue)
        {
            var f = lastFrom.Value.Date; var t = lastTo.Value.Date.AddDays(1);
            query = query.Where(p => p.LastAppointmentDate >= f && p.LastAppointmentDate < t);
        }
        if (!string.IsNullOrWhiteSpace(customerKeyword))
        {
            var kw = customerKeyword.Trim();
            switch (customerSearchBy)
            {
                case "customerId":
                    {
                        var digits = kw.TrimStart('0');
                        if (long.TryParse(kw, out var exact))
                            query = query.Where(p => p.CustomerVisiableId == exact || p.CustomerVisiableId.ToString().Contains(digits));
                        else
                            query = query.Where(p => p.CustomerVisiableId.ToString().Contains(kw));
                        break;
                    }
                case "prescriptionId":
                    query = query.Where(p => p.Id.ToString().Contains(kw));
                    break;
                default:
                    query = query.WhereUnaccentLike(p => p.CustomerName, kw);
                    break;
            }
        }
        if (!string.IsNullOrWhiteSpace(diseaseKeyword))
        {
            var kw = diseaseKeyword.Trim();
            query = diseaseField switch
            {
                "treatment" => query.Where(p => p.Treatment != null && p.Treatment != "").WhereUnaccentLike(p => p.Treatment, kw),
                "diagnostic" => query.Where(p => p.Diagnostic != null && p.Diagnostic != "").WhereUnaccentLike(p => p.Diagnostic, kw),
                _ => query.Where(p => p.Symptoms != null && p.Symptoms != "").WhereUnaccentLike(p => p.Symptoms, kw),
            };
        }
        var paged = await query.OrderByDescending(p => p.LastAppointmentDate).ThenByDescending(p => p.Id).ToPagedAsync(page, pageSize);
        return new PagedResult<PrescriptionRowDto>(paged.Items.Select(ToRow).ToList(), paged.Total, paged.Page, paged.PageSize);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PrescriptionRowDto>> Get(long id)
    {
        var p = await Scoped(null).FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        return ToRow(p);
    }

    /// <summary>
    /// Deletes a prescription and all of its data. Administrators only; refused when money has been collected.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var prescription = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        if (await _db.Payments.AnyAsync(p => p.PrescriptionId == id) || await _db.ContainedTreatments.AnyAsync(t => t.PrescriptionId == id && t.Paid > 0))
            throw new ApiException("Phiếu khám đã có thu tiền, không thể xóa.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        await AppointmentsController.DeleteAppointmentDataAsync(_db, id, null);
        _db.ContainedDiscounts.RemoveRange(_db.ContainedDiscounts.Where(d => d.PrescriptionId == id));
        _db.ContainedConsultingLabos.RemoveRange(_db.ContainedConsultingLabos.Where(l => l.PrescriptionId == id));
        _db.ContainedConsultings.RemoveRange(_db.ContainedConsultings.Where(c => c.PrescriptionId == id));
        _db.Appointments.RemoveRange(_db.Appointments.Where(a => a.PrescriptionId == id));
        _db.Prescriptions.Remove(prescription);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] long? customerId,
        [FromQuery] string? customerKeyword, [FromQuery] string customerSearchBy = "name",
        [FromQuery] string? diseaseKeyword = null, [FromQuery] string diseaseField = "symptoms",
        [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null,
        [FromQuery] DateTime? lastFrom = null, [FromQuery] DateTime? lastTo = null)
    {
        var rows = await List(branchId, customerId, customerKeyword, customerSearchBy, diseaseKeyword, diseaseField, createdFrom, createdTo, lastFrom, lastTo, 1, 500);
        var bytes = ExcelExporter.Build("Lich su kham", rows.Items, new List<ExcelExporter.Column<PrescriptionRowDto>>
        {
            new("Mã phiếu", r => r.Id, "0"),
            new("Chi nhánh", r => r.BranchName),
            new("Khách hàng", r => r.CustomerName),
            new("Mã KH", r => r.CustomerVisiableId, "000000"),
            new("Ngày tạo", r => r.CreatedDate),
            new("Số lần hẹn", r => r.AppointmentNo, "0"),
            new("Ngày hẹn cuối", r => r.LastAppointmentDate),
            new("Yêu cầu điều trị", r => r.Treatment),
            new("Triệu chứng", r => r.Diagnostic),
            new("Chuẩn đoán", r => r.Symptoms),
            new("Lịch hẹn", r => r.LastNextAppointmentDate),
            new("Lí do hẹn", r => r.LastAppointmentReason),
            new("Tổng tiền", r => r.Total),
            new("Đã trả", r => r.Paid),
            new("Còn lại", r => r.Remain),
        }, "Lịch sử khám");
        return File(bytes, ExcelExporter.ContentType, "lich-su-kham.xlsx");
    }
}
