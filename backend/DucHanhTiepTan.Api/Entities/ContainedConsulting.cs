using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedConsulting
{
    public long PrescriptionId { get; set; }
    public int SubId { get; set; }
    public int ServiceId { get; set; }
    public long OriginalPrice { get; set; }
    public long Price { get; set; }
    public DateTime CreatedDate { get; set; }
    [MaxLength(250)]
    public string? Comment { get; set; }
    public int zQuantity { get; set; }
    [MaxLength(150)]
    public string? ToothPositions { get; set; }

    public Service? Service { get; set; }
}
