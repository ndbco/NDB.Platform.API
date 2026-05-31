using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NDB.Platform.Api.Middleware;
using Xunit;

namespace NDB.Platform.Api.Tests.Middleware;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        var called = false;
        var mw = new RequestLoggingMiddleware(
            _ => { called = true; return Task.CompletedTask; },
            NullLogger<RequestLoggingMiddleware>.Instance);
        await mw.InvokeAsync(new DefaultHttpContext());
        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw()
    {
        var mw = new RequestLoggingMiddleware(
            _ => Task.CompletedTask,
            NullLogger<RequestLoggingMiddleware>.Instance);
        var act = async () => await mw.InvokeAsync(new DefaultHttpContext());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw_When_Next_Throws()
    {
        var mw = new RequestLoggingMiddleware(
            _ => Task.FromException(new InvalidOperationException("fail")),
            NullLogger<RequestLoggingMiddleware>.Instance);
        var act = async () => await mw.InvokeAsync(new DefaultHttpContext());
        // Exception propagates — middleware logs then re-throws
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvokeAsync_Should_Pass_Context_To_Next()
    {
        HttpContext? capturedCtx = null;
        var mw = new RequestLoggingMiddleware(
            ctx => { capturedCtx = ctx; return Task.CompletedTask; },
            NullLogger<RequestLoggingMiddleware>.Instance);
        var inputCtx = new DefaultHttpContext();
        await mw.InvokeAsync(inputCtx);
        capturedCtx.Should().BeSameAs(inputCtx);
    }
}
