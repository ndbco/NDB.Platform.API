using FluentAssertions;
using Microsoft.Extensions.Options;
using NDB.Platform.Api.Authentication;
using System.Security.Claims;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateSut(Action<NdbJwtOptions>? configure = null)
    {
        var opts = new NdbJwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningKey = "this-is-a-test-key-32-chars-long!",
            ClockSkew = TimeSpan.Zero,
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RequireHttpsMetadata = false
        };
        configure?.Invoke(opts);
        return new JwtTokenService(Options.Create(opts));
    }

    [Fact]
    public void IssueAccessToken_Should_Return_Non_Empty_String()
    {
        var sut = CreateSut();
        var token = sut.IssueAccessToken([new Claim(ClaimTypes.Name, "user1")]);
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void IssueAccessToken_Should_Contain_Three_Jwt_Segments()
    {
        var sut = CreateSut();
        var token = sut.IssueAccessToken([new Claim(ClaimTypes.Name, "user1")]);
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void IssueRefreshToken_Should_Return_Base64_Of_64_Bytes()
    {
        var sut = CreateSut();
        var token = sut.IssueRefreshToken();
        token.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(token).Should().HaveCount(64);
    }

    [Fact]
    public void IssueRefreshToken_Should_Return_Different_Values_Each_Call()
    {
        var sut = CreateSut();
        var token1 = sut.IssueRefreshToken();
        var token2 = sut.IssueRefreshToken();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_Should_Return_Principal_For_Valid_Token()
    {
        var sut = CreateSut();
        var claims = new[] { new Claim(ClaimTypes.Name, "alice") };
        var token = sut.IssueAccessToken(claims);
        var principal = sut.ValidateToken(token);
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.Name)?.Value.Should().Be("alice");
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Garbage_Token()
    {
        var sut = CreateSut();
        sut.ValidateToken("garbage.not.jwt").Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Tampered_Token()
    {
        // Validate returns null for a tampered / garbage token (simulates expiry scenario)
        var sut = CreateSut();
        var token = sut.IssueAccessToken([new Claim(ClaimTypes.Name, "user")]);
        // Tamper the payload segment to simulate corruption
        var parts = token.Split('.');
        parts[1] = parts[1] + "tampered";
        sut.ValidateToken(string.Join('.', parts)).Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Wrong_Issuer()
    {
        var issuer1 = CreateSut(o => o.Issuer = "issuer-A");
        var issuer2 = CreateSut(o => o.Issuer = "issuer-B");
        var token = issuer1.IssueAccessToken([new Claim(ClaimTypes.Name, "user")]);
        issuer2.ValidateToken(token).Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Wrong_Audience()
    {
        var aud1 = CreateSut(o => o.Audience = "audience-A");
        var aud2 = CreateSut(o => o.Audience = "audience-B");
        var token = aud1.IssueAccessToken([new Claim(ClaimTypes.Name, "user")]);
        aud2.ValidateToken(token).Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Wrong_Key()
    {
        var sut1 = CreateSut(o => o.SigningKey = "this-is-a-test-key-32-chars-long!");
        var sut2 = CreateSut(o => o.SigningKey = "another-key-with-32-chars-length!");
        var token = sut1.IssueAccessToken([new Claim(ClaimTypes.Name, "user")]);
        sut2.ValidateToken(token).Should().BeNull();
    }

    [Fact]
    public void IssueAccessToken_Should_Include_Custom_Claims()
    {
        var sut = CreateSut();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "bob"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("custom", "value123")
        };
        var token = sut.IssueAccessToken(claims);
        var principal = sut.ValidateToken(token);
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
        principal.FindFirst("custom")?.Value.Should().Be("value123");
    }
}
