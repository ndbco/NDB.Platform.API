using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NDB.Platform.Api.AccessToken;
using NSubstitute;
using Xunit;

namespace NDB.Platform.Api.Tests.AccessToken;

public class HttpContextAccessTokenProviderTests
{
    private static HttpContextAccessTokenProvider CreateSut(HttpContext? ctx)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(ctx);
        return new HttpContextAccessTokenProvider(accessor);
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Return_Token_For_Bearer_Header()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Bearer my-jwt-token";
        var sut = CreateSut(ctx);
        (await sut.GetAccessTokenAsync()).Should().Be("my-jwt-token");
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Return_Null_When_No_Authorization_Header()
    {
        var sut = CreateSut(new DefaultHttpContext());
        (await sut.GetAccessTokenAsync()).Should().BeNull();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Return_Null_For_Basic_Auth()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic dXNlcjpwYXNz";
        var sut = CreateSut(ctx);
        (await sut.GetAccessTokenAsync()).Should().BeNull();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Return_Null_When_HttpContext_Is_Null()
    {
        var sut = CreateSut(null);
        (await sut.GetAccessTokenAsync()).Should().BeNull();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Be_Case_Insensitive_Bearer_Prefix()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "bearer token-value";
        var sut = CreateSut(ctx);
        (await sut.GetAccessTokenAsync()).Should().Be("token-value");
    }

    [Fact]
    public async Task GetAccessTokenAsync_Should_Return_Null_For_Empty_Authorization_Header()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "";
        var sut = CreateSut(ctx);
        (await sut.GetAccessTokenAsync()).Should().BeNull();
    }
}
