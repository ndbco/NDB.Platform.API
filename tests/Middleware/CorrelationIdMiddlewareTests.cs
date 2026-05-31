using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NDB.Platform.Api.Middleware;
using NDB.Platform.Kit.Identifiers;
using Xunit;

namespace NDB.Platform.Api.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Add_Correlation_Id_To_Response()
    {
        var mw = new CorrelationIdMiddleware(
            _ => Task.CompletedTask,
            NullLogger<CorrelationIdMiddleware>.Instance);
        var ctx = new DefaultHttpContext();
        await mw.InvokeAsync(ctx);
        ctx.Response.Headers.ContainsKey(CorrelationId.HeaderName).Should().BeTrue();
        ctx.Response.Headers[CorrelationId.HeaderName].ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_Should_Propagate_Existing_Correlation_Id()
    {
        var mw = new CorrelationIdMiddleware(
            _ => Task.CompletedTask,
            NullLogger<CorrelationIdMiddleware>.Instance);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[CorrelationId.HeaderName] = "existing-123";
        await mw.InvokeAsync(ctx);
        ctx.Response.Headers[CorrelationId.HeaderName].ToString().Should().Be("existing-123");
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_New_Id_When_None_In_Request()
    {
        var mw = new CorrelationIdMiddleware(
            _ => Task.CompletedTask,
            NullLogger<CorrelationIdMiddleware>.Instance);
        var ctx = new DefaultHttpContext();
        await mw.InvokeAsync(ctx);
        var id = ctx.Response.Headers[CorrelationId.HeaderName].ToString();
        id.Should().NotBeNullOrEmpty();
        id.Should().HaveLength(32); // Guid.NewGuid().ToString("N")
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_Delegate()
    {
        var called = false;
        var mw = new CorrelationIdMiddleware(
            _ => { called = true; return Task.CompletedTask; },
            NullLogger<CorrelationIdMiddleware>.Instance);
        await mw.InvokeAsync(new DefaultHttpContext());
        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Should_Not_Throw()
    {
        var mw = new CorrelationIdMiddleware(
            _ => Task.CompletedTask,
            NullLogger<CorrelationIdMiddleware>.Instance);
        var act = async () => await mw.InvokeAsync(new DefaultHttpContext());
        await act.Should().NotThrowAsync();
    }
}
