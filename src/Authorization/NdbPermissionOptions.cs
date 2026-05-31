namespace NDB.Platform.Api.Authorization;

/// <summary>
/// Configuration options for NDB Platform permission-based authorization.
/// Used by <see cref="PermissionAuthorizationHandler"/> to configure superadmin bypass behavior.
/// </summary>
public sealed class NdbPermissionOptions
{
    /// <summary>
    /// JWT claim name that marks a user as superadmin. A value of <c>"true"</c> bypasses all permission checks.
    /// Default: <c>"is_superadmin"</c>.
    /// </summary>
    public string SuperAdminClaim { get; set; } = "is_superadmin";

    /// <summary>
    /// Role codes that receive a full bypass (equivalent to superadmin).
    /// Example: <c>["SUPER_ADMIN"]</c>.
    /// Default: empty — only the <see cref="SuperAdminClaim"/> is active.
    /// </summary>
    public IReadOnlyList<string> BypassRoles { get; set; } = [];
}
