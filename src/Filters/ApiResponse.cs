namespace NDB.Platform.Api.Filters;

/// <summary>Standard response envelope returned by all NDB Platform API endpoints.</summary>
public sealed record ApiResponse
{
    /// <summary>Whether the request succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Response payload. <c>null</c> for non-data responses.</summary>
    public object? Data { get; init; }

    /// <summary>Descriptive message — either a success message or an error description.</summary>
    public string? Message { get; init; }

    /// <summary>List of business-level error messages.</summary>
    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>Field-level validation errors from FluentValidation.</summary>
    public IDictionary<string, string[]>? ValidationErrors { get; init; }

    /// <summary>Creates a new <see cref="ApiResponse"/> with all fields.</summary>
    public ApiResponse(
        bool success,
        object? data = null,
        string? message = null,
        IReadOnlyList<string>? errors = null,
        IDictionary<string, string[]>? validationErrors = null)
    {
        Success = success;
        Data = data;
        Message = message;
        Errors = errors;
        ValidationErrors = validationErrors;
    }
}
