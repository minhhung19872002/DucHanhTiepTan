namespace DucHanhTiepTan.Api.Entities;

public class ContainedUsedReceipt
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int DrugId { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }

    public Drug? Drug { get; set; }
    public UsedReceipt? UsedReceipt { get; set; }
}
