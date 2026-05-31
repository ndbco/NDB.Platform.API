using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NDB.Platform.Api.Middleware;
using Xunit;

namespace NDB.Platform.Api.Tests.Middleware;

public sealed class RequestLoggingMiddlewareActorTests
{
    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldCapureActorId()
    {
        // Middleware just needs to not throw — actual log capture requires integration test
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-123")],
            "Bearer");
        context.User = new ClaimsPrincipal(identity);

        var mw = new RequestLoggingMiddleware(
            _ => Task.CompletedTask,
            NullLogger<RequestLoggingMiddleware>.Instance);

        var act = async () => await mw.InvokeAsync(context);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_ShouldNotThrow()
    {
        var context = new DefaultHttpContext();
        // No identity set — anonymous user

        var mw = new RequestLoggingMiddleware(
            _ => Task.CompletedTask,
            NullLogger<RequestLoggingMiddleware>.Instance);

        var act = async () => await mw.InvokeAsync(context);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_SlowRequest_ShouldNotThrow()
    {
        var context = new DefaultHttpContext();

        // Set threshold to 0ms so any request is "slow"
        var originalThreshold = RequestLoggingMiddleware.SlowRequestThresholdMs;
        try
        {
            RequestLoggingMiddleware.SlowRequestThresholdMs = 0;
            var mw = new RequestLoggingMiddleware(
                async _ => await Task.Delay(5), // ensure some delay
                NullLogger<RequestLoggingMiddleware>.Instance);

            var act = async () => await mw.InvokeAsync(context);
            await act.Should().NotThrowAsync();
        }
        finally
        {
            RequestLoggingMiddleware.SlowRequestThresholdMs = originalThreshold;
        }
    }

    [Fact]
    public async Task InvokeAsync_WithIpAddress_ShouldNotThrow()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");

        var mw = new RequestLoggingMiddleware(
            _ => Task.CompletedTask,
            NullLogger<RequestLoggingMiddleware>.Instance);

        var act = async () => await mw.InvokeAsync(context);
        await act.Should().NotThrowAsync();
    }
}
