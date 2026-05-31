using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NDB.Platform.Abstraction;
using NDB.Platform.Http.Resilience;
using System.Text;

namespace NDB.Platform.Api.Authentication;

/// <summary>Extension methods for registering JWT authentication services.</summary>
public static class AuthenticationExtensions
{
    /// <summary>Registers JWT Bearer authentication, <see cref="ITokenIssuer"/>, <see cref="ITokenStorage"/>, and <see cref="ITokenRefresher"/>.</summary>
    public static IServiceCollection AddNdbJwt(
        this IServiceCollection services,
        Action<NdbJwtOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var opts = new NdbJwtOptions();
        configure(opts);
        services.Configure(configure);

        // Fail-fast: validate NdbJwtOptions at startup before serving any request
        services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<NdbJwtOptions>, NdbJwtOptionsValidator>();
        services.AddOptions<NdbJwtOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.AddScoped<NdbJwtBearerEvents>();
        services.AddSingleton<ITokenIssuer, JwtTokenService>();

        // Token storage + refresher for BaseApiService auto-refresh support
        services.AddScoped<ITokenStorage, InMemoryTokenStorage>();
        services.AddScoped<ITokenRefresher, DefaultTokenRefresher>();

        // Register named HttpClient for DefaultTokenRefresher with the configured base address
        var clientBuilder = services.AddHttpClient(opts.RefreshClientName);
        if (!string.IsNullOrWhiteSpace(opts.RefreshBaseAddress))
        {
            clientBuilder.ConfigureHttpClient(c =>
                c.BaseAddress = new Uri(opts.RefreshBaseAddress));
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = opts.RequireHttpsMetadata;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey)),
                    ValidateIssuer = true,
                    ValidIssuer = opts.Issuer,
                    ValidateAudience = true,
                    ValidAudience = opts.Audience,
                    ValidateLifetime = true,
                    ClockSkew = opts.ClockSkew
                };
                o.EventsType = typeof(NdbJwtBearerEvents);
            });

        services.AddAuthorization();

        // Register IHttpContextAccessor and the web-aware IActorAccessor implementation
        // Override the SystemActorAccessor fallback (registered by AddNdbCqrs) with the HTTP context-aware version
        services.AddHttpContextAccessor();
        services.AddScoped<IActorAccessor, HttpContextActorAccessor>();

        return services;
    }
}
