using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Customer
{
    public long Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public int Sex { get; set; }
    public int BirthYear { get; set; }
    [MaxLength(250)]
    public string? Address { get; set; }
    public DateTime CreatedDate { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(50)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Job { get; set; }
    public byte[]? Image { get; set; }
    public int BranchId { get; set; }
    [MaxLength(50)]
    public string? ProvinceName { get; set; }
    [MaxLength(50)]
    public string? DistrictName { get; set; }
    [MaxLength(50)]
    public string? WardName { get; set; }
    public long VisiableId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
