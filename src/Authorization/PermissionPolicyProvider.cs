using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace NDB.Platform.Api.Authorization;

/// <summary>
/// Dynamic policy provider that generates <c>"perm:{key}"</c> policies on demand — no need to
/// pre-register each permission key at startup.
/// Enables <c>[RequirePermission("users.create")]</c> on controllers without any Startup boilerplate.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    internal const string PolicyPrefix = "perm:";
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    /// <summary>Initializes the provider with the <see cref="AuthorizationOptions"/> from the DI container.</summary>
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        => _fallback = new DefaultAuthorizationPolicyProvider(options);

    /// <inheritdoc />
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permKey = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permKey))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return _fallback.GetPolicyAsync(policyName);
    }
}
