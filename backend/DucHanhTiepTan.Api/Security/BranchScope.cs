namespace DucHanhTiepTan.Api.Security;

/// <summary>
/// Resolves the effective branch filter (legacy BranchFilter.Current):
/// receptionists are always limited to their own branch; administrators may request any branch or 0 = all.
/// </summary>
public static class BranchScope
{
    public static int Resolve(ICurrentUser user, int? requestedBranchId)
    {
        if (!user.IsAdministrator) return user.BranchId;
        return requestedBranchId ?? 0;
    }

    /// <summary>True when the row's branch is visible for the resolved scope.</summary>
    public static bool Matches(int scopeBranchId, int rowBranchId) => scopeBranchId == 0 || scopeBranchId == rowBranchId;
}
