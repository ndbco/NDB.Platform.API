using FluentAssertions;
using NDB.Platform.Api.Authentication;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

public class NdbJwtOptionsTests
{
    [Fact]
    public void Default_Options_Should_Have_Correct_Defaults()
    {
        var opts = new NdbJwtOptions();
        opts.ClockSkew.Should().Be(TimeSpan.Zero);
        opts.AccessTokenLifetime.Should().Be(TimeSpan.FromMinutes(15));
        opts.RefreshTokenLifetime.Should().Be(TimeSpan.FromDays(7));
        opts.RequireHttpsMetadata.Should().BeTrue();
        opts.Issuer.Should().BeEmpty();
        opts.Audience.Should().BeEmpty();
        opts.SigningKey.Should().BeEmpty();
    }

    [Fact]
    public void Options_Should_Allow_Custom_AccessToken_Lifetime()
    {
        var opts = new NdbJwtOptions { AccessTokenLifetime = TimeSpan.FromHours(1) };
        opts.AccessTokenLifetime.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Options_Should_Allow_Custom_RefreshToken_Lifetime()
    {
        var opts = new NdbJwtOptions { RefreshTokenLifetime = TimeSpan.FromDays(30) };
        opts.RefreshTokenLifetime.Should().Be(TimeSpan.FromDays(30));
    }

    [Fact]
    public void Options_Should_Allow_Disable_Https()
    {
        var opts = new NdbJwtOptions { RequireHttpsMetadata = false };
        opts.RequireHttpsMetadata.Should().BeFalse();
    }
}
