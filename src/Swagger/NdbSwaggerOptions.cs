namespace NDB.Platform.Api.Swagger;

/// <summary>Swagger configuration options for NDB Platform.</summary>
public sealed class NdbSwaggerOptions
{
    /// <summary>API title displayed in the Swagger UI. Default: <c>NDB API</c>.</summary>
    public string Title { get; set; } = "NDB API";

    /// <summary>API version, also used as the Swagger document name. Default: <c>v1</c>.</summary>
    public string Version { get; set; } = "v1";

    /// <summary>Optional API description displayed below the title.</summary>
    public string? Description { get; set; }

    /// <summary>Whether to add a JWT Bearer security definition to Swagger. Default: <c>true</c>.</summary>
    public bool JwtAuthEnabled { get; set; } = true;

    /// <summary>Route prefix for the Swagger UI. Default: <c>swagger</c>.</summary>
    public string RoutePrefix { get; set; } = "swagger";
}
