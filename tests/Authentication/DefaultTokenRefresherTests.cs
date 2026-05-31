using FluentAssertions;
using Microsoft.Extensions.Options;
using NDB.Platform.Api.Authentication;
using NDB.Platform.Http.Resilience;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

public sealed class DefaultTokenRefresherTests
{
    [Fact]
    public async Task RefreshAsync_NoRefreshToken_ShouldReturnUnauthorized()
    {
        var storage = new InMemoryTokenStorage(); // no tokens set
        var factory = new StubHttpClientFactory(new StaticResponseHandler(
            System.Net.HttpStatusCode.Unauthorized, "{}"));
        var opts = Options.Create(new NdbJwtOptions { AccessTokenLifetime = TimeSpan.FromMinutes(15) });

        var refresher = new DefaultTokenRefresher(factory, storage, opts);
        var result = await refresher.RefreshAsync();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(NDB.Platform.Abstraction.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task RefreshAsync_ServerReturns401_ShouldReturnUnauthorized()
    {
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("old-access", "old-refresh");

        var factory = new StubHttpClientFactory(new StaticResponseHandler(
            System.Net.HttpStatusCode.Unauthorized, "{}"));
        var opts = Options.Create(new NdbJwtOptions { AccessTokenLifetime = TimeSpan.FromMinutes(15) });
        var refresher = new DefaultTokenRefresher(factory, storage, opts);

        var result = await refresher.RefreshAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_NetworkError_ShouldReturnError()
    {
        var storage = new InMemoryTokenStorage();
        storage.SetTokens("old-access", "old-refresh");

        var factory = new StubHttpClientFactory(new NetworkFailingHandler());
        var opts = Options.Create(new NdbJwtOptions { AccessTokenLifetime = TimeSpan.FromMinutes(15) });
        var refresher = new DefaultTokenRefresher(factory, storage, opts);

        var result = await refresher.RefreshAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void DefaultTokenRefresher_ShouldImplementITokenRefresher()
    {
        typeof(DefaultTokenRefresher).Should().Implement<ITokenRefresher>();
    }
}

// Stub IHttpClientFactory that returns pre-configured HttpClient with a dummy BaseAddress
internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;
    public HttpClient CreateClient(string name) => new(_handler)
    {
        BaseAddress = new Uri("http://stub-auth-server")
    };
}

internal sealed class StaticResponseHandler : HttpMessageHandler
{
    private readonly System.Net.HttpStatusCode _status;
    private readonly string _body;

    public StaticResponseHandler(System.Net.HttpStatusCode status, string body)
    {
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct) =>
        Task.FromResult(new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
        });
}

internal sealed class NetworkFailingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct) =>
        Task.FromException<HttpResponseMessage>(new HttpRequestException("Network failure"));
}
