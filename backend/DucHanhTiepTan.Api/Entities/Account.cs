using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Account
{
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
    [MaxLength(150)]
    public string Password { get; set; } = string.Empty;
    public int BranchId { get; set; }
    [MaxLength(50)]
    public string? FullName { get; set; }
    [MaxLength(50)]
    public string? EmployeeUserName { get; set; }

    public Branch? Branch { get; set; }
    public Employee? EmployeeUser { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
