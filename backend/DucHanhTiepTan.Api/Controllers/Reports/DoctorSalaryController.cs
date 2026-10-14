using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Reports;

/// <summary>
/// "Lương bác sĩ" (DoctorSalaryControl + DeclareDoctorSalary): per (year, month, branch) = MonthYearDoctor + DoctorSalary rows.
/// The month dashboard (Tổng thu / Chi / Lợi nhuận / Mức %) drives the "Lợi nhuận" column of every doctor.
/// </summary>
[ApiController]
[Route("api/doctor-salary")]
[Authorize(Policy = "Admin")]
public class DoctorSalaryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public DoctorSalaryController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public const int NumberOfDoctors = 2; // legacy DashboardAdapter._NumberOfDoctors

    public record DashboardDto(long Income, long Expense, long Equipment, long FixedLabo, long DynamicLabo, long TotalExpense, long Profit,
        int NumberOfDoctors, double PercentProfit, double RankProfit);

    public record DoctorLineInput(string UserName, long BaseSalary, long LunchSubsidy, long RespondSubsidy, long Advance);
    public record DoctorLineDto(string UserName, string FullName, long Income, long BaseSalary, long LunchSubsidy, long RespondSubsidy, long Advance, long Salary, long Total);
    public record DoctorSheetDto(bool Declared, int Year, int Month, int BranchId, DashboardDto Dashboard, List<DoctorLineDto> Items, long TotalSalary, long TotalAdvance, long TotalReal);
    public record DoctorSheetInput(int Year, int Month, int BranchId, List<DoctorLineInput> Items);

    /// <summary>Port of DashboardAdapter.RankProbit: progressive share of the per-doctor profit.</summary>
    public static double RankProfit(double percentProfit)
    {
        if (percentProfit <= 100_000_000.0) return 15_000_000.0;
        if (percentProfit <= 150_000_000.0) return 15_000_000.0 + 0.16 * (percentProfit - 100_000_000.0);
        if (percentProfit <= 200_000_000.0) return 23_000_000.0 + 0.18 * (percentProfit - 150_000_000.0);
        return 32_000_000.0 + 0.19 * (percentProfit - 200_000_000.0);
    }

    private async Task<DashboardDto> DashboardAsync(int branch, int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1);
        var income = await _db.Payments.Where(r => r.CreatedDate >= from && r.CreatedDate < to && (branch == 0 || r.Receptionist!.BranchId == branch)).SumAsync(r => (long?)r.Amount) ?? 0;
        var expense = await _db.ContainedExpenses.Where(r => (branch == 0 || r.Expense!.Accountant!.BranchId == branch) && r.Expense!.CreatedDate >= from && r.Expense.CreatedDate < to).SumAsync(r => (long?)r.Amount) ?? 0;
        var equipment = await _db.ContainedEquipmentInvoices.Where(r => (branch == 0 || r.Invoice!.BranchId == branch) && r.Invoice!.CreatedDate >= from && r.Invoice.CreatedDate < to).SumAsync(r => (long?)((long)r.Quantity * r.Price)) ?? 0;
        var labo = await _db.ContainedLabos.Where(r => (branch == 0 || r.ContainedTreatment!.Appointment!.Prescription!.Customer!.BranchId == branch)
                && r.DeliverDate >= from && r.DeliverDate < to && r.ContainedTreatment!.Appointment!.HasDone && !r.ContainedTreatment.IsCancelled)
            .GroupBy(r => r.LaboType!.Disassembly)
            .Select(g => new { Disassembly = g.Key, Total = g.Sum(r => r.Price * (long)r.ContainedTreatment!.zQuantity) }).ToListAsync();
        var fixedLabo = labo.FirstOrDefault(l => !l.Disassembly)?.Total ?? 0;
        var dynamicLabo = labo.FirstOrDefault(l => l.Disassembly)?.Total ?? 0;
        var totalExpense = expense + equipment + fixedLabo + dynamicLabo;
        var profit = income - totalExpense;
        var percent = (double)profit / NumberOfDoctors;
        return new DashboardDto(income, expense, equipment, fixedLabo, dynamicLabo, totalExpense, profit, NumberOfDoctors, percent, RankProfit(percent));
    }

    private static DoctorLineDto Line(string userName, string fullName, long income, DoctorLineInput i)
    {
        var salary = income + i.BaseSalary + i.RespondSubsidy + i.LunchSubsidy;
        return new DoctorLineDto(userName, fullName, income, i.BaseSalary, i.LunchSubsidy, i.RespondSubsidy, i.Advance, salary, salary - i.Advance);
    }

    private static DoctorSheetDto Sheet(bool declared, int year, int month, int branch, DashboardDto dash, List<DoctorLineDto> items) =>
        new(declared, year, month, branch, dash, items, items.Sum(i => i.Salary), items.Sum(i => i.Advance), items.Sum(i => i.Total));

    private static DashboardDto EmptyDashboard => new(0, 0, 0, 0, 0, 0, 0, NumberOfDoctors, 0, RankProfit(0));

    /// <summary>Declared sheet (DoctorSalaryControl.LoadContent). Requires a concrete branch like the legacy screen.</summary>
    [HttpGet]
    public async Task<DoctorSheetDto> Get(int year, int month, int? branchId)
    {
        var branch = BranchScope.Resolve(_user, branchId);
        if (branch == 0) return Sheet(false, year, month, 0, EmptyDashboard, []);
        var dash = await DashboardAsync(branch, year, month);
        var declared = await _db.MonthYearDoctors.AnyAsync(m => m.Year == year && m.Month == month && m.BranchId == branch);
        var rows = await _db.DoctorSalaries.Include(d => d.User!).ThenInclude(d => d.User)
            .Where(d => d.BranchId == branch && d.Year == year && d.Month == month).OrderBy(d => d.User!.User!.FullName).ToListAsync();
        var items = rows.Select(d => Line(d.UserName, d.User?.User?.FullName ?? d.UserName, (long)dash.RankProfit,
            new DoctorLineInput(d.UserName, d.BaseSalary, d.LunchSubsidy, d.RespondSubsidy, d.Advance))).ToList();
        return Sheet(declared, year, month, branch, dash, items);
    }

    /// <summary>Pre-filled declaration from the valid doctors of the branch (DoctorSalaryControl.LoadDeclarationContent).</summary>
    [HttpGet("declaration")]
    public async Task<DoctorSheetDto> Declaration(int year, int month, int? branchId)
    {
        var branch = BranchScope.Resolve(_user, branchId);
        if (branch == 0) throw new ApiException("Chọn một chi nhánh cụ thể để khai báo lương bác sĩ");
        var dash = await DashboardAsync(branch, year, month);
        var doctors = await _db.Doctors.Include(d => d.User).Where(d => d.User!.BranchId == branch && d.User.IsValid).OrderBy(d => d.User!.FullName).ToListAsync();
        var items = doctors.Select(d => Line(d.UserName, d.User?.FullName ?? d.UserName, (long)dash.RankProfit,
            new DoctorLineInput(d.UserName, d.User?.DefaultSalary ?? 0, 0, 0, 0))).ToList();
        return Sheet(false, year, month, branch, dash, items);
    }

    /// <summary>Declare the month (DeclareDoctorSalary.buttonOK_Click).</summary>
    [HttpPost]
    public async Task<DoctorSheetDto> Declare(DoctorSheetInput input)
    {
        var branch = BranchScope.Resolve(_user, input.BranchId);
        if (branch == 0) throw new ApiException("Chọn một chi nhánh cụ thể để khai báo lương bác sĩ");
        if (input.Month is < 1 or > 12) throw new ApiException("Tháng không hợp lệ");
        var lastDay = new DateTime(input.Year, input.Month, DateTime.DaysInMonth(input.Year, input.Month));
        if (lastDay > DateTime.Today) throw new ApiException("Tháng bạn chọn chưa tới nên không thể khai báo lương.");
        if (await _db.MonthYearDoctors.AnyAsync(m => m.Year == input.Year && m.Month == input.Month && m.BranchId == branch))
            throw new ApiException("Tháng này đã được khai báo lương");
        _db.MonthYearDoctors.Add(new MonthYearDoctor { Year = input.Year, Month = input.Month, BranchId = branch });
        foreach (var i in input.Items)
            _db.DoctorSalaries.Add(new DoctorSalary
            {
                Year = input.Year, Month = input.Month, BranchId = branch, UserName = i.UserName,
                BaseSalary = i.BaseSalary, LunchSubsidy = i.LunchSubsidy, RespondSubsidy = i.RespondSubsidy, Advance = i.Advance,
            });
        await _db.SaveChangesAsync();
        return await Get(input.Year, input.Month, branch);
    }

    [HttpPut("{year:int}/{month:int}/{branchId:int}")]
    public async Task<DoctorSheetDto> Update(int year, int month, int branchId, DoctorSheetInput input)
    {
        var branch = BranchScope.Resolve(_user, branchId);
        if (!await _db.MonthYearDoctors.AnyAsync(m => m.Year == year && m.Month == month && m.BranchId == branch))
            throw new NotFoundException("Tháng này chưa được khai báo lương bác sĩ");
        var existing = await _db.DoctorSalaries.Where(d => d.Year == year && d.Month == month && d.BranchId == branch).ToListAsync();
        var incoming = input.Items.ToDictionary(i => i.UserName);
        foreach (var d in existing)
        {
            if (!incoming.TryGetValue(d.UserName, out var i)) { _db.DoctorSalaries.Remove(d); continue; }
            d.BaseSalary = i.BaseSalary; d.LunchSubsidy = i.LunchSubsidy; d.RespondSubsidy = i.RespondSubsidy; d.Advance = i.Advance;
            incoming.Remove(d.UserName);
        }
        foreach (var i in incoming.Values)
            _db.DoctorSalaries.Add(new DoctorSalary { Year = year, Month = month, BranchId = branch, UserName = i.UserName, BaseSalary = i.BaseSalary, LunchSubsidy = i.LunchSubsidy, RespondSubsidy = i.RespondSubsidy, Advance = i.Advance });
        await _db.SaveChangesAsync();
        return await Get(year, month, branch);
    }

    [HttpDelete("{year:int}/{month:int}/{branchId:int}")]
    public async Task<IActionResult> Delete(int year, int month, int branchId)
    {
        var branch = BranchScope.Resolve(_user, branchId);
        var my = await _db.MonthYearDoctors.FirstOrDefaultAsync(m => m.Year == year && m.Month == month && m.BranchId == branch)
                 ?? throw new NotFoundException("Tháng này chưa được khai báo lương bác sĩ");
        _db.DoctorSalaries.RemoveRange(_db.DoctorSalaries.Where(d => d.Year == year && d.Month == month && d.BranchId == branch));
        _db.MonthYearDoctors.Remove(my);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(int year, int month, int? branchId)
    {
        var sheet = await Get(year, month, branchId);
        var cols = new List<ExcelExporter.Column<DoctorLineDto>>
        {
            new("Bác sĩ", r => r.FullName), new("Mã", r => r.UserName), new("Lợi nhuận", r => r.Income), new("Lương căn bản", r => r.BaseSalary),
            new("Cơm", r => r.LunchSubsidy), new("Trách nhiệm", r => r.RespondSubsidy), new("Tổng thu nhập", r => r.Salary), new("Tạm ứng", r => r.Advance), new("Thực lãnh", r => r.Total),
        };
        var d = sheet.Dashboard;
        var title = $"Lương bác sĩ tháng {month:00}/{year} - Tổng thu {d.Income:N0}, tổng chi {d.TotalExpense:N0}, lợi nhuận {d.Profit:N0}, mức % {d.RankProfit:N0}";
        return new FileContentResult(ExcelExporter.Build("Luong bac si", sheet.Items, cols, title), ExcelExporter.ContentType) { FileDownloadName = $"luong-bac-si-{year}-{month:00}.xlsx" };
    }
}
