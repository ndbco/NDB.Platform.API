using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.ForwardedHeaders;

/// <summary>Extension methods for configuring forwarded header support (reverse proxy).</summary>
public static class ForwardedHeadersExtensions
{
    /// <summary>
    /// Configures forwarded header processing for reverse proxies (Nginx, Traefik, YARP).
    /// Existing <c>KnownProxies</c> and <c>KnownNetworks</c> are preserved by default (ASP.NET Core loopback is retained).
    /// Set <c>ClearExistingKnownHosts = true</c> and provide explicit entries for a strict allow-list.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="NdbForwardedHeadersOptions"/>.</param>
    public static IServiceCollection AddNdbForwardedHeaders(
        this IServiceCollection services,
        Action<NdbForwardedHeadersOptions>? configure = null)
    {
        var ndbOpts = new NdbForwardedHeadersOptions();
        configure?.Invoke(ndbOpts);

        services.Configure<ForwardedHeadersOptions>(opts =>
        {
            opts.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                                  | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                                  | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedPrefix;

            if (ndbOpts.ClearExistingKnownHosts)
            {
#pragma warning disable ASPDEPR005 // KnownNetworks is obsolete in net10 but still needed for net8 compatibility
                opts.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
                opts.KnownProxies.Clear();
            }

            // Add trusted proxies from options
            foreach (var proxy in ndbOpts.KnownProxies)
                opts.KnownProxies.Add(proxy);

            // Add trusted networks from options
            foreach (var network in ndbOpts.KnownNetworks)
            {
#pragma warning disable ASPDEPR005
                opts.KnownNetworks.Add(network);
#pragma warning restore ASPDEPR005
            }
        });

        return services;
    }

    /// <summary>Adds the <c>ForwardedHeaders</c> middleware to the pipeline.</summary>
    public static IApplicationBuilder UseNdbForwardedHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseForwardedHeaders();
    }
}
