namespace DucHanhTiepTan.Api.Entities;

public class ContainedInvoice
{
    public int InvoiceId { get; set; }
    public int DrugId { get; set; }
    public int Quantity { get; set; }
    public long Price { get; set; }

    public Drug? Drug { get; set; }
    public Invoice? Invoice { get; set; }
}
