using FluentAssertions;
using Microsoft.Extensions.Options;
using NDB.Platform.Api.Authentication;
using NDB.Platform.Http.Resilience;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

// ── FIX 4 (C-03): DefaultTokenRefresher configurable endpoint tests ──

public sealed class DefaultTokenRefresherConfigTests
{
    [Fact]
    public async Task RefreshAsync_UsesConfiguredEndpoint_AbsoluteUri()
    {
        // Absolute endpoint must be used directly
        var capturedUri = (Uri?)null;
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("old-access", "old-refresh");

        var handler = new CapturingHandler(System.Net.HttpStatusCode.Unauthorized, "{}", uri => capturedUri = uri);
        var factory = new StubHttpClientFactory(handler);
        var opts = Options.Create(new NdbJwtOptions
        {
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RefreshEndpoint = "https://auth.example.com/api/refresh",
            RefreshBaseAddress = null
        });

        var refresher = new DefaultTokenRefresher(factory, storage, opts);
        await refresher.RefreshAsync();

        // Should use absolute endpoint directly
        capturedUri.Should().NotBeNull();
        capturedUri!.ToString().Should().Be("https://auth.example.com/api/refresh");
    }

    [Fact]
    public async Task RefreshAsync_UsesBaseAddress_WhenEndpointIsRelative()
    {
        var capturedUri = (Uri?)null;
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("old-access", "old-refresh");

        var handler = new CapturingHandler(System.Net.HttpStatusCode.Unauthorized, "{}", uri => capturedUri = uri);
        var factory = new StubHttpClientFactory(handler);
        var opts = Options.Create(new NdbJwtOptions
        {
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RefreshEndpoint = "/api/v1/auth/refresh",
            RefreshBaseAddress = "https://auth.myapp.com"
        });

        var refresher = new DefaultTokenRefresher(factory, storage, opts);
        await refresher.RefreshAsync();

        capturedUri.Should().NotBeNull();
        capturedUri!.ToString().Should().Be("https://auth.myapp.com/api/v1/auth/refresh");
    }

    [Fact]
    public void NdbJwtOptions_RefreshClientName_Default_Should_Be_NdbTokenRefresher()
    {
        var opts = new NdbJwtOptions();
        opts.RefreshClientName.Should().Be("NdbTokenRefresher");
    }

    [Fact]
    public void NdbJwtOptions_RefreshEndpoint_Default_Should_Be_RefreshPath()
    {
        var opts = new NdbJwtOptions();
        opts.RefreshEndpoint.Should().Be("/api/v1/auth/refresh");
    }

    [Fact]
    public void NdbJwtOptions_RefreshBaseAddress_Default_Should_Be_Null()
    {
        var opts = new NdbJwtOptions();
        opts.RefreshBaseAddress.Should().BeNull();
    }
}

/// <summary>Handler that captures the request URI before returning a static response.</summary>
internal sealed class CapturingHandler : HttpMessageHandler
{
    private readonly System.Net.HttpStatusCode _status;
    private readonly string _body;
    private readonly Action<Uri?> _capture;

    public CapturingHandler(System.Net.HttpStatusCode status, string body, Action<Uri?> capture)
    {
        _status = status;
        _body = body;
        _capture = capture;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        _capture(req.RequestUri);
        return Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
        });
    }
}
