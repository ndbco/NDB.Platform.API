using FluentAssertions;
using Hangfire.Dashboard;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Api.Hangfire;
using System.Text;
using Xunit;

namespace NDB.Platform.Api.Tests.Hangfire;

// ── FIX 8 (C-13): NdbHangfireBasicAuthFilter timing-attack safe comparison ──

public sealed class NdbHangfireBasicAuthFilterFixedTimeEqualsTests
{
    private static AspNetCoreDashboardContext CreateDashContext(HttpContext ctx)
    {
        ctx.RequestServices ??= new ServiceCollection().BuildServiceProvider();
        return new AspNetCoreDashboardContext(new InMemoryStorage(), new global::Hangfire.DashboardOptions(), ctx);
    }

    private static string Base64Encode(string s) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

    [Fact]
    public void Authorize_SameCredentials_DifferentLength_Should_Not_Match()
    {
        // Timing attack guard: different length bytes should still return false
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        // "admin:sec" — correct user, shorter password
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("admin:sec");
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
    }

    [Fact]
    public void Authorize_CorrectUser_EmptyPassword_Should_Not_Match_NonEmptyPassword()
    {
        var filter = new NdbHangfireBasicAuthFilter("admin", "secret");
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("admin:");
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
    }

    [Fact]
    public void Authorize_UnicodePassword_Should_Match_Correctly()
    {
        // UTF-8 multi-byte password
        const string unicodePass = "p@ssw0rd-é";
        var filter = new NdbHangfireBasicAuthFilter("admin", unicodePass);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode($"admin:{unicodePass}");
        filter.Authorize(CreateDashContext(ctx)).Should().BeTrue();
    }

    [Fact]
    public void Authorize_UnicodePassword_WrongInput_Should_Return_False()
    {
        const string unicodePass = "p@ssw0rd-é";
        var filter = new NdbHangfireBasicAuthFilter("admin", unicodePass);
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = "Basic " + Base64Encode("admin:p@ssw0rd-e");
        filter.Authorize(CreateDashContext(ctx)).Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Throw_When_Password_Empty()
    {
        var opts = new NdbHangfireOptions
        {
            BasicAuthUser = "admin",
            BasicAuthPassword = ""
        };

        var act = () => opts.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*BasicAuthPassword*");
    }

    [Fact]
    public void Validate_Should_Throw_When_User_Empty()
    {
        var opts = new NdbHangfireOptions
        {
            BasicAuthUser = "   ",
            BasicAuthPassword = "secret"
        };

        var act = () => opts.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*BasicAuthUser*");
    }

    [Fact]
    public void Validate_Should_Not_Throw_When_Both_Set()
    {
        var opts = new NdbHangfireOptions
        {
            BasicAuthUser = "admin",
            BasicAuthPassword = "supersecret"
        };

        var act = () => opts.Validate();

        act.Should().NotThrow();
    }
}
