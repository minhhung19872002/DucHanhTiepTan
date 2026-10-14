namespace DucHanhTiepTan.Api.Entities;

public class EquipmentExportInvoice
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<ContainedEquipmentExportInvoice> ContainedEquipmentExportInvoices { get; set; } = new List<ContainedEquipmentExportInvoice>();
}
