using NDB.Platform.Http.Resilience;

namespace NDB.Platform.Api.Authentication;

/// <summary>
/// In-memory implementation of <see cref="ITokenStorage"/>.
/// Suitable for server-side services (background jobs, service-to-service calls).
/// For web applications with per-request context, a cookie or session-backed implementation is more appropriate.
/// Thread-safe via an internal lock for concurrent access.
/// </summary>
public sealed class InMemoryTokenStorage : ITokenStorage
{
    private readonly object _lock = new();
    private string? _accessToken;
    private string? _refreshToken;

    /// <inheritdoc />
    public string? GetAccessToken()
    {
        lock (_lock)
            return _accessToken;
    }

    /// <inheritdoc />
    public string? GetRefreshToken()
    {
        lock (_lock)
            return _refreshToken;
    }

    /// <inheritdoc />
    public void SetTokens(string accessToken, string refreshToken)
    {
        lock (_lock)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
        }
    }

    /// <inheritdoc />
    public void ClearTokens()
    {
        lock (_lock)
        {
            _accessToken = null;
            _refreshToken = null;
        }
    }
}
