using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ServiceGroup
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string? Name { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
