using FluentAssertions;
using NDB.Platform.Api.Filters;
using Xunit;

namespace NDB.Platform.Api.Tests.Filters;

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_Success_Should_Have_Correct_Properties()
    {
        var response = new ApiResponse(true, "data", "OK");
        response.Success.Should().BeTrue();
        response.Data.Should().Be("data");
        response.Message.Should().Be("OK");
        response.Errors.Should().BeNull();
        response.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void ApiResponse_Failure_Should_Have_Correct_Properties()
    {
        var errors = new[] { "error1", "error2" };
        var response = new ApiResponse(false, null, "fail", errors);
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ApiResponse_With_ValidationErrors_Should_Expose_Them()
    {
        var valErrors = new Dictionary<string, string[]> { ["field"] = ["required"] };
        var response = new ApiResponse(false, null, null, null, valErrors);
        response.ValidationErrors.Should().ContainKey("field");
        response.ValidationErrors!["field"].Should().ContainSingle().Which.Should().Be("required");
    }

    [Fact]
    public void ApiResponse_Default_Constructor_Should_Set_No_Errors()
    {
        var response = new ApiResponse(true);
        response.Errors.Should().BeNull();
        response.ValidationErrors.Should().BeNull();
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void ApiResponse_Record_Equality_Should_Work()
    {
        var r1 = new ApiResponse(true, null, "msg");
        var r2 = new ApiResponse(true, null, "msg");
        r1.Should().Be(r2);
    }
}
