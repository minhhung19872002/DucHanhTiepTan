using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedTreatment
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int SubId { get; set; }
    public int ServiceId { get; set; }
    public int? LaboId { get; set; }
    public long OriginalPrice { get; set; }
    public long Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public long Paid { get; set; }
    [MaxLength(50)]
    public string? DoctorName { get; set; }
    [MaxLength(50)]
    public string? AssistantName { get; set; }
    [MaxLength(250)]
    public string? Comment { get; set; }
    public int zQuantity { get; set; }
    [MaxLength(150)]
    public string? ToothPositions { get; set; }
    public long PreviousDebt { get; set; }
    public bool IsCancelled { get; set; }

    public Appointment? Appointment { get; set; }
    public Assistant? Assistant { get; set; }
    public Doctor? Doctor { get; set; }
    public Service? Service { get; set; }
    public ICollection<ContainedLabo> ContainedLabos { get; set; } = new List<ContainedLabo>();
}
