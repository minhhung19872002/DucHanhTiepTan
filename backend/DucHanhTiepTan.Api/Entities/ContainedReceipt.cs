using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedReceipt
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int DrugId { get; set; }
    public int Amount { get; set; }
    [MaxLength(50)]
    public string? Instruction { get; set; }
    public double PerDay { get; set; }
    public double PerTimes { get; set; }

    public Appointment? Appointment { get; set; }
    public Drug? Drug { get; set; }
}
