namespace DucHanhTiepTan.Api.Entities;

public class DrugInBranch
{
    public int BranchId { get; set; }
    public int DrugId { get; set; }
    public int Quantity { get; set; }

    public Branch? Branch { get; set; }
    public Drug? Drug { get; set; }
}
