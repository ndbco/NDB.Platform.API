using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NDB.Platform.Abstraction.Security;
using System.Security.Claims;

namespace NDB.Platform.Api.Authorization;

/// <summary>
/// Authorization handler that checks the user's effective permissions via <see cref="IPermissionResolver"/>.
/// Superadmin bypass behavior is configured via <see cref="NdbPermissionOptions"/>.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionResolver _resolver;
    private readonly NdbPermissionOptions _options;

    /// <summary>Initializes the handler with the permission resolver and options from the DI container.</summary>
    public PermissionAuthorizationHandler(
        IPermissionResolver resolver,
        IOptions<NdbPermissionOptions> options)
    {
        _resolver = resolver;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        // Superadmin bypass: check the configured JWT claim
        var superadminClaim = context.User.FindFirst(_options.SuperAdminClaim)?.Value;
        if (string.Equals(superadminClaim, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        // Superadmin bypass: check the configured bypass roles
        foreach (var role in _options.BypassRoles)
        {
            if (context.User.IsInRole(role))
            {
                context.Succeed(requirement);
                return;
            }
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return;

        if (await _resolver.HasPermissionAsync(userId, requirement.PermissionKey).ConfigureAwait(false))
            context.Succeed(requirement);
    }
}
