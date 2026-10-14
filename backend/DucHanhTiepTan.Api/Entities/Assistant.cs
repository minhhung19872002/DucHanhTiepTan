using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Assistant
{
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    public Employee? User { get; set; }
    public ICollection<ContainedTreatment> ContainedTreatments { get; set; } = new List<ContainedTreatment>();
}
