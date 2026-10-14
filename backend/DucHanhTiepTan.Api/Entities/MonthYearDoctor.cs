namespace DucHanhTiepTan.Api.Entities;

public class MonthYearDoctor
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<DoctorSalary> DoctorSalaries { get; set; } = new List<DoctorSalary>();
}
