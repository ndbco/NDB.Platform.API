using FluentAssertions;
using Hangfire.Dashboard;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Api.Hangfire;
using System.Text;
using Xunit;

namespace NDB.Platform.Api.Tests.Hangfire;

public class NdbHangfireBasicAuthFilterTests
{
    // AspNetCoreDashboardContext requires HttpContext.RequestServices (not null)
    private static AspNetCoreDashboardContext CreateDashContext(HttpContext ctx)
    {
        ctx.RequestServices ??= new ServiceCollection().BuildServiceProvider();
        return new AspNetCoreDashboardContext(new InMemoryStorage(), new global::Hangfire.DashboardOptions(), ctx);
    }

    private static string Base64Encode(string s) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

    [Fact]
    public void Authorize_Should_Return_True_For_Valid_Credentials()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("admin:secret");
        filter.Authorize(CreateDashContext(ctx)).Should().BeTrue();
    }

    [Fact]
    public void Authorize_Should_Return_False_For_Wrong_Password()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("admin:wrong");
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Authorize_Should_Return_False_When_No_Header()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        filter.Authorize(CreateDashContext(new DefaultHttpContext())).Should().BeFalse();
    }

    [Fact]
    public void Authorize_Should_Return_False_For_Invalid_Base64()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic !!!invalid!!!";
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
    }

    [Fact]
    public void Authorize_Should_Return_False_For_Wrong_Username()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("wronguser:secret");
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
    }

    [Fact]
    public void Authorize_Should_Set_WWW_Authenticate_Header_On_Reject()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        filter.Authorize(CreateDashContext(ctx));
        ctx.Response.Headers.WWWAuthenticate.ToString().Should().Contain("Basic");
    }
}
