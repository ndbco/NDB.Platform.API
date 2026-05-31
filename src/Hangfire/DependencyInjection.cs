using global::Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Hangfire;

/// <summary>Extension methods for registering Hangfire (provider-agnostic wrapper).</summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Registers Hangfire with the given options and a storage configuration callback.
    /// No storage provider is included in <c>NDB.Platform.Api</c> — the consuming project wires it via <paramref name="storageCallback"/>.
    /// </summary>
    public static IServiceCollection AddNdbHangfire(
        this IServiceCollection services,
        Action<NdbHangfireOptions> configure,
        Action<IGlobalConfiguration> storageCallback)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(storageCallback);

        var opts = new NdbHangfireOptions();
        configure(opts);
        services.Configure(configure);

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            storageCallback(config);
        });

        services.AddHangfireServer(serverOpts =>
        {
            serverOpts.WorkerCount = opts.WorkerCount;
            serverOpts.Queues = opts.Queues;
        });

        return services;
    }

    /// <summary>
    /// Maps the Hangfire dashboard to the configured URL.
    /// Calls <see cref="NdbHangfireOptions.Validate"/> before registering to ensure the password is not empty.
    /// </summary>
    public static IApplicationBuilder UseNdbHangfireDashboard(
        this IApplicationBuilder app,
        Action<NdbHangfireOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configure);

        var opts = new NdbHangfireOptions();
        configure(opts);

        // Fail-fast: reject empty password before registering the dashboard
        opts.Validate();

        app.UseHangfireDashboard(opts.DashboardUrl, new DashboardOptions
        {
            DashboardTitle = opts.DashboardTitle,
            Authorization = [new NdbHangfireBasicAuthFilter(opts.BasicAuthUser, opts.BasicAuthPassword)],
            IgnoreAntiforgeryToken = true
        });

        return app;
    }
}
