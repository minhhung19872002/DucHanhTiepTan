namespace DucHanhTiepTan.Api.Entities;

public class EquipmentInvoice
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<ContainedEquipmentInvoice> ContainedEquipmentInvoices { get; set; } = new List<ContainedEquipmentInvoice>();
}
