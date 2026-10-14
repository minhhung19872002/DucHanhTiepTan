using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Drug
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    public long Price { get; set; }
    [MaxLength(250)]
    public string? Description { get; set; }
    public long OutPrice { get; set; }

    public ICollection<ContainedInvoice> ContainedInvoices { get; set; } = new List<ContainedInvoice>();
    public ICollection<ContainedReceipt> ContainedReceipts { get; set; } = new List<ContainedReceipt>();
    public ICollection<ContainedUsedReceipt> ContainedUsedReceipts { get; set; } = new List<ContainedUsedReceipt>();
    public ICollection<DrugInBranch> DrugInBranches { get; set; } = new List<DrugInBranch>();
}
