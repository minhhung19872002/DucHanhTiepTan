using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Expense
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    [MaxLength(50)]
    public string AccountantName { get; set; } = string.Empty;

    public Account? Accountant { get; set; }
    public ICollection<ContainedExpense> ContainedExpenses { get; set; } = new List<ContainedExpense>();
}
