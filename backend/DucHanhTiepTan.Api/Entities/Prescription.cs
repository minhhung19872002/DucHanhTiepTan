using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Prescription
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public bool HasDone { get; set; }
    [MaxLength(250)]
    public string? Treatment { get; set; }
    [MaxLength(250)]
    public string? Diagnostic { get; set; }
    [MaxLength(250)]
    public string? Symptoms { get; set; }
    public DateTime CreatedDate { get; set; }
    [MaxLength(30)]
    public string? MembershipId { get; set; }
    public double DiscountValue { get; set; }

    public Customer? Customer { get; set; }
    public Membership? Membership { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ContainedDiscount> ContainedDiscounts { get; set; } = new List<ContainedDiscount>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
