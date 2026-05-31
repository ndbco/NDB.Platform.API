using Microsoft.AspNetCore.Authorization;

namespace NDB.Platform.Api.Authorization;

/// <summary>
/// ASP.NET Core authorization requirement for a granular permission key.
/// Used by <see cref="PermissionPolicyProvider"/> and <see cref="PermissionAuthorizationHandler"/>.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>The permission key being required (e.g. <c>"users.create"</c>, <c>"reports.execute"</c>).</summary>
    public string PermissionKey { get; }

    /// <summary>Creates a new requirement for the given permission key.</summary>
    public PermissionRequirement(string permissionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionKey);
        PermissionKey = permissionKey;
    }
}
