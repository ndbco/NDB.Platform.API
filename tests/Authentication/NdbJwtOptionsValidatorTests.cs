using FluentAssertions;
using Microsoft.Extensions.Options;
using NDB.Platform.Api.Authentication;
using Xunit;

namespace NDB.Platform.Api.Tests.Authentication;

// ── FIX 6 (C-08): NdbJwtOptionsValidator tests ──

public sealed class NdbJwtOptionsValidatorTests
{
    private static readonly NdbJwtOptionsValidator Validator = new();

    [Fact]
    public void Validate_ValidOptions_Should_ReturnSuccess()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "this-is-a-valid-key-minimum-32chars!",
            Issuer = "ndb-issuer",
            Audience = "ndb-audience"
        };

        var result = Validator.Validate(null, opts);

        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_EmptySigningKey_Should_Fail()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "",
            Issuer = "ndb",
            Audience = "ndb"
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SigningKey");
    }

    [Fact]
    public void Validate_SigningKeyTooShort_Should_Fail()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "short",
            Issuer = "ndb",
            Audience = "ndb"
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("32");
    }

    [Fact]
    public void Validate_EmptyIssuer_Should_Fail()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "this-is-a-valid-key-minimum-32chars!",
            Issuer = "",
            Audience = "ndb"
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Issuer");
    }

    [Fact]
    public void Validate_EmptyAudience_Should_Fail()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "this-is-a-valid-key-minimum-32chars!",
            Issuer = "ndb",
            Audience = ""
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Audience");
    }

    [Fact]
    public void Validate_SigningKeyExactly32Chars_Should_Succeed()
    {
        // Boundary: 32 chars = minimum valid
        var opts = new NdbJwtOptions
        {
            SigningKey = new string('x', 32),
            Issuer = "ndb",
            Audience = "ndb"
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeFalse();
    }

    [Fact]
    public void Validate_SigningKey31Chars_Should_Fail()
    {
        // Boundary: 31 chars = below minimum
        var opts = new NdbJwtOptions
        {
            SigningKey = new string('x', 31),
            Issuer = "ndb",
            Audience = "ndb"
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultipleFailures_Should_Combine_Messages()
    {
        var opts = new NdbJwtOptions
        {
            SigningKey = "",
            Issuer = "",
            Audience = ""
        };

        var result = Validator.Validate(null, opts);

        result.Failed.Should().BeTrue();
        // Multiple failures joined
        result.Failures!.Should().HaveCountGreaterThan(1);
    }
}
