using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DucHanhTiepTan.Api.Controllers.Examination;

/// <summary>"Khám bệnh" workflow: in-progress list, start/continue a prescription, edit an appointment, pictures, printing data.</summary>
[ApiController]
[Route("api/examination")]
[Authorize]
public class ExaminationController : ControllerBase
{
    private readonly ExaminationService _service;
    private readonly ICurrentUser _user;

    public ExaminationController(AppDbContext db, ICurrentUser user)
    {
        _service = new ExaminationService(db, user);
        _user = user;
    }

    /// <summary>Lookup lists for the examination form (services, drugs, consumables with stock, labo, doctors, assistants).</summary>
    [HttpGet("lookups")]
    public Task<LookupsDto> Lookups([FromQuery] int? branchId) => _service.GetLookupsAsync(BranchScope.Resolve(_user, branchId));

    /// <summary>"Đang khám": appointments not finished yet.</summary>
    [HttpGet("in-progress")]
    public Task<List<InProgressItem>> InProgress([FromQuery] int? branchId) => _service.GetInProgressAsync(BranchScope.Resolve(_user, branchId));

    /// <summary>Create a new prescription (phiếu khám) + first appointment for a customer.</summary>
    [HttpPost("start")]
    public Task<StartResult> Start(StartRequest request) => _service.StartAsync(request.CustomerId);

    /// <summary>Create the next appointment (No = max + 1) for an existing prescription ("Khám" from the appointment list).</summary>
    [HttpPost("{prescriptionId:long}/appointments")]
    public Task<StartResult> NextAppointment(long prescriptionId) => _service.NextAppointmentAsync(prescriptionId);

    /// <summary>Remove an unfinished appointment ("Xóa phiếu khám" in the in-progress list).</summary>
    [HttpDelete("{prescriptionId:long}/{appointmentNo:int}")]
    public async Task<IActionResult> Delete(long prescriptionId, int appointmentNo, [FromQuery] bool cancelAppointmentElse = false)
    {
        await _service.DeleteOpenAppointmentAsync(prescriptionId, appointmentNo, cancelAppointmentElse);
        return NoContent();
    }

    [HttpGet("{prescriptionId:long}/{appointmentNo:int}")]
    public Task<ExaminationDetail> Detail(long prescriptionId, int appointmentNo) => _service.GetDetailAsync(prescriptionId, appointmentNo);

    /// <summary>Save the whole appointment (legacy "Lưu lại" button).</summary>
    [HttpPut("{prescriptionId:long}/{appointmentNo:int}")]
    public async Task<IActionResult> Save(long prescriptionId, int appointmentNo, SaveRequest request)
    {
        await _service.SaveAsync(prescriptionId, appointmentNo, request);
        return NoContent();
    }

    /// <summary>Cancel a service of a previous appointment ("Hủy dịch vụ").</summary>
    [HttpPost("{prescriptionId:long}/treatments/{subId:int}/cancel")]
    public async Task<IActionResult> CancelService(long prescriptionId, int subId)
    {
        await _service.CancelServiceAsync(prescriptionId, subId);
        return NoContent();
    }

    /// <summary>Validate a VIP code (mã khuyến mãi) and return its discounts.</summary>
    [HttpGet("membership/{id}")]
    public async Task<ActionResult<MembershipInfo>> Membership(string id)
    {
        var m = await _service.LoadMembershipAsync(id, validate: true);
        if (m is null) return NotFound(new { message = "Không tìm thấy mã VIP" });
        return m;
    }

    /// <summary>Read-only summary used by the prescription detail page (appointment numbers, newest first).</summary>
    [HttpGet("prescriptions/{id:long}")]
    public Task<PrescriptionSummary> Prescription(long id) => _service.GetPrescriptionSummaryAsync(id);

    // ---------------- pictures ----------------
    [HttpPost("{prescriptionId:long}/{appointmentNo:int}/pictures")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<object>> AddPicture(long prescriptionId, int appointmentNo, IFormFile file)
    {
        if (file is null || file.Length == 0) throw new ApiException("Chưa chọn tập tin hình ảnh");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var id = await _service.AddPictureAsync(prescriptionId, appointmentNo, file.FileName, ms.ToArray());
        return new { id };
    }

    [HttpDelete("pictures/{id:long}")]
    public async Task<IActionResult> DeletePicture(long id)
    {
        await _service.DeletePictureAsync(id);
        return NoContent();
    }

    [HttpGet("pictures/{id:long}")]
    public async Task<IActionResult> GetPicture(long id)
    {
        var p = await _service.GetPictureAsync(id);
        if (p is null) return NotFound();
        var (data, name) = p.Value;
        var contentType = name?.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == true ? "image/png" : "image/jpeg";
        return File(data, contentType);
    }
}
