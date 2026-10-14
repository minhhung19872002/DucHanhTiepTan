using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Service
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public long Price { get; set; }
    [MaxLength(250)]
    public string? Description { get; set; }
    public int GroupId { get; set; }

    public ServiceGroup? Group { get; set; }
    public ICollection<ContainedConsulting> ContainedConsultings { get; set; } = new List<ContainedConsulting>();
    public ICollection<ContainedDiscount> ContainedDiscounts { get; set; } = new List<ContainedDiscount>();
    public ICollection<ContainedTreatment> ContainedTreatments { get; set; } = new List<ContainedTreatment>();
    public ICollection<ServiceDiscount> ServiceDiscounts { get; set; } = new List<ServiceDiscount>();
}
