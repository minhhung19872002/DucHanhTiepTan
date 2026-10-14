using System.Security.Claims;

namespace DucHanhTiepTan.Api.Security;

/// <summary>Strongly typed access to the authenticated user's claims (equivalent of Global.Default.Account in the legacy app).</summary>
public interface ICurrentUser
{
    string UserName { get; }
    string? FullName { get; }
    bool IsAdministrator { get; }
    int BranchId { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor accessor) => _principal = accessor.HttpContext?.User;

    public string UserName => _principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    public string? FullName => _principal?.FindFirstValue("full_name");
    public bool IsAdministrator => _principal?.IsInRole(Roles.Administrator) ?? false;
    public int BranchId => int.TryParse(_principal?.FindFirstValue("branch_id"), out var id) ? id : 0;
}

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Receptionist = "Receptionist";
}
