namespace DucHanhTiepTan.Api.Entities;

public class ContainedDiscount
{
    public long PrescriptionId { get; set; }
    public int ServiceId { get; set; }
    public double Value { get; set; }

    public Prescription? Prescription { get; set; }
    public Service? Service { get; set; }
}
