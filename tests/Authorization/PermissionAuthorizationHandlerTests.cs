using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NDB.Platform.Abstraction.Security;
using NDB.Platform.Api.Authorization;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace NDB.Platform.Api.Tests.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    private static PermissionAuthorizationHandler BuildHandler(
        IPermissionResolver resolver,
        Action<NdbPermissionOptions>? configure = null)
    {
        var opts = new NdbPermissionOptions();
        configure?.Invoke(opts);
        return new PermissionAuthorizationHandler(resolver, Options.Create(opts));
    }

    private static AuthorizationHandlerContext BuildContext(
        ClaimsPrincipal user,
        PermissionRequirement requirement)
    {
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    private static ClaimsPrincipal AuthenticatedUser(params Claim[] extra)
    {
        var claims = new List<Claim>(extra);
        if (!extra.Any(c => c.Type == ClaimTypes.NameIdentifier))
            claims.Add(new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        if (!extra.Any(c => c.Type == ClaimTypes.Name))
            claims.Add(new(ClaimTypes.Name, "testuser"));
        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task HandleAsync_SuperadminClaim_ShouldSucceedWithoutResolver()
    {
        var resolver = Substitute.For<IPermissionResolver>();
        var handler = BuildHandler(resolver);
        var user = AuthenticatedUser(new Claim("is_superadmin", "true"));
        var ctx = BuildContext(user, new PermissionRequirement("users.create"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
        await resolver.DidNotReceive().HasPermissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_BypassRole_ShouldSucceedWithoutResolver()
    {
        var resolver = Substitute.For<IPermissionResolver>();
        var handler = BuildHandler(resolver, opt => opt.BypassRoles = ["SUPER_ADMIN"]);
        var user = AuthenticatedUser(new Claim(ClaimTypes.Role, "SUPER_ADMIN"));
        var ctx = BuildContext(user, new PermissionRequirement("users.delete"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
        await resolver.DidNotReceive().HasPermissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_HasPermission_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var resolver = Substitute.For<IPermissionResolver>();
        resolver.HasPermissionAsync(userId, "reports.execute", Arg.Any<CancellationToken>()).Returns(true);

        var handler = BuildHandler(resolver);
        var user = AuthenticatedUser(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var ctx = BuildContext(user, new PermissionRequirement("reports.execute"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_NoPermission_ShouldNotSucceed()
    {
        var userId = Guid.NewGuid();
        var resolver = Substitute.For<IPermissionResolver>();
        resolver.HasPermissionAsync(userId, "admin.delete", Arg.Any<CancellationToken>()).Returns(false);

        var handler = BuildHandler(resolver);
        var user = AuthenticatedUser(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var ctx = BuildContext(user, new PermissionRequirement("admin.delete"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_Unauthenticated_ShouldNotSucceed()
    {
        var resolver = Substitute.For<IPermissionResolver>();
        var handler = BuildHandler(resolver);
        var unauthUser = new ClaimsPrincipal(new ClaimsIdentity()); // no auth type = not authenticated
        var ctx = BuildContext(unauthUser, new PermissionRequirement("users.view"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_InvalidUserId_ShouldNotSucceed()
    {
        var resolver = Substitute.For<IPermissionResolver>();
        var handler = BuildHandler(resolver);
        var user = AuthenticatedUser(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));
        var ctx = BuildContext(user, new PermissionRequirement("users.view"));

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
        await resolver.DidNotReceive().HasPermissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
