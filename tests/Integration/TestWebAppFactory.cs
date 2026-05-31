using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NDB.Platform.Api.Authentication;
using NDB.Platform.Api.Diagnostics;
using NDB.Platform.Api.Hangfire;
using NDB.Platform.Api.Middleware;
using NDB.Platform.Api.Swagger;

namespace NDB.Platform.Api.Tests.Integration;

/// <summary>Minimal web app factory untuk integration test.</summary>
public sealed class TestWebAppFactory : IDisposable
{
    private readonly IHost _host;
    private bool _disposed;

    public TestWebAppFactory()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(ConfigureServices);
                webHost.Configure(Configure);
            })
            .Build();

        _host.StartAsync().GetAwaiter().GetResult();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddControllers();
        services.AddNdbJwt(o =>
        {
            o.Issuer = "test";
            o.Audience = "test";
            o.SigningKey = "test-signing-key-minimum-32-chars!!";
            o.RequireHttpsMetadata = false;
        });
        services.AddNdbSwagger();
        services.AddNdbMiddleware();
        services.AddNdbHealthChecks();
        services.AddNdbHangfire(
            o => { o.DashboardUrl = "/jobs"; },
            cfg => cfg.UseInMemoryStorage());
    }

    private static void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseNdbMiddleware();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseNdbSwaggerUI();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapNdbHealthChecks();
        });
    }

    public HttpClient CreateClient() =>
        _host.GetTestClient();

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}
