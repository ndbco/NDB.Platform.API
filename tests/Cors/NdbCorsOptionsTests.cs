using FluentAssertions;
using NDB.Platform.Api.Cors;
using Xunit;

namespace NDB.Platform.Api.Tests.Cors;

// ── FIX 9 (C-14): NdbCorsOptions validation tests ──

public sealed class NdbCorsOptionsTests
{
    [Fact]
    public void Default_AllowCredentials_Should_Be_False()
    {
        // BREAKING change from alpha.2: default was true, now false
        var opts = new NdbCorsOptions();
        opts.AllowCredentials.Should().BeFalse();
    }

    [Fact]
    public void Default_AllowAnyHeader_Should_Be_True()
    {
        var opts = new NdbCorsOptions();
        opts.AllowAnyHeader.Should().BeTrue();
    }

    [Fact]
    public void Default_AllowAnyMethod_Should_Be_True()
    {
        var opts = new NdbCorsOptions();
        opts.AllowAnyMethod.Should().BeTrue();
    }

    [Fact]
    public void Default_AllowedOrigins_Should_Be_Empty()
    {
        var opts = new NdbCorsOptions();
        opts.AllowedOrigins.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AllowCredentials_With_SpecificOrigins_Should_Not_Throw()
    {
        var opts = new NdbCorsOptions
        {
            AllowCredentials = true,
            AllowedOrigins = ["https://app.example.com"]
        };

        var act = () => opts.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_AllowCredentials_With_EmptyOrigins_Should_Throw()
    {
        // AllowAnyOrigin + AllowCredentials = forbidden by CORS spec
        var opts = new NdbCorsOptions
        {
            AllowCredentials = true,
            AllowedOrigins = []
        };

        var act = () => opts.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AllowCredentials*AllowedOrigins*");
    }

    [Fact]
    public void Validate_NoCredentials_EmptyOrigins_Should_Not_Throw()
    {
        // AllowAnyOrigin without credentials is valid (development scenario)
        var opts = new NdbCorsOptions
        {
            AllowCredentials = false,
            AllowedOrigins = []
        };

        var act = () => opts.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NoCredentials_WithOrigins_Should_Not_Throw()
    {
        var opts = new NdbCorsOptions
        {
            AllowCredentials = false,
            AllowedOrigins = ["https://app.example.com"]
        };

        var act = () => opts.Validate();

        act.Should().NotThrow();
    }
}
