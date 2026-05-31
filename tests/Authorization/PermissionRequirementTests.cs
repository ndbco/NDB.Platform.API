using FluentAssertions;
using NDB.Platform.Api.Authorization;
using Xunit;

namespace NDB.Platform.Api.Tests.Authorization;

public sealed class PermissionRequirementTests
{
    [Fact]
    public void Constructor_ValidKey_ShouldSetPermissionKey()
    {
        var req = new PermissionRequirement("users.create");
        req.PermissionKey.Should().Be("users.create");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyOrWhitespaceKey_ShouldThrowArgumentException(string key)
    {
        var act = () => new PermissionRequirement(key);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullKey_ShouldThrowArgumentException()
    {
        var act = () => new PermissionRequirement(null!);
        act.Should().Throw<ArgumentException>();
    }
}
