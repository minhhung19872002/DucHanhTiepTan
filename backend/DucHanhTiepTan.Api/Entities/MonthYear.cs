namespace DucHanhTiepTan.Api.Entities;

public class MonthYear
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int BranchId { get; set; }
    public int WorkDaysInMonth { get; set; }
    public long BaseLunchSubsidy { get; set; }
    public long BasePhoneSubsidy { get; set; }
    public long BaseToxicSubsidy { get; set; }
    public long BaseSalary { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}
