using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly ICurrentUser _user;

    public AuthController(AppDbContext db, JwtTokenService jwt, ICurrentUser user)
    {
        _db = db; _jwt = jwt; _user = user;
    }

    public record LoginRequest(string UserName, string Password);
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
    public record UserInfo(string UserName, string? FullName, bool IsAdministrator, int BranchId, string BranchName);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var account = await _db.Accounts.Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.UserName == request.UserName);
        if (account is null || !Md5Hasher.Verify(request.Password, account.Password))
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        var (token, expires) = _jwt.CreateToken(account);
        return Ok(new
        {
            token,
            expires,
            user = new UserInfo(account.UserName, account.FullName, account.IsAdministrator, account.BranchId, account.Branch?.Name ?? string.Empty),
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> Me()
    {
        var account = await _db.Accounts.Include(a => a.Branch).FirstOrDefaultAsync(a => a.UserName == _user.UserName);
        if (account is null) return Unauthorized();
        return new UserInfo(account.UserName, account.FullName, account.IsAdministrator, account.BranchId, account.Branch?.Name ?? string.Empty);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.UserName == _user.UserName);
        if (account is null) return Unauthorized();
        if (!Md5Hasher.Verify(request.OldPassword, account.Password))
            return BadRequest(new { message = "Mật khẩu cũ không đúng" });
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Mật khẩu mới không được để trống" });
        account.Password = Md5Hasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
