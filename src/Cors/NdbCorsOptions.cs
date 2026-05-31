namespace NDB.Platform.Api.Cors;

/// <summary>
/// CORS policy options for NDB Platform.
/// AllowAnyHeader and AllowAnyMethod default to <c>true</c> for developer convenience.
/// AllowCredentials defaults to <c>false</c> and must be set explicitly alongside specific <see cref="AllowedOrigins"/>.
/// </summary>
public sealed class NdbCorsOptions
{
    /// <summary>CORS policy name. Default: <c>NdbDefaultCors</c>.</summary>
    public string PolicyName { get; set; } = "NdbDefaultCors";

    /// <summary>
    /// Allowed origins. Empty array means <c>AllowAnyOrigin</c>.
    /// When <see cref="AllowCredentials"/> is <c>true</c>, this list must not be empty — <c>AllowAnyOrigin</c> is forbidden by the CORS spec.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>Allow any request header. Default: <c>true</c>.</summary>
    public bool AllowAnyHeader { get; set; } = true;

    /// <summary>Allow any HTTP method. Default: <c>true</c>.</summary>
    public bool AllowAnyMethod { get; set; } = true;

    /// <summary>
    /// Allow credentials (cookies, <c>Authorization</c> header).
    /// Default: <c>false</c>. Must be set explicitly if required.
    /// Cannot be combined with an empty <see cref="AllowedOrigins"/> list (AllowAnyOrigin is forbidden with credentials).
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// Validates the CORS configuration. Called by <see cref="CorsExtensions.AddNdbCors"/> before registering the policy.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="AllowCredentials"/> is <c>true</c> and <see cref="AllowedOrigins"/> is empty
    /// (AllowAnyOrigin + credentials is forbidden by the CORS specification and rejected by browsers).
    /// </exception>
    public void Validate()
    {
        if (AllowCredentials && AllowedOrigins.Length == 0)
            throw new InvalidOperationException(
                "NdbCorsOptions: AllowCredentials cannot be combined with an empty AllowedOrigins list " +
                "(AllowAnyOrigin). This is forbidden by the CORS specification and will be rejected by browsers. " +
                "Either specify explicit origins in AllowedOrigins, or set AllowCredentials = false.");
    }
}
