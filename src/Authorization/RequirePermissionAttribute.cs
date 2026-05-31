using Microsoft.AspNetCore.Authorization;

namespace NDB.Platform.Api.Authorization;

/// <summary>
/// Enforces a granular permission key on a controller action or class.
/// Generates a policy name <c>"perm:{key}"</c> handled by <see cref="PermissionPolicyProvider"/>
/// and <see cref="PermissionAuthorizationHandler"/>.
/// </summary>
/// <example>
/// <code>
/// [RequirePermission("users.create")]
/// public IActionResult CreateUser(...) { ... }
///
/// [RequirePermission("reports.execute")]
/// public IActionResult RunReport(...) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates the attribute with the required permission key.
    /// </summary>
    /// <param name="permissionKey">The permission key to require (e.g. <c>"users.create"</c>, <c>"reports.execute"</c>).</param>
    public RequirePermissionAttribute(string permissionKey)
        : base($"{PermissionPolicyProvider.PolicyPrefix}{permissionKey}") { }
}
