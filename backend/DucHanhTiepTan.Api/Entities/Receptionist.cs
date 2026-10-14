using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Receptionist
{
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    public Employee? User { get; set; }
}
