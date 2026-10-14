using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class YesNoQuestion
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string Header { get; set; } = string.Empty;

}
