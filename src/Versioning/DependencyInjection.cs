using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Versioning;

/// <summary>Extension methods for registering API versioning.</summary>
public static class VersioningExtensions
{
    /// <summary>
    /// Registers API versioning with NDB defaults: URL segment reader, default version <c>1.0</c>,
    /// <c>AssumeDefaultVersionWhenUnspecified = true</c>, and <c>ReportApiVersions = true</c>.
    /// </summary>
    public static IServiceCollection AddNdbApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(opts =>
        {
            opts.DefaultApiVersion = new ApiVersion(1, 0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ReportApiVersions = true;
            opts.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
