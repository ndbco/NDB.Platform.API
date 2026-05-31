using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NDB.Platform.Api.Swagger;

/// <summary>Extension methods for registering and enabling Swashbuckle Swagger.</summary>
public static class SwaggerExtensions
{
    /// <summary>Registers Swashbuckle with NDB standard options.</summary>
    public static IServiceCollection AddNdbSwagger(
        this IServiceCollection services,
        Action<NdbSwaggerOptions>? configure = null)
    {
        var opts = new NdbSwaggerOptions();
        configure?.Invoke(opts);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(opts.Version, new OpenApiInfo
            {
                Title = opts.Title,
                Version = opts.Version,
                Description = opts.Description
            });

            if (opts.JwtAuthEnabled)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter your JWT token here. Format: Bearer {token}"
                });
                c.OperationFilter<SwaggerJwtSecuritySchemeFilter>();
            }
        });

        return services;
    }

    /// <summary>Adds the Swagger UI middleware to the pipeline.</summary>
    public static IApplicationBuilder UseNdbSwaggerUI(
        this IApplicationBuilder app,
        Action<NdbSwaggerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var opts = new NdbSwaggerOptions();
        configure?.Invoke(opts);

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{opts.Version}/swagger.json", $"{opts.Title} {opts.Version}");
            c.RoutePrefix = opts.RoutePrefix;
        });

        return app;
    }
}
