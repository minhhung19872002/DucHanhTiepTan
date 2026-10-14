namespace DucHanhTiepTan.Api.Entities;

public class ContainedEquipmentInvoice
{
    public int InvoiceId { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; }
    public long Price { get; set; }

    public Equipment? Equipment { get; set; }
    public EquipmentInvoice? Invoice { get; set; }
}
