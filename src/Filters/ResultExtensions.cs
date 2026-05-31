using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDB.Platform.Abstraction;

namespace NDB.Platform.Api.Filters;

/// <summary>Extension methods for converting Result types to <see cref="IActionResult"/>.</summary>
public static class ResultExtensions
{
    /// <summary>Converts a non-generic <see cref="Result"/> to the appropriate <see cref="IActionResult"/> based on its status.</summary>
    public static IActionResult ToActionResult(this Result result) =>
        result.Status switch
        {
            ResultStatus.Success => new OkObjectResult(new ApiResponse(true, null, result.Message)),
            ResultStatus.NotFound => new NotFoundObjectResult(new ApiResponse(false, null, result.Message, result.Errors)),
            ResultStatus.BadRequest => new BadRequestObjectResult(new ApiResponse(false, null, result.Message, result.Errors)),
            ResultStatus.Unauthorized => new UnauthorizedObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.Forbidden => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status403Forbidden },
            ResultStatus.Conflict => new ConflictObjectResult(new ApiResponse(false, null, result.Message, result.Errors)),
            ResultStatus.Error => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => new ObjectResult(new ApiResponse(false, null, "Unknown status")) { StatusCode = StatusCodes.Status500InternalServerError }
        };

    /// <summary>Converts a <see cref="Result{T}"/> to the appropriate <see cref="IActionResult"/> based on its status.</summary>
    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.Status switch
        {
            ResultStatus.Success => new OkObjectResult(new ApiResponse(true, result.Data, result.Message)),
            ResultStatus.NotFound => new NotFoundObjectResult(new ApiResponse(false, null, result.Message, result.Errors)),
            ResultStatus.BadRequest => new BadRequestObjectResult(new ApiResponse(false, null, result.Message, result.Errors, result.ValidationErrors?.ToDictionary(k => k.Key, v => v.Value))),
            ResultStatus.Unauthorized => new UnauthorizedObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.Forbidden => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status403Forbidden },
            ResultStatus.Conflict => new ConflictObjectResult(new ApiResponse(false, null, result.Message, result.Errors)),
            ResultStatus.Error => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => new ObjectResult(new ApiResponse(false, null, "Unknown status")) { StatusCode = StatusCodes.Status500InternalServerError }
        };

    /// <summary>
    /// Converts a <see cref="CollectionResult{T}"/> to the appropriate <see cref="IActionResult"/>.
    /// On success, the response shape is: <c>{ success, data: { items: [...], totalCount }, message }</c>.
    /// </summary>
    public static IActionResult ToActionResult<T>(this CollectionResult<T> result)
    {
        if (result.Status == ResultStatus.Success)
        {
            return new OkObjectResult(new ApiResponse(
                success: true,
                data: new { items = result.Items, totalCount = result.Items.Count },
                message: result.Message
            ));
        }

        return MapCollectionNonOkStatus(result);
    }

    /// <summary>
    /// Converts a <see cref="PagedResult{T}"/> to the appropriate <see cref="IActionResult"/>.
    /// On success, the response shape is: <c>{ success, data: { items: [...], pageInfo: { page, pageSize, totalItems, totalPages } }, message }</c>.
    /// </summary>
    public static IActionResult ToActionResult<T>(this PagedResult<T> result)
    {
        if (result.Status == ResultStatus.Success)
        {
            return new OkObjectResult(new ApiResponse(
                success: true,
                data: new { items = result.Items, pageInfo = result.PageInfo },
                message: result.Message
            ));
        }

        return MapCollectionNonOkStatus(result);
    }

    /// <summary>Maps non-success statuses from a <see cref="CollectionResult{T}"/> to the appropriate error response.</summary>
    private static ObjectResult MapCollectionNonOkStatus<T>(CollectionResult<T> result) =>
        result.Status switch
        {
            ResultStatus.NotFound => new NotFoundObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.BadRequest => new BadRequestObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.Unauthorized => new UnauthorizedObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.Forbidden => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status403Forbidden },
            ResultStatus.Conflict => new ConflictObjectResult(new ApiResponse(false, null, result.Message)),
            ResultStatus.Error => new ObjectResult(new ApiResponse(false, null, result.Message)) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => new ObjectResult(new ApiResponse(false, null, "Unknown status")) { StatusCode = StatusCodes.Status500InternalServerError }
        };
}
