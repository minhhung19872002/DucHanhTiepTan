namespace DucHanhTiepTan.Api.Entities;

public class ContainedUsedEquipment
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; }

    public Appointment? Appointment { get; set; }
    public Equipment? Equipment { get; set; }
}
