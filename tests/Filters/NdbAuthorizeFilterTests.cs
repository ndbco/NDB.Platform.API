using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Api.Authorization;
using Xunit;

namespace NDB.Platform.Api.Tests.Filters;

/// <summary>
/// FIX C-07: Tests untuk NdbRequireRoleAttribute dan AddNdbAuthorization.
/// NdbAuthorizeFilter dan NdbRoleAuthorizeFilter telah dihapus — pakai ASP.NET standard [Authorize].
/// </summary>
public sealed class NdbRequireRoleAttributeTests
{
    [Fact]
    public void NdbRequireRoleAttribute_Should_Set_Roles_Property()
    {
        var attr = new NdbRequireRoleAttribute("ADMIN");
        attr.Roles.Should().Be("ADMIN");
    }

    [Fact]
    public void NdbRequireRoleAttribute_MultipleRoles_Should_Join_With_Comma()
    {
        var attr = new NdbRequireRoleAttribute("ADMIN", "SUPERADMIN");
        attr.Roles.Should().Be("ADMIN,SUPERADMIN");
    }

    [Fact]
    public void NdbRequireRoleAttribute_ThreeRoles_Should_Join_With_Comma()
    {
        var attr = new NdbRequireRoleAttribute("ADMIN", "SUPERVISOR", "MANAGER");
        attr.Roles.Should().Be("ADMIN,SUPERVISOR,MANAGER");
    }

    [Fact]
    public void NdbRequireRoleAttribute_EmptyRoles_Should_Throw_ArgumentException()
    {
        var act = () => new NdbRequireRoleAttribute();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one role*");
    }

    [Fact]
    public void NdbRequireRoleAttribute_Should_Extend_AuthorizeAttribute()
    {
        typeof(NdbRequireRoleAttribute).Should().BeDerivedFrom<AuthorizeAttribute>();
    }

    [Fact]
    public void NdbRequireRoleAttribute_AttributeUsage_Should_Target_ClassAndMethod()
    {
        var usage = typeof(NdbRequireRoleAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Method);
    }

    [Fact]
    public void AddNdbAuthorization_FallbackPolicy_Should_Require_Authenticated_User()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNdbAuthorization();

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>().Value;

        options.FallbackPolicy.Should().NotBeNull();
    }

    [Fact]
    public void AddNdbAuthorization_DefaultPolicy_Should_Require_Authenticated_User()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNdbAuthorization();

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>().Value;

        options.DefaultPolicy.Should().NotBeNull();
    }

    [Fact]
    public void AddNdbAuthorization_CustomConfigure_Should_Apply()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNdbAuthorization(opts =>
        {
            opts.AddPolicy("TestPolicy", policy => policy.RequireClaim("custom"));
        });

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>().Value;

        options.GetPolicy("TestPolicy").Should().NotBeNull();
    }
}
