using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Equipment
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Serial { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Position { get; set; }
    [MaxLength(150)]
    public string? Usage { get; set; }
    [MaxLength(150)]
    public string? Note { get; set; }
    [MaxLength(50)]
    public string Vendor { get; set; } = string.Empty;
    public bool IsVarious { get; set; }
    public long Price { get; set; }
    [MaxLength(50)]
    public string? Unit { get; set; }

    public ICollection<ContainedEquipmentExportInvoice> ContainedEquipmentExportInvoices { get; set; } = new List<ContainedEquipmentExportInvoice>();
    public ICollection<ContainedEquipmentInvoice> ContainedEquipmentInvoices { get; set; } = new List<ContainedEquipmentInvoice>();
    public ICollection<ContainedUsedEquipment> ContainedUsedEquipments { get; set; } = new List<ContainedUsedEquipment>();
    public ICollection<EquimentInBranch> EquimentInBranches { get; set; } = new List<EquimentInBranch>();
}
