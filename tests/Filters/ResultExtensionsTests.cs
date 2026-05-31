using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDB.Platform.Abstraction;
using NDB.Platform.Api.Filters;
using Xunit;

namespace NDB.Platform.Api.Tests.Filters;

public class ResultExtensionsTests
{
    [Fact]
    public void ToActionResult_Result_Success_Should_Return_200()
    {
        Result.Success().ToActionResult().Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ToActionResult_Result_NotFound_Should_Return_404()
    {
        Result.NotFound().ToActionResult().Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ToActionResult_Result_BadRequest_Should_Return_400()
    {
        Result.BadRequest("bad").ToActionResult().Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ToActionResult_Result_Conflict_Should_Return_409()
    {
        Result.Conflict().ToActionResult().Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void ToActionResult_Result_Unauthorized_Should_Return_401()
    {
        Result.Unauthorized().ToActionResult().Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void ToActionResult_Result_Forbidden_Should_Return_403()
    {
        var action = Result.Forbidden().ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void ToActionResult_Result_Error_Should_Return_500()
    {
        var action = Result.Error("server error").ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ToActionResult_Generic_Success_Should_Include_Data()
    {
        var result = Result<string>.Success("payload");
        var action = result.ToActionResult();
        var ok = action.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeTrue();

        // Verify Data through OkObjectResult.Value cast
        var apiResp = (ApiResponse)ok.Value!;
        apiResp.Data.Should().NotBeNull();
        apiResp.Data!.ToString().Should().Be("payload");
    }

    [Fact]
    public void ToActionResult_Generic_NotFound_Should_Return_404()
    {
        Result.NotFound<string>().ToActionResult()
            .Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ToActionResult_Generic_BadRequest_Should_Return_400()
    {
        Result<string>.BadRequest("bad").ToActionResult()
            .Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ToActionResult_Generic_Unauthorized_Should_Return_401()
    {
        Result<int>.Unauthorized().ToActionResult()
            .Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void ToActionResult_Generic_Forbidden_Should_Return_403()
    {
        var action = Result<int>.Forbidden().ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void ToActionResult_Generic_Error_Should_Return_500()
    {
        var action = Result<int>.Error("err").ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void ToActionResult_Generic_Conflict_Should_Return_409()
    {
        Result<string>.Conflict().ToActionResult()
            .Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void ToActionResult_Generic_Validation_Should_Include_ValidationErrors()
    {
        var errors = new Dictionary<string, string[]> { ["name"] = ["Name is required"] };
        var result = Result<string>.Validation(errors);
        var action = result.ToActionResult();
        var bad = action.Should().BeOfType<BadRequestObjectResult>().Subject;
        var resp = bad.Value.Should().BeOfType<ApiResponse>().Subject;
        resp.ValidationErrors.Should().ContainKey("name");
    }

    // ─── FIX C-09: PagedResult<T> ToActionResult tests ───────────────────

    [Fact]
    public void PagedResult_Success_Should_Return_Ok_With_Items_And_PageInfo()
    {
        var items = new[] { "item1", "item2", "item3" };
        var result = PagedResult<string>.Success(items, page: 1, pageSize: 10, totalItems: 3);

        var action = result.ToActionResult();
        var ok = action.Should().BeOfType<OkObjectResult>().Subject;
        var resp = ok.Value.Should().BeOfType<ApiResponse>().Subject;

        resp.Success.Should().BeTrue();
        resp.Data.Should().NotBeNull();
    }

    [Fact]
    public void PagedResult_Success_Empty_Should_Return_Ok_With_Zero_Items()
    {
        var result = PagedResult<string>.Success([], page: 1, pageSize: 10, totalItems: 0);

        var action = result.ToActionResult();
        action.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_NotFound_Should_Return_404()
    {
        var result = PagedResult<string>.NotFound("Not found");

        var action = result.ToActionResult();
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void PagedResult_Forbidden_Should_Return_403()
    {
        var result = PagedResult<string>.Forbidden();

        var action = result.ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void PagedResult_Unauthorized_Should_Return_401()
    {
        var result = PagedResult<string>.Unauthorized();

        var action = result.ToActionResult();
        action.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ─── FIX C-09: CollectionResult<T> ToActionResult tests ─────────────

    [Fact]
    public void CollectionResult_ListResult_Success_Should_Return_Ok_With_Items()
    {
        var items = new[] { 1, 2, 3 };
        var result = ListResult<int>.Success(items);

        var action = result.ToActionResult();
        var ok = action.Should().BeOfType<OkObjectResult>().Subject;
        var resp = ok.Value.Should().BeOfType<ApiResponse>().Subject;

        resp.Success.Should().BeTrue();
        resp.Data.Should().NotBeNull();
    }

    [Fact]
    public void CollectionResult_ListResult_NotFound_Should_Return_404()
    {
        var result = ListResult<int>.NotFound();

        var action = result.ToActionResult();
        action.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void CollectionResult_ListResult_Error_Should_Return_500()
    {
        var result = ListResult<int>.Error("error");

        var action = result.ToActionResult();
        action.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
