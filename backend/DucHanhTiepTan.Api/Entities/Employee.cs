using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Employee
{
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    public int EmployeeTypeId { get; set; }
    public long DefaultSalary { get; set; }
    public double? DefaultRatio { get; set; }
    public int BranchId { get; set; }
    public bool IsValid { get; set; }
    public DateOnly JoinDate { get; set; }
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
    [MaxLength(250)]
    public string? Address { get; set; }
    public int Sex { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(50)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Rank { get; set; }
    [MaxLength(50)]
    public string? OtherEmployeeTypeName { get; set; }
    public long BaseLunchSubsidy { get; set; }
    public long BasePhoneSubsidy { get; set; }
    public long BaseToxicSubsidy { get; set; }

    public Branch? Branch { get; set; }
    public EmployeeType? EmployeeType { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public Assistant? Assistant { get; set; }
    public Doctor? Doctor { get; set; }
    public OtherEmployee? OtherEmployee { get; set; }
    public Receptionist? Receptionist { get; set; }
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}
