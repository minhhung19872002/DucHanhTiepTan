using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Membership
{
    [MaxLength(30)]
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    [MaxLength(250)]
    public string? Description { get; set; }
    public int GroupId { get; set; }

    public MemberShipGroup? Group { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
