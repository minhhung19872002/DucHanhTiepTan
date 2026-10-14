using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Invoice
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    public int BranchId { get; set; }

    public Account? User { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<ContainedInvoice> ContainedInvoices { get; set; } = new List<ContainedInvoice>();
}
