namespace DucHanhTiepTan.Api.Entities;

public class UsedReceipt
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<ContainedUsedReceipt> ContainedUsedReceipts { get; set; } = new List<ContainedUsedReceipt>();
}
