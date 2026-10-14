using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>
/// "Khách hàng đang nợ" / "Thu nợ khách hàng" (legacy CustomerDebtViewerControl).
/// Debt = PrescriptionAdapter.Remain &gt; 0, grouped by customer. Collecting a payment adds a Payment row and
/// increases Paid on the last finished treatment of the prescription, exactly like the legacy code.
/// </summary>
[ApiController]
[Route("api/debts")]
[Authorize]
public class DebtsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public DebtsController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public record DebtPrescriptionDto(long Id, DateTime CreatedDate, DateTime LastAppointmentDate, long Total, long Paid, long Remain, string BranchName);
    public record DebtCustomerDto(long CustomerId, long VisiableId, string Name, string? Phone, long TotalCost, long Paid, long Remain, List<DebtPrescriptionDto> Prescriptions);
    public record DebtListDto(List<DebtCustomerDto> Customers, long TotalCost, long TotalPaid, long TotalRemain);
    public record DebtTreatmentDto(long PrescriptionId, int AppointmentNo, int SubId, DateTime CreatedDate, string ServiceName, int Quantity, long Price,
        string? Positions, string? DoctorName, string? AssistantName, string? Labo, long Total, long Paid, long Remain, bool IsCancelled);
    public record DebtTreatmentListDto(List<DebtTreatmentDto> Items, long Total, long Paid, long Remain);
    public record CollectPaymentRequest(long Amount, DateTime? Date);
    public record CollectPaymentResult(long PaymentId, long Paid, long Remain);

    private IQueryable<PrescriptionAdapter> DebtQuery(int? branchId)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        return _db.PrescriptionAdapters.Where(r => r.Remain > 0 && (scope == 0 || r.BranchId == scope));
    }

    private async Task<DebtListDto> BuildListAsync(int? branchId, string? keyword, bool byId)
    {
        var rows = await DebtQuery(branchId).OrderBy(r => r.CustomerName).ThenBy(r => r.CreatedDate).ToListAsync();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = VietnameseText.Normalize(keyword);
            rows = rows.Where(r => byId
                    ? r.CustomerVisiableId.ToString().Contains(key) || r.CustomerId.ToString().Contains(key)
                    : VietnameseText.Normalize(r.CustomerName).Contains(key))
                .ToList();
        }
        var customers = rows.GroupBy(r => r.CustomerId).Select(g =>
        {
            var first = g.First();
            var prescriptions = g.Select(p => new DebtPrescriptionDto(p.Id, p.CreatedDate, p.LastAppointmentDate, p.Paid + p.Remain, p.Paid, p.Remain, p.BranchName)).ToList();
            return new DebtCustomerDto(g.Key, first.CustomerVisiableId, first.CustomerName, first.CustomerPhone,
                prescriptions.Sum(p => p.Total), prescriptions.Sum(p => p.Paid), prescriptions.Sum(p => p.Remain), prescriptions);
        }).ToList();
        var paid = customers.Sum(c => c.Paid);
        var remain = customers.Sum(c => c.Remain);
        return new DebtListDto(customers, paid + remain, paid, remain);
    }

    /// <summary>List customers in debt. keyword matches name (default) or customer id when byId=true.</summary>
    [HttpGet]
    public Task<DebtListDto> List([FromQuery] int? branchId, [FromQuery] string? keyword, [FromQuery] bool byId = false) =>
        BuildListAsync(branchId, keyword, byId);

    /// <summary>Finished treatments of a prescription (the bottom grid of the legacy screen).</summary>
    [HttpGet("prescriptions/{id:long}/treatments")]
    public async Task<DebtTreatmentListDto> Treatments(long id)
    {
        var items = await _db.ContainedTreatments
            .Where(r => r.PrescriptionId == id && r.Appointment!.HasDone)
            .OrderBy(r => r.SubId)
            .Select(r => new
            {
                r.PrescriptionId, r.AppointmentNo, r.SubId, r.CreatedDate, r.Price, r.zQuantity, r.ToothPositions, r.PreviousDebt, r.Paid, r.IsCancelled,
                ServiceName = r.Service!.Name,
                DoctorName = r.Doctor != null ? r.Doctor.User!.FullName : null,
                AssistantName = r.Assistant != null ? r.Assistant.User!.FullName : null,
                Labo = r.ContainedLabos.Select(l => l.LaboType!.Name + " - " + l.LaboVendor!.Name).FirstOrDefault(),
            })
            .ToListAsync();
        var list = items.Select(r =>
        {
            var total = r.IsCancelled ? 0 : r.PreviousDebt + r.zQuantity * r.Price;
            return new DebtTreatmentDto(r.PrescriptionId, r.AppointmentNo, r.SubId, r.CreatedDate, r.ServiceName, r.zQuantity, r.Price, r.ToothPositions,
                r.DoctorName, r.AssistantName, r.Labo, total, r.Paid, total - r.Paid, r.IsCancelled);
        }).ToList();
        var adapter = await _db.PrescriptionAdapters.FirstOrDefaultAsync(r => r.Id == id);
        var paid = adapter?.Paid ?? 0;
        var remain = adapter?.Remain ?? 0;
        return new DebtTreatmentListDto(list, paid + remain, paid, remain);
    }

    /// <summary>Collect a payment on a prescription (receptionist only, like the legacy MainForm).</summary>
    [HttpPost("prescriptions/{id:long}/payments")]
    public async Task<CollectPaymentResult> Collect(long id, CollectPaymentRequest request)
    {
        if (_user.IsAdministrator)
            throw new ApiException("Tài khoản quản trị chỉ được xem; vui lòng dùng tài khoản tiếp tân để thu nợ");
        if (request.Amount <= 0) throw new ApiException("Giá trị thanh toán không thể bé hơn hoặc bằng 0.");
        var adapter = await _db.PrescriptionAdapters.FirstOrDefaultAsync(r => r.Id == id) ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        if (!BranchScope.Matches(BranchScope.Resolve(_user, null), adapter.BranchId)) throw new ApiException("Phiếu khám thuộc chi nhánh khác");
        if (request.Amount > adapter.Remain) throw new ApiException("Giá trị thanh toán không thể lớn hơn giá trị còn lại.");

        var last = await _db.ContainedTreatments
            .Where(r => r.PrescriptionId == id && r.Appointment!.HasDone)
            .OrderByDescending(r => r.SubId).FirstOrDefaultAsync()
            ?? throw new ApiException("Phiếu khám chưa có điều trị hoàn tất để thu tiền");
        last.Paid += request.Amount;
        var payment = new Payment
        {
            PrescriptionId = id,
            Amount = request.Amount,
            CreatedDate = (request.Date ?? DateTime.Now).Date,
            ReceptionistName = _user.UserName,
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return new CollectPaymentResult(payment.Id, adapter.Paid + request.Amount, adapter.Remain - request.Amount);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] string? keyword, [FromQuery] bool byId = false)
    {
        var data = await BuildListAsync(branchId, keyword, byId);
        var rows = data.Customers.SelectMany(c => c.Prescriptions.Select(p => (c, p))).ToList();
        var bytes = ExcelExporter.Build("Khach hang no", rows, new ExcelExporter.Column<(DebtCustomerDto c, DebtPrescriptionDto p)>[]
        {
            new("Khách hàng", r => r.c.Name),
            new("Mã KH", r => r.c.VisiableId),
            new("Điện thoại", r => r.c.Phone),
            new("Chi nhánh", r => r.p.BranchName),
            new("Mã khám", r => r.p.Id),
            new("Ngày tạo", r => r.p.CreatedDate),
            new("Ngày khám cuối", r => r.p.LastAppointmentDate),
            new("Tổng phí", r => r.p.Total),
            new("Đã trả", r => r.p.Paid),
            new("Còn nợ", r => r.p.Remain),
        }, "Danh sách khách hàng nợ");
        return File(bytes, ExcelExporter.ContentType, "khach-hang-no.xlsx");
    }
}
