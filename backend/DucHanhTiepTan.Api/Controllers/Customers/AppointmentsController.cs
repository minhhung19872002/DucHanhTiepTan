using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Customers;

/// <summary>
/// Lịch hẹn – port of AppointingControl (+ MainForm Khám/Hủy/Đổi handlers) and AppointmentDashboardControl.
/// </summary>
[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public AppointmentsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    private static AppointmentRowDto ToRow(AppointmentAdapter a) => new(
        a.PrescriptionId, a.No, a.BranchId, a.BranchName, a.CreatedDate, a.HasDone, a.AppointmentDate, a.AppointmentReason, a.Advised,
        a.CustomerId, a.CustomerVisiableId, a.CustomerName, a.CustomerPhone, a.DoctorName, a.DoctorFullName, a.NextAppointmentNo, a.Paid,
        a.NextAppointmentNo.HasValue, a.NextAppointmentNo.HasValue ? "Đã hẹn" : "Đang hẹn");

    private static (DateTime from, DateTime to) Range(string dateMode, DateTime? date, DateTime? from, DateTime? to)
    {
        var today = DateTime.Today;
        return dateMode switch
        {
            "date" => ((date ?? today).Date, (date ?? today).Date),
            "range" => ((from ?? today).Date, (to ?? today).Date),
            _ => (today, today),
        };
    }

    /// <summary>
    /// Danh sách hẹn. status: all | appointing (Đang hẹn) | appointed (Đã hẹn). searchBy: name | customerId | prescriptionId.
    /// </summary>
    [HttpGet]
    public async Task<PagedResult<AppointmentRowDto>> List(
        [FromQuery] int? branchId, [FromQuery] string dateMode = "today", [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string status = "all",
        [FromQuery] string? keyword = null, [FromQuery] string searchBy = "name",
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var (f, t) = Range(dateMode, date, from, to);
        var tEnd = t.AddDays(1);
        var query = _db.AppointmentAdapters.AsNoTracking()
            .Where(a => (scope == 0 || a.BranchId == scope) && a.HasDone && a.AppointmentDate != null
                        && a.AppointmentDate >= f && a.AppointmentDate < tEnd);
        query = status switch
        {
            "appointing" => query.Where(a => a.NextAppointmentNo == null),
            "appointed" => query.Where(a => a.NextAppointmentNo != null),
            _ => query,
        };
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = searchBy switch
            {
                "customerId" => query.Where(a => a.CustomerVisiableId.ToString().Contains(kw.TrimStart('0').Length == 0 ? kw : kw.TrimStart('0'))),
                "prescriptionId" => query.Where(a => a.PrescriptionId.ToString().Contains(kw)),
                _ => query.WhereUnaccentLike(a => a.CustomerName, kw),
            };
        }
        var paged = await query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.PrescriptionId).ToPagedAsync(page, pageSize);
        return new PagedResult<AppointmentRowDto>(paged.Items.Select(ToRow).ToList(), paged.Total, paged.Page, paged.PageSize);
    }

    [HttpGet("{prescriptionId:long}/{no:int}")]
    public async Task<ActionResult<AppointmentRowDto>> Get(long prescriptionId, int no)
    {
        var scope = BranchScope.Resolve(_user, null);
        var a = await _db.AppointmentAdapters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && x.No == no && (scope == 0 || x.BranchId == scope))
            ?? throw new NotFoundException("Không tìm thấy lịch hẹn");
        return ToRow(a);
    }

    private async Task<Appointment> LoadAppointmentAsync(long prescriptionId, int no)
    {
        var a = await _db.Appointments.Include(x => x.Prescription!).ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && x.No == no)
            ?? throw new NotFoundException("Không tìm thấy lịch hẹn");
        if (!_user.IsAdministrator && a.Prescription!.Customer!.BranchId != _user.BranchId) throw new NotFoundException("Không tìm thấy lịch hẹn");
        return a;
    }

    private async Task EnsureNotAppointedAsync(Appointment a)
    {
        var hasNext = await _db.Appointments.AnyAsync(x => x.PrescriptionId == a.PrescriptionId && x.No == a.No + 1);
        if (!a.AppointmentDate.HasValue || hasNext) throw new ApiException("Phiếu khám này đã hẹn rồi.");
    }

    /// <summary>"Khám": opens a new appointment (No = max + 1) on the prescription of the selected row.</summary>
    [HttpPost("{prescriptionId:long}/examine")]
    public async Task<ActionResult<object>> Examine(long prescriptionId)
    {
        var prescription = await _db.Prescriptions.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == prescriptionId)
                           ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        if (!_user.IsAdministrator && prescription.Customer!.BranchId != _user.BranchId) throw new NotFoundException("Không tìm thấy phiếu khám");
        var last = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId).OrderByDescending(a => a.No).FirstOrDefaultAsync();
        if (last is not null && !last.HasDone) throw new ApiException("Phiếu khám này đã hẹn rồi.");
        var appointment = new Appointment
        {
            PrescriptionId = prescriptionId,
            No = (last?.No ?? 0) + 1,
            HasDone = false,
            CreatedDate = DateTime.Now,
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return new { prescriptionId, no = appointment.No, customerName = prescription.Customer!.Name };
    }

    /// <summary>"Hủy": removes the appointment date (only while still "Đang hẹn").</summary>
    [HttpPost("{prescriptionId:long}/{no:int}/cancel")]
    public async Task<IActionResult> Cancel(long prescriptionId, int no)
    {
        var a = await LoadAppointmentAsync(prescriptionId, no);
        await EnsureNotAppointedAsync(a);
        a.AppointmentDate = null;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>"Đổi": changes the appointment date (only while still "Đang hẹn").</summary>
    [HttpPut("{prescriptionId:long}/{no:int}/date")]
    public async Task<IActionResult> ChangeDate(long prescriptionId, int no, ChangeAppointmentDateInput input)
    {
        var a = await LoadAppointmentAsync(prescriptionId, no);
        await EnsureNotAppointedAsync(a);
        a.AppointmentDate = input.AppointmentDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes an unfinished appointment and its data (legacy DoctorNavigator "Hủy" / DeleteAppointmentConfirmationDialog).
    /// If the prescription has no appointment left it is deleted too; otherwise, when cancelAppointmentElse is true,
    /// the latest remaining appointment loses its appointment date ("Hủy hẹn"), else it stays "Đang hẹn".
    /// </summary>
    [HttpDelete("{prescriptionId:long}/{no:int}")]
    public async Task<IActionResult> Delete(long prescriptionId, int no, [FromQuery] bool cancelAppointmentElse = false)
    {
        var a = await LoadAppointmentAsync(prescriptionId, no);
        if (a.HasDone) throw new ApiException("Phiếu khám này đã xong nên không hủy được.");
        await using var tx = await _db.Database.BeginTransactionAsync();
        await DeleteAppointmentDataAsync(_db, prescriptionId, no);
        _db.Appointments.Remove(a);
        await _db.SaveChangesAsync();
        var lasted = await _db.Appointments.Where(x => x.PrescriptionId == prescriptionId).OrderByDescending(x => x.No).FirstOrDefaultAsync();
        if (lasted is null)
        {
            _db.ContainedDiscounts.RemoveRange(_db.ContainedDiscounts.Where(d => d.PrescriptionId == prescriptionId));
            _db.ContainedConsultingLabos.RemoveRange(_db.ContainedConsultingLabos.Where(l => l.PrescriptionId == prescriptionId));
            _db.ContainedConsultings.RemoveRange(_db.ContainedConsultings.Where(c => c.PrescriptionId == prescriptionId));
            _db.Payments.RemoveRange(_db.Payments.Where(p => p.PrescriptionId == prescriptionId));
            _db.Prescriptions.Remove(a.Prescription!);
        }
        else if (cancelAppointmentElse)
        {
            lasted.AppointmentDate = null;
        }
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    /// <summary>Removes sub-entities of one appointment (no = null → every appointment of the prescription).</summary>
    internal static async Task DeleteAppointmentDataAsync(AppDbContext db, long prescriptionId, int? no)
    {
        db.ContainedLabos.RemoveRange(db.ContainedLabos.Where(l => l.PrescriptionId == prescriptionId && (no == null || l.AppointmentNo == no)));
        db.ContainedTreatments.RemoveRange(db.ContainedTreatments.Where(t => t.PrescriptionId == prescriptionId && (no == null || t.AppointmentNo == no)));
        db.ContainedReceipts.RemoveRange(db.ContainedReceipts.Where(r => r.PrescriptionId == prescriptionId && (no == null || r.AppointmentNo == no)));
        db.ContainedUsedReceipts.RemoveRange(db.ContainedUsedReceipts.Where(r => r.PrescriptionId == prescriptionId && (no == null || r.AppointmentNo == no)));
        db.UsedReceipts.RemoveRange(db.UsedReceipts.Where(r => r.PrescriptionId == prescriptionId && (no == null || r.AppointmentNo == no)));
        db.ContainedUsedEquipments.RemoveRange(db.ContainedUsedEquipments.Where(e => e.PrescriptionId == prescriptionId && (no == null || e.AppointmentNo == no)));
        db.ContainedPictures.RemoveRange(db.ContainedPictures.Where(p => p.PrescriptionId == prescriptionId && (no == null || p.AppointmentNo == no)));
        await Task.CompletedTask;
    }

    // ------------------------------------------------------------------ dashboard ("Công việc trong ngày")

    /// <summary>dateMode: all | today | yesterday | day | month | range.</summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "Admin")]
    public async Task<DashboardDto> Dashboard([FromQuery] int? branchId, [FromQuery] string dateMode = "today",
        [FromQuery] DateTime? date = null, [FromQuery] DateTime? month = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int limit = 200)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var today = DateTime.Today;
        DateTime? minor, major;
        switch (dateMode)
        {
            case "all": minor = major = null; break;
            case "yesterday": minor = major = today.AddDays(-1); break;
            case "day": minor = major = (date ?? today).Date; break;
            case "month":
                {
                    var m = month ?? today;
                    minor = new DateTime(m.Year, m.Month, 1);
                    major = minor.Value.AddMonths(1).AddDays(-1);
                    break;
                }
            case "range": minor = (from ?? today).Date; major = (to ?? today).Date; break;
            default: minor = major = today; break;
        }
        limit = Math.Clamp(limit, 1, 2000);

        var query = _db.Appointments.AsNoTracking()
            .Where(a => a.HasDone && (scope == 0 || a.Prescription!.Customer!.BranchId == scope));
        if (minor.HasValue)
        {
            var f = minor.Value; var t = major!.Value.AddDays(1);
            query = query.Where(a => a.CreatedDate >= f && a.CreatedDate < t);
        }
        var appointments = await query.OrderBy(a => a.CreatedDate).Take(limit)
            .Select(a => new
            {
                a.PrescriptionId, a.No, a.CreatedDate, a.AppointmentDate,
                BranchName = a.Prescription!.Customer!.Branch!.Name,
                CustomerId = a.Prescription.CustomerId,
                a.Prescription.Customer.VisiableId, CustomerName = a.Prescription.Customer.Name,
                a.Prescription.Customer.Sex, a.Prescription.Customer.BirthYear, a.Prescription.Customer.Phone,
                a.Prescription.Customer.Address, a.Prescription.Customer.WardName, a.Prescription.Customer.DistrictName, a.Prescription.Customer.ProvinceName,
                CustomerCreated = a.Prescription.Customer.CreatedDate,
                DoctorFullName = _db.Employees.Where(e => e.UserName == a.DoctorName).Select(e => e.FullName).FirstOrDefault(),
                Treatments = a.ContainedTreatments.OrderBy(t => t.SubId).Select(t => new
                {
                    t.SubId, t.IsCancelled, t.zQuantity, t.Price, t.OriginalPrice, t.Paid, t.Comment, ServiceName = t.Service!.Name,
                }).ToList(),
            }).ToListAsync();

        var ids = appointments.Select(a => a.PrescriptionId).Distinct().ToList();
        var remains = await _db.TreatmentAdapters.AsNoTracking()
            .Where(t => ids.Contains(t.PrescriptionId))
            .Select(t => new { t.PrescriptionId, t.AppointmentNo, t.SubId, t.Remain }).ToListAsync();
        var remainLookup = remains.GroupBy(r => (r.PrescriptionId, r.AppointmentNo))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.SubId).First().Remain);

        var items = new List<DashboardItemDto>();
        long total = 0, totalIfNotDiscount = 0, paid = 0, remain = 0;
        foreach (var a in appointments)
        {
            var tTotal = a.Treatments.Sum(t => t.IsCancelled ? 0L : (long)t.zQuantity * t.Price);
            var tTotalNd = a.Treatments.Sum(t => t.IsCancelled ? 0L : (long)t.zQuantity * t.OriginalPrice);
            var tPaid = a.Treatments.Sum(t => t.Paid);
            var tRemain = remainLookup.TryGetValue((a.PrescriptionId, a.No), out var r) ? r : 0;
            var item = new DashboardItemDto(
                a.PrescriptionId, a.No, a.BranchName, a.CreatedDate, a.CustomerId, a.VisiableId, a.CustomerName, a.Sex, a.BirthYear,
                CustomersController.AddressRepresentation(a.Address, a.WardName, a.DistrictName, a.ProvinceName), a.Phone,
                string.Concat(a.Treatments.Where(t => !string.IsNullOrEmpty(t.ServiceName)).Select(t => t.ServiceName + ". ")),
                tTotal, tTotalNd, tPaid, tRemain,
                !a.AppointmentDate.HasValue,
                a.DoctorFullName,
                string.Concat(a.Treatments.Where(t => t.Comment != null).Select(t => t.Comment + ". ")),
                a.CustomerCreated.Date == a.CreatedDate.Date);
            items.Add(item);
            total += tTotal; totalIfNotDiscount += tTotalNd; paid += tPaid; remain += tRemain;
        }
        return new DashboardDto(items, total, totalIfNotDiscount, paid, remain);
    }

    [HttpGet("dashboard/export")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DashboardExport([FromQuery] int? branchId, [FromQuery] string dateMode = "today",
        [FromQuery] DateTime? date = null, [FromQuery] DateTime? month = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int limit = 2000)
    {
        var data = await Dashboard(branchId, dateMode, date, month, from, to, limit);
        var bytes = ExcelExporter.Build("Cong viec", data.Items, new List<ExcelExporter.Column<DashboardItemDto>>
        {
            new("Chi nhánh", r => r.BranchName),
            new("Ngày tạo", r => r.CreatedDate),
            new("Mã KH", r => r.VisiableId, "000000"),
            new("Khách hàng", r => r.CustomerName),
            new("Giới tính", r => r.Sex == 1 ? "Nữ" : "Nam"),
            new("Năm sinh", r => r.BirthYear, "0"),
            new("Điện thoại", r => r.Phone),
            new("Địa chỉ", r => r.Address),
            new("Nội dung làm việc trong ngày", r => r.Content),
            new("Tổng (nếu không giảm)", r => r.TotalIfNotDiscount),
            new("Tổng", r => r.Total),
            new("Đã trả", r => r.Paid),
            new("Còn lại", r => r.Remain),
            new("Kết thúc", r => r.Finished ? "R" : ""),
            new("Bác sĩ", r => r.DoctorFullName),
            new("Ghi chú", r => r.Comment),
        }, "Công việc trong ngày");
        return File(bytes, ExcelExporter.ContentType, "cong-viec-trong-ngay.xlsx");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] string dateMode = "today", [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string status = "all",
        [FromQuery] string? keyword = null, [FromQuery] string searchBy = "name")
    {
        var rows = await List(branchId, dateMode, date, from, to, status, keyword, searchBy, 1, 500);
        var bytes = ExcelExporter.Build("Lich hen", rows.Items, new List<ExcelExporter.Column<AppointmentRowDto>>
        {
            new("Ngày", r => r.AppointmentDate),
            new("Bác sĩ hẹn", r => r.DoctorFullName),
            new("Khách hàng", r => r.CustomerName),
            new("Mã KH", r => r.CustomerVisiableId, "000000"),
            new("Điện thoại", r => r.CustomerPhone),
            new("Lí do hẹn", r => r.AppointmentReason),
            new("Phiếu", r => r.PrescriptionId, "0"),
            new("Trạng thái", r => r.Status),
        }, "Danh sách hẹn");
        return File(bytes, ExcelExporter.ContentType, "danh-sach-hen.xlsx");
    }
}
