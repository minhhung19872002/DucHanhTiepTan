using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class ContainedExpense
{
    public int ExpenseId { get; set; }
    [MaxLength(150)]
    public string Description { get; set; } = string.Empty;
    public long Amount { get; set; }

    public Expense? Expense { get; set; }
}
