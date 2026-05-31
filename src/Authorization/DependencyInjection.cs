using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Authorization;

/// <summary>Extension methods for registering NDB Platform authorization services.</summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers ASP.NET Core authorization with a default and fallback policy that both require an authenticated user.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional additional configuration for <see cref="AuthorizationOptions"/>.</param>
    /// <remarks>
    /// Default behavior after calling <c>AddNdbAuthorization()</c>:
    /// <list type="bullet">
    /// <item>All endpoints require an authenticated user unless decorated with <c>[AllowAnonymous]</c>.</item>
    /// <item><see cref="AuthorizationOptions.FallbackPolicy"/> requires authentication — endpoints without <c>[Authorize]</c> are still protected.</item>
    /// <item>Additional named policies can be registered via the <paramref name="configure"/> delegate.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddNdbAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions>? configure = null)
    {
        services.AddAuthorization(opts =>
        {
            // Default policy: [Authorize] without further specification requires an authenticated user
            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Fallback policy: endpoints without any [Authorize] attribute still require authentication
            // ASP.NET Core respects [AllowAnonymous] even when a fallback policy is set
            opts.FallbackPolicy = opts.DefaultPolicy;

            configure?.Invoke(opts);
        });
        return services;
    }

    /// <summary>
    /// Registrasi granular permission authorization via <see cref="PermissionPolicyProvider"/>
    /// dan <see cref="PermissionAuthorizationHandler"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Konfigurasi opsional untuk superadmin bypass (<see cref="NdbPermissionOptions"/>).</param>
    /// <remarks>
    /// Panggil SETELAH <see cref="AddNdbAuthorization"/>.
    /// Konsumer wajib register implementasi <see cref="NDB.Platform.Abstraction.Security.IPermissionResolver"/>
    /// di project mereka sendiri.
    /// <code>
    /// services.AddNdbAuthorization();
    /// services.AddNdbPermissionAuthorization(opt =>
    /// {
    ///     opt.BypassRoles = ["SUPER_ADMIN"];
    /// });
    /// services.AddScoped&lt;IPermissionResolver, MyPermissionResolver&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddNdbPermissionAuthorization(
        this IServiceCollection services,
        Action<NdbPermissionOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<NdbPermissionOptions>(_ => { });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
