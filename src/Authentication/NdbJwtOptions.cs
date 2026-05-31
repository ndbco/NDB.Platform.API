namespace NDB.Platform.Api.Authentication;

/// <summary>JWT configuration options for NDB Platform.</summary>
public sealed class NdbJwtOptions
{
    /// <summary>JWT Issuer. Required.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>JWT Audience. Required.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key. Minimum 32 characters. Required.
    /// Validated at startup via <see cref="NdbJwtOptionsValidator"/> (fail-fast).
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Clock skew tolerance. Default: 0 (strict).</summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>Access token lifetime. Default: 15 minutes.</summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Refresh token lifetime. Default: 7 days.</summary>
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Require HTTPS metadata. Default: <c>true</c>. Set to <c>false</c> for local development.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    // ── FIX 4 (C-03): Configurable refresh endpoint ──

    /// <summary>
    /// Named <see cref="HttpClient"/> used by <see cref="DefaultTokenRefresher"/>.
    /// Default: <c>"NdbTokenRefresher"</c>. Override if the consuming project registers its own named client.
    /// </summary>
    public string RefreshClientName { get; set; } = "NdbTokenRefresher";

    /// <summary>
    /// Relative or absolute endpoint path for the token refresh request. Default: <c>/api/v1/auth/refresh</c>.
    /// If <c>RefreshEndpoint</c> is an absolute URI, <c>RefreshBaseAddress</c> is ignored.
    /// </summary>
    public string RefreshEndpoint { get; set; } = "/api/v1/auth/refresh";

    /// <summary>
    /// Base address of the auth server for token refresh. Required when <c>RefreshEndpoint</c> is a relative path.
    /// Example: <c>https://auth.myapp.com</c>
    /// </summary>
    public string? RefreshBaseAddress { get; set; }
}
