using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NDB.Platform.Abstraction;
using NDB.Platform.Http.Resilience;

namespace NDB.Platform.Api.Authentication;

/// <summary>
/// Default implementation of <see cref="ITokenRefresher"/> for NDB Platform.
/// POSTs to the configurable JWT refresh endpoint defined in <see cref="NdbJwtOptions"/>.
/// Used by <c>BaseApiService</c> for automatic access token refresh.
/// </summary>
public sealed class DefaultTokenRefresher : ITokenRefresher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStorage _tokenStorage;
    private readonly NdbJwtOptions _options;

    /// <summary>Initializes a new instance of <see cref="DefaultTokenRefresher"/>.</summary>
    public DefaultTokenRefresher(
        IHttpClientFactory httpClientFactory,
        ITokenStorage tokenStorage,
        IOptions<NdbJwtOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result<TokenResponse>> RefreshAsync(CancellationToken ct = default)
    {
        var refreshToken = _tokenStorage.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
            return Result<TokenResponse>.Unauthorized("No refresh token available");

        // Use the named HttpClient and configurable endpoint from options
        using var client = _httpClientFactory.CreateClient(_options.RefreshClientName);
        var requestUri = ResolveRequestUri(_options);

        try
        {
            var response = await client.PostAsJsonAsync(
                requestUri,
                new { RefreshToken = refreshToken },
                ct);

            if (!response.IsSuccessStatusCode)
                return Result<TokenResponse>.Unauthorized("Token refresh failed");

            var payload = await response.Content.ReadFromJsonAsync<RefreshResponsePayload>(
                cancellationToken: ct);

            if (payload is null || string.IsNullOrEmpty(payload.AccessToken))
                return Result<TokenResponse>.BadRequest("Invalid refresh response");

            var tokenResponse = new TokenResponse(
                payload.AccessToken,
                payload.RefreshToken ?? refreshToken,
                DateTimeOffset.UtcNow.Add(_options.AccessTokenLifetime));

            _tokenStorage.SetTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken);
            return Result<TokenResponse>.Success(tokenResponse);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return Result<TokenResponse>.Error("Network error during token refresh");
        }
    }

    /// <summary>
    /// Resolves the final URI for the refresh request.
    /// If <c>RefreshEndpoint</c> is an absolute URI it is used as-is.
    /// If it is a relative path, <c>RefreshBaseAddress</c> is prepended.
    /// </summary>
    private static string ResolveRequestUri(NdbJwtOptions opts)
    {
        if (Uri.IsWellFormedUriString(opts.RefreshEndpoint, UriKind.Absolute))
            return opts.RefreshEndpoint;

        if (!string.IsNullOrWhiteSpace(opts.RefreshBaseAddress))
        {
            var baseAddress = opts.RefreshBaseAddress.TrimEnd('/');
            var endpoint = opts.RefreshEndpoint.TrimStart('/');
            return $"{baseAddress}/{endpoint}";
        }

        // Fallback: use the relative path as-is (works if HttpClient.BaseAddress is set by the consuming project)
        return opts.RefreshEndpoint;
    }

    // Internal DTO for deserializing the refresh response payload
    private sealed record RefreshResponsePayload(string AccessToken, string? RefreshToken);
}
