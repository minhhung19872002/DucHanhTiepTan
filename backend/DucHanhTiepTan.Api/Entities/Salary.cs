using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Salary
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int BranchId { get; set; }
    [MaxLength(50)]
    public string EmployeeName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Rank { get; set; }
    public double Ratio { get; set; }
    public int WorkDays { get; set; }
    public int OvertimeInMinute { get; set; }
    public long DegreeAllowance { get; set; }
    public long HeathAllowance { get; set; }
    public long DayOffAllowance { get; set; }
    public long RiskAllowance { get; set; }
    public long RetireAllowance { get; set; }
    public long Advance { get; set; }
    public long BasePhoneSubsidy { get; set; }
    public long BaseToxicSubsidy { get; set; }
    public long BaseLunchSubsidy { get; set; }
    public int TowHoursOvertimeDays { get; set; }
    public int Absent { get; set; }
    public long Fee { get; set; }

    public Employee? Employee { get; set; }
    public MonthYear? MonthYear { get; set; }
}
