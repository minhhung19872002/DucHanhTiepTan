namespace DucHanhTiepTan.Api.Entities;

public class ContainedEquipmentExportInvoice
{
    public int EquipmentExportInvoiceId { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; }

    public Equipment? Equipment { get; set; }
    public EquipmentExportInvoice? EquipmentExportInvoice { get; set; }
}
