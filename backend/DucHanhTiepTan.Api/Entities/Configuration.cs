using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Configuration
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(150)]
    public string? Value { get; set; }

}
