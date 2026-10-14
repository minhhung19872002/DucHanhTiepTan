using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Finance;

/// <summary>"Phiếu chi" (legacy ExpenseManagerControl + DeclareExpenseDialog): Expense header + ContainedExpense lines.</summary>
[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public ExpensesController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    public record ExpenseLineDto(string Description, long Amount);
    public record ExpenseDto(int Id, DateTime CreatedDate, string AccountantName, string? AccountantFullName, string BranchName, int BranchId, long Total);
    public record ExpenseDetailDto(int Id, DateTime CreatedDate, string AccountantName, string? AccountantFullName, string BranchName, int BranchId, long Total, List<ExpenseLineDto> Items);
    public record ExpenseListDto(List<ExpenseDto> Items, long Total, int Count);
    public record ExpenseInput(DateTime? CreatedDate, List<ExpenseLineDto> Items);

    private IQueryable<ExpenseView> Query(int? branchId, DateTime? from, DateTime? to)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var q = _db.ExpenseViews.AsQueryable();
        if (scope != 0) q = q.Where(r => r.BranchId == scope);
        if (from.HasValue) { var f = from.Value.Date; q = q.Where(r => r.CreatedDate >= f); }
        if (to.HasValue) { var t = to.Value.Date.AddDays(1); q = q.Where(r => r.CreatedDate < t); }
        return q;
    }

    private static ExpenseDto Map(ExpenseView r) => new(r.Id, r.CreatedDate, r.AccountantName, r.AccountantFullName, r.BranchName, r.BranchId, r.Total);

    [HttpGet]
    public async Task<ExpenseListDto> List([FromQuery] int? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int limit = 500)
    {
        var q = Query(branchId, from, to);
        var total = await q.SumAsync(r => (long?)r.Total) ?? 0;
        var count = await q.CountAsync();
        var items = await q.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id).Take(Math.Clamp(limit, 1, 5000)).ToListAsync();
        return new ExpenseListDto(items.Select(Map).ToList(), total, count);
    }

    [HttpGet("{id:int}")]
    public async Task<ExpenseDetailDto> Get(int id)
    {
        var view = await _db.ExpenseViews.FirstOrDefaultAsync(r => r.Id == id) ?? throw new NotFoundException("Không tìm thấy phiếu chi");
        var items = await _db.ContainedExpenses.Where(r => r.ExpenseId == id).OrderBy(r => r.Description)
            .Select(r => new ExpenseLineDto(r.Description, r.Amount)).ToListAsync();
        return new ExpenseDetailDto(view.Id, view.CreatedDate, view.AccountantName, view.AccountantFullName, view.BranchName, view.BranchId, view.Total, items);
    }

    private static List<ExpenseLineDto> ValidateLines(ExpenseInput input)
    {
        if (input.Items is null || input.Items.Count == 0) throw new ApiException("Phiếu chi phải có ít nhất một dòng.");
        var lines = new List<ExpenseLineDto>();
        foreach (var item in input.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Description)) throw new ApiException("Vui lòng điền nội dung.");
            if (item.Amount <= 0) throw new ApiException("Chi phí phải lớn hơn 0.");
            lines.Add(new ExpenseLineDto(item.Description.Trim(), item.Amount));
        }
        var dup = lines.GroupBy(l => l.Description).FirstOrDefault(g => g.Count() > 1);
        if (dup is not null) throw new ApiException($"\"{dup.Key}\" bị trùng");
        return lines;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDetailDto>> Create(ExpenseInput input)
    {
        var lines = ValidateLines(input);
        var expense = new Expense
        {
            CreatedDate = (input.CreatedDate ?? DateTime.Now).Date,
            AccountantName = _user.UserName,
        };
        foreach (var l in lines) expense.ContainedExpenses.Add(new ContainedExpense { Description = l.Description, Amount = l.Amount });
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = expense.Id }, await Get(expense.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ExpenseInput input)
    {
        var lines = ValidateLines(input);
        var expense = await _db.Expenses.Include(e => e.ContainedExpenses).Include(e => e.Accountant).FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Không tìm thấy phiếu chi");
        if (!_user.IsAdministrator && expense.Accountant!.BranchId != _user.BranchId) throw new ApiException("Phiếu chi thuộc chi nhánh khác");
        if (input.CreatedDate.HasValue) expense.CreatedDate = input.CreatedDate.Value.Date;
        _db.ContainedExpenses.RemoveRange(expense.ContainedExpenses);
        expense.ContainedExpenses.Clear();
        foreach (var l in lines) expense.ContainedExpenses.Add(new ContainedExpense { ExpenseId = id, Description = l.Description, Amount = l.Amount });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.Include(e => e.ContainedExpenses).Include(e => e.Accountant).FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException("Không tìm thấy phiếu chi");
        if (!_user.IsAdministrator && expense.Accountant!.BranchId != _user.BranchId) throw new ApiException("Phiếu chi thuộc chi nhánh khác");
        _db.ContainedExpenses.RemoveRange(expense.ContainedExpenses);
        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var heads = await Query(branchId, from, to).OrderBy(r => r.CreatedDate).ThenBy(r => r.Id).ToListAsync();
        var ids = heads.Select(h => h.Id).ToList();
        var lines = await _db.ContainedExpenses.Where(l => ids.Contains(l.ExpenseId)).ToListAsync();
        var rows = heads.SelectMany(h => lines.Where(l => l.ExpenseId == h.Id).DefaultIfEmpty().Select(l => (h, l))).ToList();
        var bytes = ExcelExporter.Build("Phieu chi", rows, new ExcelExporter.Column<(ExpenseView h, ContainedExpense? l)>[]
        {
            new("Số phiếu", r => r.h.Id),
            new("Ngày", r => r.h.CreatedDate),
            new("Chi nhánh", r => r.h.BranchName),
            new("Người chi", r => r.h.AccountantFullName ?? r.h.AccountantName),
            new("Nội dung", r => r.l?.Description),
            new("Số tiền", r => r.l?.Amount),
            new("Tổng phiếu", r => r.h.Total),
        }, "Danh sách chi");
        return File(bytes, ExcelExporter.ContentType, "phieu-chi.xlsx");
    }
}
