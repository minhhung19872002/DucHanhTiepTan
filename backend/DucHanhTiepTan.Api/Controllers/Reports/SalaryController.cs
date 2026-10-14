using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Reports;

/// <summary>
/// "Lương nhân viên" (SalaryControl + DeclareSalaryDialog): one salary sheet per (year, month, branch) = MonthYear + Salary rows.
/// Doctors are excluded (they have their own sheet, see DoctorSalaryController).
/// </summary>
[ApiController]
[Route("api/salary")]
[Authorize(Policy = "Admin")]
public class SalaryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public SalaryController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    // ------------------------------------------------------------------ DTOs

    public record MonthYearDto(int Year, int Month, int BranchId, long BaseSalary, int WorkDaysInMonth);

    /// <summary>Editable inputs of one salary line (the light-blue columns of the legacy grid).</summary>
    public record SalaryLineInput(string UserName, string? Rank, double? Ratio, int WorkDays, int OvertimeInMinute, int TowHoursOvertimeDays,
        long DegreeAllowance, long HeathAllowance, long DayOffAllowance, long RiskAllowance, long RetireAllowance,
        long Absent, long Fee, long Advance, long BaseLunchSubsidy, long BasePhoneSubsidy, long BaseToxicSubsidy);

    /// <summary>Line + computed values (SalaryItemAdapter).</summary>
    public record SalaryLineDto(string UserName, string FullName, string? BranchName, string? EmployeeType, string? Rank, double? Ratio,
        int WorkDays, int OvertimeInMinute, int TowHoursOvertimeDays, long DegreeAllowance, long HeathAllowance, long DayOffAllowance,
        long RiskAllowance, long RetireAllowance, long Absent, long Fee, long Advance, long BaseLunchSubsidy, long BasePhoneSubsidy, long BaseToxicSubsidy,
        double BaseSalary, double BaseSalaryDependOnWorkDays, double OvertimeRatio, double OvertimeBonus, double LunchSubsidy, double PhoneSubsidy, double ToxicSubsidy,
        double Income, double Total);

    public record SalarySheetDto(bool Declared, MonthYearDto MonthYear, List<SalaryLineDto> Items, double TotalIncome, double TotalAdvance, double TotalReal);

    public record SalarySheetInput(int Year, int Month, int BranchId, long BaseSalary, int WorkDaysInMonth, List<SalaryLineInput> Items);

    // ------------------------------------------------------------------ calculation (port of SalaryItemAdapter)

    public static class SalaryCalc
    {
        public static SalaryLineDto Compute(SalaryLineInput x, string fullName, string? branchName, string? employeeType, long monthBaseSalary, int workDaysInMonth)
        {
            var ratio = x.Ratio ?? 0.0;
            var ww = workDaysInMonth == 0 ? 0.0 : (double)x.WorkDays / workDaysInMonth;
            var baseSalary = ratio * monthBaseSalary;
            var baseByWorkDays = baseSalary * ww;
            var hourRate = workDaysInMonth == 0 ? 0.0 : ratio * monthBaseSalary / workDaysInMonth / 8.0;
            var overtimeRatio = hourRate * 2.0 * x.TowHoursOvertimeDays;
            var overtimeBonus = x.WorkDays == 0 ? 0.0 : (x.OvertimeInMinute / 60) * hourRate * 2.0;
            var lunch = x.BaseLunchSubsidy * ww;
            var phone = x.BasePhoneSubsidy * ww;
            var toxic = x.BaseToxicSubsidy * ww;
            var income = baseByWorkDays + overtimeBonus + overtimeRatio + lunch + phone + toxic
                         + x.DegreeAllowance + x.HeathAllowance + x.DayOffAllowance + x.RiskAllowance + x.RetireAllowance - x.Absent - x.Fee;
            var total = income - x.Advance;
            return new SalaryLineDto(x.UserName, fullName, branchName, employeeType, x.Rank, x.Ratio, x.WorkDays, x.OvertimeInMinute, x.TowHoursOvertimeDays,
                x.DegreeAllowance, x.HeathAllowance, x.DayOffAllowance, x.RiskAllowance, x.RetireAllowance, x.Absent, x.Fee, x.Advance,
                x.BaseLunchSubsidy, x.BasePhoneSubsidy, x.BaseToxicSubsidy,
                baseSalary, baseByWorkDays, overtimeRatio, overtimeBonus, lunch, phone, toxic, income, total);
        }
    }

    private static string? Position(Employee e) => e.EmployeeTypeId != 4 ? e.EmployeeType?.Name : e.OtherEmployeeTypeName;

    private static SalarySheetDto Sheet(bool declared, MonthYearDto my, List<SalaryLineDto> items) =>
        new(declared, my, items, items.Sum(i => i.Income), items.Sum(i => i.Advance), items.Sum(i => i.Income) - items.Sum(i => i.Advance));

    private int ResolveBranch(int? branchId)
    {
        var b = BranchScope.Resolve(_user, branchId);
        return b;
    }

    private async Task<(long BaseSalary, int WorkDays)> ConfigurationDefaultsAsync()
    {
        var cfg = await _db.Configurations.Where(c => c.Name == "BaseSalary" || c.Name == "WorkDaysInMonth").ToDictionaryAsync(c => c.Name, c => c.Value);
        long.TryParse(cfg.GetValueOrDefault("BaseSalary"), out var baseSalary);
        int.TryParse(cfg.GetValueOrDefault("WorkDaysInMonth"), out var workDays);
        return (baseSalary, workDays);
    }

    // ------------------------------------------------------------------ endpoints

    /// <summary>Declared sheet of a month (SalaryControl.LoadContent). branchId 0 = all branches (read-only overview).</summary>
    [HttpGet]
    public async Task<SalarySheetDto> Get(int year, int month, int? branchId)
    {
        var branch = ResolveBranch(branchId);
        var monthYears = await _db.MonthYears.Where(m => m.Year == year && m.Month == month && (branch == 0 || m.BranchId == branch)).ToListAsync();
        var rows = await _db.Salaries.Include(s => s.Employee!).ThenInclude(e => e.Branch).Include(s => s.Employee!).ThenInclude(e => e.EmployeeType)
            .Where(s => s.Year == year && s.Month == month && (branch == 0 || s.BranchId == branch))
            .OrderBy(s => s.Employee!.FullName).ToListAsync();
        var items = new List<SalaryLineDto>();
        foreach (var s in rows)
        {
            var my = monthYears.FirstOrDefault(m => m.BranchId == s.BranchId);
            var input = new SalaryLineInput(s.EmployeeName, s.Rank, s.Ratio, s.WorkDays, s.OvertimeInMinute, s.TowHoursOvertimeDays, s.DegreeAllowance, s.HeathAllowance,
                s.DayOffAllowance, s.RiskAllowance, s.RetireAllowance, s.Absent, s.Fee, s.Advance, s.BaseLunchSubsidy, s.BasePhoneSubsidy, s.BaseToxicSubsidy);
            items.Add(SalaryCalc.Compute(input, s.Employee?.FullName ?? s.EmployeeName, s.Employee?.Branch?.Name, s.Employee is null ? null : Position(s.Employee), my?.BaseSalary ?? 0, my?.WorkDaysInMonth ?? 0));
        }
        var header = branch == 0
            ? new MonthYearDto(year, month, 0, monthYears.FirstOrDefault()?.BaseSalary ?? 0, monthYears.FirstOrDefault()?.WorkDaysInMonth ?? 0)
            : new MonthYearDto(year, month, branch, monthYears.FirstOrDefault()?.BaseSalary ?? 0, monthYears.FirstOrDefault()?.WorkDaysInMonth ?? 0);
        return Sheet(monthYears.Count > 0, header, items);
    }

    /// <summary>Pre-filled declaration for a month not yet declared (DeclareSalaryDialog / SalaryControl.LoadSalaryDeclaration).</summary>
    [HttpGet("declaration")]
    public async Task<SalarySheetDto> Declaration(int year, int month, int? branchId)
    {
        var branch = ResolveBranch(branchId);
        if (branch == 0) throw new ApiException("Chọn một chi nhánh cụ thể để khai báo lương");
        var (baseSalary, workDays) = await ConfigurationDefaultsAsync();
        var employees = await _db.Employees.Include(e => e.Branch).Include(e => e.EmployeeType)
            .Where(e => e.BranchId == branch && e.IsValid && e.EmployeeTypeId != 1)
            .OrderBy(e => e.FullName).ToListAsync();
        var items = employees.Select(e =>
        {
            var input = new SalaryLineInput(e.UserName, e.Rank, e.DefaultRatio, workDays, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, e.BaseLunchSubsidy, e.BasePhoneSubsidy, e.BaseToxicSubsidy);
            return SalaryCalc.Compute(input, e.FullName, e.Branch?.Name, Position(e), baseSalary, workDays);
        }).ToList();
        return Sheet(false, new MonthYearDto(year, month, branch, baseSalary, workDays), items);
    }

    /// <summary>Preview computed values for edited lines without saving (keeps formulas server-side).</summary>
    [HttpPost("compute")]
    public async Task<SalarySheetDto> Compute(SalarySheetInput input)
    {
        var names = input.Items.Select(i => i.UserName).ToList();
        var employees = await _db.Employees.Include(e => e.Branch).Include(e => e.EmployeeType).Where(e => names.Contains(e.UserName)).ToDictionaryAsync(e => e.UserName);
        var items = input.Items.Select(i =>
        {
            employees.TryGetValue(i.UserName, out var e);
            return SalaryCalc.Compute(i, e?.FullName ?? i.UserName, e?.Branch?.Name, e is null ? null : Position(e), input.BaseSalary, input.WorkDaysInMonth);
        }).ToList();
        return Sheet(false, new MonthYearDto(input.Year, input.Month, input.BranchId, input.BaseSalary, input.WorkDaysInMonth), items);
    }

    /// <summary>Declare the month (DeclareSalaryDialog.buttonOK_Click).</summary>
    [HttpPost]
    public async Task<ActionResult<SalarySheetDto>> Declare(SalarySheetInput input)
    {
        var branch = ResolveBranch(input.BranchId);
        if (branch == 0) throw new ApiException("Chọn một chi nhánh cụ thể để khai báo lương");
        if (input.Month is < 1 or > 12) throw new ApiException("Tháng không hợp lệ");
        var lastDay = new DateTime(input.Year, input.Month, DateTime.DaysInMonth(input.Year, input.Month));
        if (lastDay > DateTime.Today) throw new ApiException("Tháng bạn chọn chưa tới nên không thể khai báo lương.");
        if (await _db.MonthYears.AnyAsync(m => m.Year == input.Year && m.Month == input.Month && m.BranchId == branch))
            throw new ApiException("Tháng này đã được khai báo lương");

        _db.MonthYears.Add(new MonthYear
        {
            Year = input.Year, Month = input.Month, BranchId = branch,
            BaseSalary = input.BaseSalary, WorkDaysInMonth = input.WorkDaysInMonth,
            BaseLunchSubsidy = 0, BasePhoneSubsidy = 0, BaseToxicSubsidy = 0,
        });
        foreach (var i in input.Items)
            _db.Salaries.Add(ToEntity(i, input.Year, input.Month, branch));
        await _db.SaveChangesAsync();
        return await Get(input.Year, input.Month, branch);
    }

    /// <summary>Update an already declared month (base values + lines). Lines missing from the payload are removed.</summary>
    [HttpPut("{year:int}/{month:int}/{branchId:int}")]
    public async Task<SalarySheetDto> Update(int year, int month, int branchId, SalarySheetInput input)
    {
        var branch = ResolveBranch(branchId);
        var my = await _db.MonthYears.FirstOrDefaultAsync(m => m.Year == year && m.Month == month && m.BranchId == branch)
                 ?? throw new NotFoundException("Tháng này chưa được khai báo lương");
        my.BaseSalary = input.BaseSalary;
        my.WorkDaysInMonth = input.WorkDaysInMonth;
        var existing = await _db.Salaries.Where(s => s.Year == year && s.Month == month && s.BranchId == branch).ToListAsync();
        var incoming = input.Items.ToDictionary(i => i.UserName);
        foreach (var s in existing)
        {
            if (!incoming.TryGetValue(s.EmployeeName, out var i)) { _db.Salaries.Remove(s); continue; }
            Apply(s, i);
            incoming.Remove(s.EmployeeName);
        }
        foreach (var i in incoming.Values)
            _db.Salaries.Add(ToEntity(i, year, month, branch));
        await _db.SaveChangesAsync();
        return await Get(year, month, branch);
    }

    [HttpDelete("{year:int}/{month:int}/{branchId:int}")]
    public async Task<IActionResult> Delete(int year, int month, int branchId)
    {
        var branch = ResolveBranch(branchId);
        var my = await _db.MonthYears.FirstOrDefaultAsync(m => m.Year == year && m.Month == month && m.BranchId == branch)
                 ?? throw new NotFoundException("Tháng này chưa được khai báo lương");
        _db.Salaries.RemoveRange(_db.Salaries.Where(s => s.Year == year && s.Month == month && s.BranchId == branch));
        _db.MonthYears.Remove(my);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int year, int month, int? branchId)
    {
        var sheet = await Get(year, month, branchId);
        var cols = new List<ExcelExporter.Column<SalaryLineDto>>
        {
            new("Nhân viên", r => r.FullName), new("Mã nhân viên", r => r.UserName), new("Chi nhánh", r => r.BranchName), new("Cấp bậc", r => r.Rank),
            new("Chức vụ", r => r.EmployeeType), new("Lương cơ bản", r => Math.Round(r.BaseSalary)), new("Hệ số LCB", r => r.Ratio, "0.00"),
            new("Ngày công", r => r.WorkDays), new("LCB theo ngày công", r => Math.Round(r.BaseSalaryDependOnWorkDays)), new("Lương 2h/ngày", r => Math.Round(r.OvertimeRatio)),
            new("Số ngày tăng ca thêm 2h", r => r.TowHoursOvertimeDays), new("Tăng ca (phút)", r => r.OvertimeInMinute), new("Tiền tăng ca", r => Math.Round(r.OvertimeBonus)),
            new("Tiền ăn", r => Math.Round(r.LunchSubsidy)), new("Độc hại", r => Math.Round(r.ToxicSubsidy)), new("Điện thoại", r => Math.Round(r.PhoneSubsidy)),
            new("Phụ cấp chuyên môn", r => r.DegreeAllowance), new("Phụ cấp điều trị", r => r.HeathAllowance), new("Phụ cấp ngày nghỉ", r => r.DayOffAllowance),
            new("Trợ cấp ốm đau", r => r.RiskAllowance), new("Phụ cấp thôi việc", r => r.RetireAllowance), new("Phạt đi trễ", r => r.Absent), new("Phạt khác", r => r.Fee),
            new("Tổng lương", r => Math.Round(r.Income)), new("Đã ứng", r => r.Advance), new("Thực nhận", r => Math.Round(r.Total)),
        };
        var title = $"Lương nhân viên tháng {month:00}/{year} - Lương cơ bản {sheet.MonthYear.BaseSalary:N0}, ngày công chuẩn {sheet.MonthYear.WorkDaysInMonth}";
        return new FileContentResult(ExcelExporter.Build("Luong nhan vien", sheet.Items, cols, title), ExcelExporter.ContentType) { FileDownloadName = $"luong-nhan-vien-{year}-{month:00}.xlsx" };
    }

    private static Salary ToEntity(SalaryLineInput i, int year, int month, int branch)
    {
        var s = new Salary { Year = year, Month = month, BranchId = branch, EmployeeName = i.UserName };
        Apply(s, i);
        return s;
    }

    private static void Apply(Salary s, SalaryLineInput i)
    {
        s.Rank = i.Rank; s.Ratio = i.Ratio ?? 0.0; s.WorkDays = i.WorkDays; s.OvertimeInMinute = i.OvertimeInMinute; s.TowHoursOvertimeDays = i.TowHoursOvertimeDays;
        s.DegreeAllowance = i.DegreeAllowance; s.HeathAllowance = i.HeathAllowance; s.DayOffAllowance = i.DayOffAllowance; s.RiskAllowance = i.RiskAllowance;
        s.RetireAllowance = i.RetireAllowance; s.Absent = (int)i.Absent; s.Fee = i.Fee; s.Advance = i.Advance;
        s.BaseLunchSubsidy = i.BaseLunchSubsidy; s.BasePhoneSubsidy = i.BasePhoneSubsidy; s.BaseToxicSubsidy = i.BaseToxicSubsidy;
    }
}
