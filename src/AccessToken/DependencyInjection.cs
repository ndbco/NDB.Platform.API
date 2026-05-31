using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Http;

namespace NDB.Platform.Api.AccessToken;

/// <summary>Extension methods for registering the HTTP context-based access token provider.</summary>
public static class AccessTokenExtensions
{
    /// <summary>Registers <see cref="HttpContextAccessTokenProvider"/> as <see cref="IAccessTokenProvider"/> (Scoped).</summary>
    public static IServiceCollection AddNdbAccessTokenProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAccessTokenProvider, HttpContextAccessTokenProvider>();
        return services;
    }
}
