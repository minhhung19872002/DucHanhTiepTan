using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DucHanhTiepTan.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DucHanhTiepTan.Api.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "DucHanhTiepTan";
    public string Audience { get; set; } = "DucHanhTiepTan";
    public string Key { get; set; } = string.Empty;
    public int ExpireHours { get; set; } = 12;
}

public class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options) => _options = options;

    public (string token, DateTime expires) CreateToken(Account account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, account.UserName),
            new(ClaimTypes.Role, account.IsAdministrator ? Roles.Administrator : Roles.Receptionist),
            new("branch_id", account.BranchId.ToString()),
            new("full_name", account.FullName ?? account.UserName),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var expires = DateTime.UtcNow.AddHours(_options.ExpireHours);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
