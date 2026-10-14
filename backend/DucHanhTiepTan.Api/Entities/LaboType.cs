using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class LaboType
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(250)]
    public string? Description { get; set; }
    public bool Disassembly { get; set; }

    public ICollection<ContainedConsultingLabo> ContainedConsultingLabos { get; set; } = new List<ContainedConsultingLabo>();
    public ICollection<ContainedLabo> ContainedLabos { get; set; } = new List<ContainedLabo>();
}
