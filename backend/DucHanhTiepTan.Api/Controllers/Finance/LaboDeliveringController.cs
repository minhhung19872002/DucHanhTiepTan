using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>
/// "Labo chờ" / "Labo hẹn" (legacy LaboDeliveringControl): ContainedLabo rows of finished, non-cancelled treatments,
/// filtered by branch, receive/deliver date, vendor and delivered state. Receptionists can toggle "Đã giao" and edit dates/price.
/// </summary>
[ApiController]
[Route("api/labo-delivering")]
[Authorize]
public class LaboDeliveringController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public LaboDeliveringController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public record LaboItemDto(long PrescriptionId, int AppointmentNo, int SubId, int LaboVendorId, string LaboVendor, int LaboTypeId, string LaboType,
        string? Color, DateTime ReceiverDate, DateTime DeliverDate, bool Delivered, string? DoctorName, string? DoctorFullName, string ServiceName,
        string CustomerName, string? CustomerPhone, long CustomerId, int Quantity, long Price, long Total, string BranchName);
    public record LaboListDto(List<LaboItemDto> Items, long Total, int Count);
    public record LaboUpdateInput(bool? Delivered, DateTime? ReceiverDate, DateTime? DeliverDate, long? Price, string? Color);
    public record LookupDto(int Id, string Name);

    private IQueryable<Entities.ContainedLabo> Query(int? branchId, DateTime? receiverFrom, DateTime? receiverTo, DateTime? deliverFrom, DateTime? deliverTo,
        int? vendorId, bool? delivered)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.ContainedLabos.Where(r => r.ContainedTreatment!.Appointment!.HasDone && !r.ContainedTreatment.IsCancelled);
        if (scope != 0) q = q.Where(r => r.ContainedTreatment!.Appointment!.Prescription!.Customer!.BranchId == scope);
        if (receiverFrom.HasValue) { var f = receiverFrom.Value.Date; q = q.Where(r => r.ReceiverDate >= f); }
        if (receiverTo.HasValue) { var t = receiverTo.Value.Date.AddDays(1); q = q.Where(r => r.ReceiverDate < t); }
        if (deliverFrom.HasValue) { var f = deliverFrom.Value.Date; q = q.Where(r => r.DeliverDate >= f); }
        if (deliverTo.HasValue) { var t = deliverTo.Value.Date.AddDays(1); q = q.Where(r => r.DeliverDate < t); }
        if (vendorId.HasValue && vendorId.Value != 0) q = q.Where(r => r.LaboVendorId == vendorId.Value);
        if (delivered.HasValue) q = q.Where(r => r.Delivered == delivered.Value);
        return q;
    }

    private static IQueryable<LaboItemDto> Project(IQueryable<Entities.ContainedLabo> q) => q.Select(r => new LaboItemDto(
        r.PrescriptionId, r.AppointmentNo, r.SubId, r.LaboVendorId, r.LaboVendor!.Name, r.LaboTypeId, r.LaboType!.Name, r.Color,
        r.ReceiverDate, r.DeliverDate, r.Delivered,
        r.ContainedTreatment!.DoctorName,
        r.ContainedTreatment.Doctor != null ? r.ContainedTreatment.Doctor.User!.FullName : null,
        r.ContainedTreatment.Service!.Name,
        r.ContainedTreatment.Appointment!.Prescription!.Customer!.Name,
        r.ContainedTreatment.Appointment.Prescription.Customer.Phone,
        r.ContainedTreatment.Appointment.Prescription.CustomerId,
        r.ContainedTreatment.zQuantity, r.Price, r.Price * r.ContainedTreatment.zQuantity,
        r.ContainedTreatment.Appointment.Prescription.Customer.Branch!.Name));

    /// <summary>delivered: omit = all, true = đã giao, false = chưa giao (legacy default = chưa giao).</summary>
    [HttpGet]
    public async Task<LaboListDto> List([FromQuery] int? branchId, [FromQuery] DateTime? receiverFrom, [FromQuery] DateTime? receiverTo,
        [FromQuery] DateTime? deliverFrom, [FromQuery] DateTime? deliverTo, [FromQuery] int? vendorId, [FromQuery] bool? delivered, [FromQuery] int limit = 500)
    {
        var q = Query(branchId, receiverFrom, receiverTo, deliverFrom, deliverTo, vendorId, delivered);
        var total = await q.SumAsync(r => (long?)(r.Price * r.ContainedTreatment!.zQuantity)) ?? 0;
        var count = await q.CountAsync();
        var items = await Project(q.OrderBy(r => r.DeliverDate).ThenBy(r => r.PrescriptionId).ThenBy(r => r.SubId).Take(Math.Clamp(limit, 1, 5000))).ToListAsync();
        return new LaboListDto(items, total, count);
    }

    [HttpGet("vendors")]
    public async Task<List<LookupDto>> Vendors() =>
        await _db.LaboVendors.OrderBy(v => v.Name).Select(v => new LookupDto(v.Id, v.Name)).ToListAsync();

    [HttpPut("{prescriptionId:long}/{appointmentNo:int}/{subId:int}")]
    public async Task<IActionResult> Update(long prescriptionId, int appointmentNo, int subId, LaboUpdateInput input)
    {
        if (_user.IsAdministrator) throw new ApiException("Tài khoản quản trị chỉ được xem danh sách labo");
        var labo = await _db.ContainedLabos.Include(r => r.ContainedTreatment!).ThenInclude(t => t.Appointment!).ThenInclude(a => a.Prescription!).ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == appointmentNo && r.SubId == subId)
            ?? throw new NotFoundException("Không tìm thấy labo");
        if (labo.ContainedTreatment!.Appointment!.Prescription!.Customer!.BranchId != _user.BranchId) throw new ApiException("Labo thuộc chi nhánh khác");
        if (input.Delivered.HasValue) labo.Delivered = input.Delivered.Value;
        if (input.ReceiverDate.HasValue) labo.ReceiverDate = input.ReceiverDate.Value;
        if (input.DeliverDate.HasValue) labo.DeliverDate = input.DeliverDate.Value;
        if (input.Price.HasValue)
        {
            if (input.Price.Value < 0) throw new ApiException("Đơn giá không hợp lệ");
            labo.Price = input.Price.Value;
        }
        if (input.Color is not null) labo.Color = input.Color;
        if (labo.DeliverDate.Date < labo.ReceiverDate.Date) throw new ApiException("Ngày giao phải sau ngày nhận");
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] DateTime? receiverFrom, [FromQuery] DateTime? receiverTo,
        [FromQuery] DateTime? deliverFrom, [FromQuery] DateTime? deliverTo, [FromQuery] int? vendorId, [FromQuery] bool? delivered)
    {
        var items = await Project(Query(branchId, receiverFrom, receiverTo, deliverFrom, deliverTo, vendorId, delivered)
            .OrderBy(r => r.DeliverDate).ThenBy(r => r.PrescriptionId)).ToListAsync();
        var bytes = ExcelExporter.Build("Labo", items, new ExcelExporter.Column<LaboItemDto>[]
        {
            new("Nha Khoa", r => r.LaboVendor),
            new("Loại", r => r.LaboType),
            new("Màu sắc", r => r.Color),
            new("Ngày nhận", r => r.ReceiverDate),
            new("Ngày giao", r => r.DeliverDate),
            new("Đã giao", r => r.Delivered),
            new("Mã bác sĩ", r => r.DoctorName),
            new("Bác sĩ", r => r.DoctorFullName),
            new("Dịch vụ", r => r.ServiceName),
            new("Khách hàng", r => r.CustomerName),
            new("Điện thoại KH", r => r.CustomerPhone),
            new("Số lượng", r => r.Quantity),
            new("Đơn giá", r => r.Price),
            new("Thành tiền", r => r.Total),
            new("Chi nhánh", r => r.BranchName),
        }, "Giao Labo");
        return File(bytes, ExcelExporter.ContentType, "labo.xlsx");
    }
}
