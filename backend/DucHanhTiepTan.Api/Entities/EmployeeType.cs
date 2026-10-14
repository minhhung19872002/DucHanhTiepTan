using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class EmployeeType
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
