using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Cors;

/// <summary>Extension methods for registering CORS policy.</summary>
public static class CorsExtensions
{
    /// <summary>
    /// Registers a CORS policy from <see cref="NdbCorsOptions"/>.
    /// Calls <see cref="NdbCorsOptions.Validate"/> before registration to reject unsafe configurations.
    /// </summary>
    public static IServiceCollection AddNdbCors(
        this IServiceCollection services,
        Action<NdbCorsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var opts = new NdbCorsOptions();
        configure(opts);

        // Fail-fast: reject AllowCredentials + AllowAnyOrigin combination (forbidden by CORS spec)
        opts.Validate();

        services.AddCors(corsOpts =>
        {
            corsOpts.AddPolicy(opts.PolicyName, policy =>
            {
                if (opts.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(opts.AllowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                if (opts.AllowAnyHeader)
                    policy.AllowAnyHeader();

                if (opts.AllowAnyMethod)
                    policy.AllowAnyMethod();

                if (opts.AllowCredentials && opts.AllowedOrigins.Length > 0)
                    policy.AllowCredentials();
            });
        });

        return services;
    }
}
