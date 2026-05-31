using Microsoft.AspNetCore.Authorization;

namespace NDB.Platform.Api.Authorization;

/// <summary>
/// Shorthand attribute for role-based authorization. Equivalent to <c>[Authorize(Roles = "ROLE1,ROLE2")]</c>.
/// You can also use ASP.NET's built-in <c>[Authorize(Roles = "ADMIN,SUPERADMIN")]</c> directly.
/// </summary>
/// <remarks>
/// Examples:
/// <code>
/// [NdbRequireRole("ADMIN")]
/// public IActionResult AdminOnly() { ... }
///
/// [NdbRequireRole("ADMIN", "SUPERADMIN")]
/// public IActionResult AdminOrSuperAdmin() { ... }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class NdbRequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates the attribute with one or more allowed roles.
    /// </summary>
    /// <param name="roles">Allowed roles (case-insensitive, matching ASP.NET Core behavior).</param>
    /// <exception cref="ArgumentException">Thrown if no roles are provided.</exception>
    public NdbRequireRoleAttribute(params string[] roles)
    {
        if (roles is null || roles.Length == 0)
            throw new ArgumentException("At least one role is required.", nameof(roles));
        Roles = string.Join(',', roles);
    }
}
