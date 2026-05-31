using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NDB.Platform.Api.Authentication;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

public class NdbJwtBearerEventsTests
{
    private static AuthenticationFailedContext CreateContext(Exception ex)
    {
        var httpContext = new DefaultHttpContext();
        var scheme = new Microsoft.AspNetCore.Authentication.AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme, null,
            typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler));
        var context = new AuthenticationFailedContext(httpContext, scheme, new JwtBearerOptions())
        {
            Exception = ex
        };
        return context;
    }

    [Fact]
    public async Task AuthenticationFailed_Should_Add_Token_Expired_Header_For_Expired_Exception()
    {
        var events = new NdbJwtBearerEvents();
        var ctx = CreateContext(new SecurityTokenExpiredException("expired"));
        await events.AuthenticationFailed(ctx);
        ctx.HttpContext.Response.Headers["Token-Expired"].ToString().Should().Be("true");
    }

    [Fact]
    public async Task AuthenticationFailed_Should_Not_Add_Header_For_Generic_Exception()
    {
        var events = new NdbJwtBearerEvents();
        var ctx = CreateContext(new InvalidOperationException("other error"));
        await events.AuthenticationFailed(ctx);
        ctx.HttpContext.Response.Headers.ContainsKey("Token-Expired").Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticationFailed_Should_Not_Add_Header_For_Invalid_Signature()
    {
        var events = new NdbJwtBearerEvents();
        var ctx = CreateContext(new SecurityTokenInvalidSignatureException("bad signature"));
        await events.AuthenticationFailed(ctx);
        ctx.HttpContext.Response.Headers.ContainsKey("Token-Expired").Should().BeFalse();
    }
}
