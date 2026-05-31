using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NDB.Platform.Api.Middleware;
using Xunit;

namespace NDB.Platform.Api.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Should_Return_True_And_Set_500()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var result = await handler.TryHandleAsync(ctx, new InvalidOperationException("test"), CancellationToken.None);

        result.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Write_Json_Content_Type()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await handler.TryHandleAsync(ctx, new InvalidOperationException("err"), CancellationToken.None);

        ctx.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return_True_For_Any_Exception_Type()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var result1 = await handler.TryHandleAsync(ctx, new ArgumentNullException("arg"), CancellationToken.None);
        result1.Should().BeTrue();

        ctx.Response.Body = new MemoryStream();
        ctx.Response.StatusCode = 200;
        var result2 = await handler.TryHandleAsync(ctx, new NotSupportedException("ns"), CancellationToken.None);
        result2.Should().BeTrue();
    }
}
