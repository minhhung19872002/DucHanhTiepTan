namespace DucHanhTiepTan.Api.Entities;

public class EquimentInBranch
{
    public int BranchId { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; }

    public Branch? Branch { get; set; }
    public Equipment? Equipment { get; set; }
}
