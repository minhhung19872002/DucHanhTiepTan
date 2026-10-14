using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Catalog;

/// <summary>Login accounts ("Tài khoản") – legacy AccountManagerControl / DeclareAccount / PasswordChangerDialog (admin rule).</summary>
[ApiController]
[Route("api/accounts")]
[Authorize(Policy = "Admin")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;
    public AccountsController(AppDbContext db, ICurrentUser user) { _db = db; _user = user; }

    public record AccountDto(string UserName, string? FullName, bool IsAdministrator, int BranchId, string BranchName, string? EmployeeUserName, string? EmployeeFullName);
    public record AccountCreateInput(string UserName, string Password, string? FullName, bool IsAdministrator, int BranchId, string? EmployeeUserName);
    public record AccountUpdateInput(string? FullName, bool IsAdministrator, int BranchId, string? EmployeeUserName);
    public record ResetPasswordInput(string NewPassword);

    private static IQueryable<AccountDto> Project(IQueryable<Account> q) => q.Select(a => new AccountDto(
        a.UserName, a.FullName, a.IsAdministrator, a.BranchId, a.Branch!.Name, a.EmployeeUserName, a.EmployeeUser != null ? a.EmployeeUser.FullName : null));

    /// <summary>GET api/accounts?branchId=&amp;keyword=</summary>
    [HttpGet]
    public async Task<List<AccountDto>> List([FromQuery] int? branchId, [FromQuery] string? keyword)
    {
        var q = _db.Accounts.AsNoTracking().AsQueryable();
        if (branchId.HasValue && branchId.Value != 0) q = q.Where(a => a.BranchId == branchId.Value);
        q = q.WhereUnaccentLike(a => a.UserName, keyword);
        return await Project(q.OrderBy(a => a.UserName)).ToListAsync();
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<AccountDto>> Get(string userName)
    {
        var dto = await Project(_db.Accounts.AsNoTracking().Where(a => a.UserName == userName)).FirstOrDefaultAsync();
        return dto ?? throw new NotFoundException("Không tìm thấy tài khoản");
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(AccountCreateInput input)
    {
        if (input.BranchId == 0) throw new ApiException("Vui lòng chọn chi nhánh.");
        if (string.IsNullOrWhiteSpace(input.UserName)) throw new ApiException("Vui lòng điền tên tài khoản.");
        if (string.IsNullOrEmpty(input.Password)) throw new ApiException("Vui lòng điền mật khẩu.");
        var userName = input.UserName.Trim();
        if (await _db.Accounts.AnyAsync(a => a.UserName == userName)) throw new ApiException("Tên tài khoản đã được sử dụng.");
        await ValidateEmployeeAsync(input.EmployeeUserName);

        var a = new Account
        {
            UserName = userName,
            Password = Md5Hasher.Hash(input.Password),
            FullName = string.IsNullOrWhiteSpace(input.FullName) ? null : input.FullName.Trim(),
            IsAdministrator = input.IsAdministrator,
            BranchId = input.BranchId,
            EmployeeUserName = string.IsNullOrWhiteSpace(input.EmployeeUserName) ? null : input.EmployeeUserName,
        };
        _db.Accounts.Add(a);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { userName = a.UserName }, (await Get(a.UserName)).Value);
    }

    [HttpPut("{userName}")]
    public async Task<ActionResult<AccountDto>> Update(string userName, AccountUpdateInput input)
    {
        if (input.BranchId == 0) throw new ApiException("Vui lòng chọn chi nhánh.");
        var a = await _db.Accounts.FindAsync(userName) ?? throw new NotFoundException("Không tìm thấy tài khoản");
        await ValidateEmployeeAsync(input.EmployeeUserName);
        if (a.UserName == _user.UserName && !input.IsAdministrator) throw new ApiException("Không thể bỏ quyền quản trị của chính mình.");
        a.FullName = string.IsNullOrWhiteSpace(input.FullName) ? null : input.FullName.Trim();
        a.IsAdministrator = input.IsAdministrator;
        a.BranchId = input.BranchId;
        a.EmployeeUserName = string.IsNullOrWhiteSpace(input.EmployeeUserName) ? null : input.EmployeeUserName;
        await _db.SaveChangesAsync();
        return (await Get(userName)).Value!;
    }

    /// <summary>Admin "Đổi mật khẩu" (no old password required).</summary>
    [HttpPost("{userName}/reset-password")]
    public async Task<IActionResult> ResetPassword(string userName, ResetPasswordInput input)
    {
        if (string.IsNullOrEmpty(input.NewPassword)) throw new ApiException("Vui lòng điền mật khẩu mới.");
        var a = await _db.Accounts.FindAsync(userName) ?? throw new NotFoundException("Không tìm thấy tài khoản");
        a.Password = Md5Hasher.Hash(input.NewPassword);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{userName}")]
    public async Task<IActionResult> Delete(string userName)
    {
        var a = await _db.Accounts.FindAsync(userName) ?? throw new NotFoundException("Không tìm thấy tài khoản");
        if (a.UserName == _user.UserName) throw new ApiException("Không thể xóa tài khoản đang đăng nhập.");
        var inUse = await _db.Invoices.AnyAsync(i => i.UserName == userName)
            || await _db.Payments.AnyAsync(p => p.ReceptionistName == userName)
            || await _db.Expenses.AnyAsync(e => e.AccountantName == userName);
        if (inUse) throw new ApiException("Không thể xóa tài khoản này được, có thể nó đang được sử dụng.");
        _db.Accounts.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ValidateEmployeeAsync(string? employeeUserName)
    {
        if (string.IsNullOrWhiteSpace(employeeUserName)) return;
        if (!await _db.Employees.AnyAsync(e => e.UserName == employeeUserName)) throw new ApiException("Nhân viên liên kết không tồn tại.");
    }
}
