using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NDB.Platform.Api.Authorization;
using Xunit;

namespace NDB.Platform.Api.Tests.Authorization;

public sealed class PermissionPolicyProviderTests
{
    private static PermissionPolicyProvider BuildProvider()
    {
        var opts = Options.Create(new AuthorizationOptions());
        return new PermissionPolicyProvider(opts);
    }

    [Fact]
    public async Task GetPolicyAsync_PermissionPrefix_ShouldReturnPolicy()
    {
        var provider = BuildProvider();
        var policy = await provider.GetPolicyAsync("perm:users.create");

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<PermissionRequirement>()
            .Should().ContainSingle(r => r.PermissionKey == "users.create");
    }

    [Fact]
    public async Task GetPolicyAsync_NoPrefix_ShouldReturnNull()
    {
        var provider = BuildProvider();
        var policy = await provider.GetPolicyAsync("some-other-policy");
        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_PrefixCaseInsensitive_ShouldReturnPolicy()
    {
        var provider = BuildProvider();
        var policy = await provider.GetPolicyAsync("PERM:reports.execute");
        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ShouldReturnPolicy()
    {
        var provider = BuildProvider();
        var policy = await provider.GetDefaultPolicyAsync();
        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_ShouldReturnNullByDefault()
    {
        var provider = BuildProvider();
        var policy = await provider.GetFallbackPolicyAsync();
        // Default fallback is null unless AuthorizationOptions configured it
        policy.Should().BeNull();
    }
}
