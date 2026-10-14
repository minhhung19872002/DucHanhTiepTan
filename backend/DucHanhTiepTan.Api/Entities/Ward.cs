using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Ward
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string DistrictName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string ProvinceName { get; set; } = string.Empty;

}
