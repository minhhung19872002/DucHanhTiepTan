using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class DoctorSalary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int BranchId { get; set; }
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    public long BaseSalary { get; set; }
    public long LunchSubsidy { get; set; }
    public long RespondSubsidy { get; set; }
    public long Advance { get; set; }

    public Doctor? User { get; set; }
    public MonthYearDoctor? MonthYearDoctor { get; set; }
}
