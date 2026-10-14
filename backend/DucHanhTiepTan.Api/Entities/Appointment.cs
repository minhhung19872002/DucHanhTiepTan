using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Appointment
{
    public long PrescriptionId { get; set; }
    public int No { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool HasDone { get; set; }
    public DateTime? AppointmentDate { get; set; }
    [MaxLength(250)]
    public string? AppointmentReason { get; set; }
    [MaxLength(250)]
    public string? Advised { get; set; }
    [MaxLength(50)]
    public string? DoctorName { get; set; }

    public Doctor? Doctor { get; set; }
    public Prescription? Prescription { get; set; }
    public ICollection<ContainedPicture> ContainedPictures { get; set; } = new List<ContainedPicture>();
    public ICollection<ContainedReceipt> ContainedReceipts { get; set; } = new List<ContainedReceipt>();
    public ICollection<ContainedTreatment> ContainedTreatments { get; set; } = new List<ContainedTreatment>();
    public ICollection<ContainedUsedEquipment> ContainedUsedEquipments { get; set; } = new List<ContainedUsedEquipment>();
}
