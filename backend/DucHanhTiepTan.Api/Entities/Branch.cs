using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Branch
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(150)]
    public string? Address { get; set; }
    [MaxLength(50)]
    public string? Tel { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<DrugInBranch> DrugInBranches { get; set; } = new List<DrugInBranch>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<EquimentInBranch> EquimentInBranches { get; set; } = new List<EquimentInBranch>();
    public ICollection<EquipmentExportInvoice> EquipmentExportInvoices { get; set; } = new List<EquipmentExportInvoice>();
    public ICollection<EquipmentInvoice> EquipmentInvoices { get; set; } = new List<EquipmentInvoice>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<MemberShipGroup> MemberShipGroups { get; set; } = new List<MemberShipGroup>();
    public ICollection<MonthYear> MonthYears { get; set; } = new List<MonthYear>();
    public ICollection<MonthYearDoctor> MonthYearDoctors { get; set; } = new List<MonthYearDoctor>();
}
