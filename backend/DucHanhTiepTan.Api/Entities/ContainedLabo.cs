using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedLabo
{
    public long PrescriptionId { get; set; }
    public int AppointmentNo { get; set; }
    public int SubId { get; set; }
    public int LaboVendorId { get; set; }
    public int LaboTypeId { get; set; }
    [MaxLength(50)]
    public string? Color { get; set; }
    public DateTime ReceiverDate { get; set; }
    public DateTime DeliverDate { get; set; }
    public bool Delivered { get; set; }
    public long Price { get; set; }

    public ContainedTreatment? ContainedTreatment { get; set; }
    public LaboType? LaboType { get; set; }
    public LaboVendor? LaboVendor { get; set; }
}
