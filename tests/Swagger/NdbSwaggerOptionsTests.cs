using FluentAssertions;
using NDB.Platform.Api.Swagger;
using Xunit;

namespace NDB.Platform.Api.Tests.Swagger;

public class NdbSwaggerOptionsTests
{
    [Fact]
    public void Default_Options_Should_Have_Correct_Title()
    {
        var opts = new NdbSwaggerOptions();
        opts.Title.Should().Be("NDB API");
    }

    [Fact]
    public void Default_Options_Should_Have_V1_Version()
    {
        var opts = new NdbSwaggerOptions();
        opts.Version.Should().Be("v1");
    }

    [Fact]
    public void Default_Options_Should_Have_Jwt_Enabled()
    {
        var opts = new NdbSwaggerOptions();
        opts.JwtAuthEnabled.Should().BeTrue();
    }

    [Fact]
    public void Default_Options_Should_Have_Swagger_RoutePrefix()
    {
        var opts = new NdbSwaggerOptions();
        opts.RoutePrefix.Should().Be("swagger");
    }

    [Fact]
    public void Default_Options_Should_Have_Null_Description()
    {
        var opts = new NdbSwaggerOptions();
        opts.Description.Should().BeNull();
    }

    [Fact]
    public void Should_Allow_Custom_Title()
    {
        var opts = new NdbSwaggerOptions { Title = "My Custom API" };
        opts.Title.Should().Be("My Custom API");
    }

    [Fact]
    public void Should_Allow_Disable_Jwt()
    {
        var opts = new NdbSwaggerOptions { JwtAuthEnabled = false };
        opts.JwtAuthEnabled.Should().BeFalse();
    }
}
