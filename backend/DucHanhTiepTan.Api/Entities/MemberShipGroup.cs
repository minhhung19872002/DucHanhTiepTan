using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class MemberShipGroup
{
    public int Id { get; set; }
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    [MaxLength(150)]
    public string? Description { get; set; }
    public bool IsFixed { get; set; }
    public double FixedValue { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<ServiceDiscount> ServiceDiscounts { get; set; } = new List<ServiceDiscount>();
}
