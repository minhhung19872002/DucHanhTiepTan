using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Province
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

}
