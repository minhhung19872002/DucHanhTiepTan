using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Payment
{
    public long PrescriptionId { get; set; }
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public long Amount { get; set; }
    [MaxLength(50)]
    public string ReceptionistName { get; set; } = string.Empty;

    public Account? Receptionist { get; set; }
    public Prescription? Prescription { get; set; }
}
