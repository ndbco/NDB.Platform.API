using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Middleware;

/// <summary>Extension methods for registering and enabling the NDB middleware pipeline.</summary>
public static class MiddlewareExtensions
{
    /// <summary>Registers NDB middleware services — global exception handler and problem details support.</summary>
    public static IServiceCollection AddNdbMiddleware(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    /// <summary>Adds the NDB middleware components to the ASP.NET Core pipeline.</summary>
    public static IApplicationBuilder UseNdbMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseExceptionHandler();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
