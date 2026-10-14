namespace DucHanhTiepTan.Api.Entities;

public class ServiceDiscount
{
    public int MemberShipGroupId { get; set; }
    public int ServiceId { get; set; }
    public double Value { get; set; }

    public MemberShipGroup? MemberShipGroup { get; set; }
    public Service? Service { get; set; }
}
