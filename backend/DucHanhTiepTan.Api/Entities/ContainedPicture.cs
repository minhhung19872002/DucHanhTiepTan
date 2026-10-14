using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedPicture
{
    public long Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public byte[] Image { get; set; } = Array.Empty<byte>();
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }

    public Appointment? Appointment { get; set; }
}
