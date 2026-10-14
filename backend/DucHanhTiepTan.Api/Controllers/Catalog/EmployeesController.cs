using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>
/// Employees ("Nhân viên") – legacy EmployeeManagerControl.
/// EmployeeTypeId: 1 Bác sĩ, 2 Tiếp tân, 3 Phụ tá, 4 Khác; the role tables Doctor/Receptionist/Assistant/OtherEmployee are kept in sync.
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    public const int TypeDoctor = 1, TypeReceptionist = 2, TypeAssistant = 3, TypeOther = 4;

    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;
    public EmployeesController(AppDbContext db, ICurrentUser user) { _db = db; _user = user; }

    public record EmployeeDto(string UserName, string FullName, int EmployeeTypeId, string EmployeeTypeName, int BranchId, string BranchName,
        bool IsValid, DateOnly JoinDate, int Sex, string? Phone, string? Email, string? Address, string? Rank, string? OtherEmployeeTypeName,
        long DefaultSalary, double? DefaultRatio, long BaseLunchSubsidy, long BasePhoneSubsidy, long BaseToxicSubsidy, string Position);
    public record EmployeeInput(string UserName, string FullName, int EmployeeTypeId, int BranchId, DateOnly JoinDate, int Sex,
        string? Phone, string? Email, string? Address, string? Rank, string? OtherEmployeeTypeName,
        long DefaultSalary, double? DefaultRatio, long BaseLunchSubsidy, long BasePhoneSubsidy, long BaseToxicSubsidy, bool IsValid = true);
    public record EmployeeTypeDto(int Id, string Name);

    private static readonly Func<IQueryable<Employee>, IQueryable<EmployeeDto>> Project = q => q.Select(e => new EmployeeDto(
        e.UserName, e.FullName, e.EmployeeTypeId, e.EmployeeType!.Name, e.BranchId, e.Branch!.Name, e.IsValid, e.JoinDate, e.Sex,
        e.Phone, e.Email, e.Address, e.Rank, e.OtherEmployeeTypeName, e.DefaultSalary, e.DefaultRatio,
        e.BaseLunchSubsidy, e.BasePhoneSubsidy, e.BaseToxicSubsidy,
        e.EmployeeTypeId == TypeOther && e.OtherEmployeeTypeName != null && e.OtherEmployeeTypeName != "" ? e.OtherEmployeeTypeName : e.EmployeeType!.Name));

    /// <summary>GET api/employees?type=doctor|assistant|receptionist|other&amp;branchId=&amp;includeInvalid=&amp;keyword=</summary>
    [HttpGet]
    public async Task<List<EmployeeDto>> List([FromQuery] string? type, [FromQuery] int? branchId, [FromQuery] bool includeInvalid = false, [FromQuery] string? keyword = null)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.Employees.AsNoTracking().AsQueryable();
        if (scope != 0) q = q.Where(e => e.BranchId == scope);
        if (!includeInvalid) q = q.Where(e => e.IsValid);
        var typeId = ParseType(type);
        if (typeId.HasValue) q = q.Where(e => e.EmployeeTypeId == typeId.Value);
        q = q.WhereUnaccentLike(e => e.FullName, keyword);
        return await Project(q.OrderBy(e => e.FullName)).ToListAsync();
    }

    [HttpGet("types")]
    public async Task<List<EmployeeTypeDto>> Types() =>
        await _db.EmployeeTypes.AsNoTracking().OrderBy(t => t.Id).Select(t => new EmployeeTypeDto(t.Id, t.Name)).ToListAsync();

    [HttpGet("{userName}")]
    public async Task<ActionResult<EmployeeDto>> Get(string userName)
    {
        var dto = await Project(_db.Employees.AsNoTracking().Where(e => e.UserName == userName)).FirstOrDefaultAsync();
        return dto ?? throw new NotFoundException("Không tìm thấy nhân viên");
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.UserName)) throw new ApiException("Vui lòng điền mã số nhân viên.");
        if (string.IsNullOrWhiteSpace(input.FullName)) throw new ApiException("Vui lòng điền tên của nhân viên.");
        if (input.BranchId == 0) throw new ApiException("Vui lòng chọn chi nhánh.");
        if (!await _db.EmployeeTypes.AnyAsync(t => t.Id == input.EmployeeTypeId)) throw new ApiException($"Type {input.EmployeeTypeId} không có trong CSDL");
        var userName = input.UserName.Trim();
        if (await _db.Employees.AnyAsync(e => e.UserName == userName)) throw new ApiException("Mã số nhân viên đã được sử dụng.");

        var e = new Employee { UserName = userName, IsValid = true };
        Apply(e, input);
        _db.Employees.Add(e);
        AddRoleRow(e.UserName, e.EmployeeTypeId);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { userName = e.UserName }, (await Get(e.UserName)).Value);
    }

    [HttpPut("{userName}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<EmployeeDto>> Update(string userName, EmployeeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName)) throw new ApiException("Vui lòng điền tên của nhân viên.");
        if (input.BranchId == 0) throw new ApiException("Vui lòng chọn chi nhánh.");
        var e = await _db.Employees.Include(x => x.Doctor).Include(x => x.Receptionist).Include(x => x.Assistant).Include(x => x.OtherEmployee)
            .FirstOrDefaultAsync(x => x.UserName == userName) ?? throw new NotFoundException("Không tìm thấy nhân viên");
        if (!await _db.EmployeeTypes.AnyAsync(t => t.Id == input.EmployeeTypeId)) throw new ApiException($"Type {input.EmployeeTypeId} không có trong CSDL");

        if (e.EmployeeTypeId != input.EmployeeTypeId)
        {
            // Changing role: remove the old role row if it is not referenced, then add the new one.
            await RemoveRoleRowAsync(e);
            AddRoleRow(e.UserName, input.EmployeeTypeId);
        }
        Apply(e, input);
        e.IsValid = input.IsValid;
        await _db.SaveChangesAsync();
        return (await Get(userName)).Value!;
    }

    /// <summary>Lock ("sa thải") or re-activate an employee.</summary>
    [HttpPost("{userName}/valid")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetValid(string userName, [FromBody] bool isValid)
    {
        var e = await _db.Employees.FindAsync(userName) ?? throw new NotFoundException("Không tìm thấy nhân viên");
        e.IsValid = isValid;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userName}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(string userName)
    {
        var e = await _db.Employees.Include(x => x.Doctor).Include(x => x.Receptionist).Include(x => x.Assistant).Include(x => x.OtherEmployee)
            .FirstOrDefaultAsync(x => x.UserName == userName) ?? throw new NotFoundException("Không tìm thấy nhân viên");
        var inUse = await _db.Accounts.AnyAsync(a => a.EmployeeUserName == userName)
            || await _db.Salaries.AnyAsync(s => s.EmployeeName == userName)
            || await _db.ContainedTreatments.AnyAsync(t => t.DoctorName == userName || t.AssistantName == userName)
            || await _db.Appointments.AnyAsync(a => a.DoctorName == userName)
            || await _db.DoctorSalaries.AnyAsync(s => s.UserName == userName);
        if (inUse) throw new ApiException("Không thể xóa giá trị này, có thể nó đang được sử dụng. Bạn có thể khóa tài khoản này bằng cách sa thải nó.");
        await RemoveRoleRowAsync(e);
        _db.Employees.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static int? ParseType(string? type) => type?.ToLowerInvariant() switch
    {
        "doctor" => TypeDoctor,
        "receptionist" => TypeReceptionist,
        "assistant" => TypeAssistant,
        "other" => TypeOther,
        null or "" or "all" => null,
        _ => int.TryParse(type, out var id) ? id : null,
    };

    private static void Apply(Employee e, EmployeeInput input)
    {
        e.FullName = input.FullName.Trim();
        e.EmployeeTypeId = input.EmployeeTypeId;
        e.BranchId = input.BranchId;
        e.JoinDate = input.JoinDate;
        e.Sex = input.Sex == 1 ? 1 : 0;
        e.Phone = input.Phone; e.Email = input.Email; e.Address = input.Address;
        e.Rank = input.Rank;
        e.OtherEmployeeTypeName = input.EmployeeTypeId == TypeOther ? input.OtherEmployeeTypeName : null;
        // Legacy rule: only doctors have a base salary; other employees have a salary ratio instead.
        e.DefaultSalary = input.EmployeeTypeId == TypeDoctor ? input.DefaultSalary : 0;
        e.DefaultRatio = input.EmployeeTypeId == TypeDoctor ? null : (input.DefaultRatio ?? 1);
        e.BaseLunchSubsidy = input.BaseLunchSubsidy;
        e.BasePhoneSubsidy = input.BasePhoneSubsidy;
        e.BaseToxicSubsidy = input.BaseToxicSubsidy;
    }

    private void AddRoleRow(string userName, int typeId)
    {
        switch (typeId)
        {
            case TypeDoctor: _db.Doctors.Add(new Doctor { UserName = userName }); break;
            case TypeReceptionist: _db.Receptionists.Add(new Receptionist { UserName = userName }); break;
            case TypeAssistant: _db.Assistants.Add(new Assistant { UserName = userName }); break;
            case TypeOther: _db.OtherEmployees.Add(new OtherEmployee { UserName = userName }); break;
            default: throw new ApiException($"Type {typeId} không có trong CSDL");
        }
    }

    private async Task RemoveRoleRowAsync(Employee e)
    {
        if (e.Doctor != null)
        {
            var used = await _db.ContainedTreatments.AnyAsync(t => t.DoctorName == e.UserName) || await _db.Appointments.AnyAsync(a => a.DoctorName == e.UserName) || await _db.DoctorSalaries.AnyAsync(s => s.UserName == e.UserName);
            if (used) throw new ApiException("Bác sĩ này đã có dữ liệu khám bệnh, không thể đổi loại nhân viên.");
            _db.Doctors.Remove(e.Doctor);
        }
        if (e.Assistant != null)
        {
            if (await _db.ContainedTreatments.AnyAsync(t => t.AssistantName == e.UserName)) throw new ApiException("Phụ tá này đã có dữ liệu điều trị, không thể đổi loại nhân viên.");
            _db.Assistants.Remove(e.Assistant);
        }
        if (e.Receptionist != null) _db.Receptionists.Remove(e.Receptionist);
        if (e.OtherEmployee != null) _db.OtherEmployees.Remove(e.OtherEmployee);
    }
}
