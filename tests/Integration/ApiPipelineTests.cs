using FluentAssertions;
using System.Net;
using Xunit;

namespace NDB.Platform.Api.Tests.Integration;

public class ApiPipelineTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public ApiPipelineTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Live_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_Ready_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swagger_Json_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_Response_Should_Be_Json()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Response_Should_Include_Correlation_Id_Header()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
    }
}
