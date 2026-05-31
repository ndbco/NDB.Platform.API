using Microsoft.AspNetCore.Http;
using NDB.Platform.Http;

namespace NDB.Platform.Api.AccessToken;

/// <summary>Extracts the Bearer token from the HTTP <c>Authorization</c> header.</summary>
public sealed class HttpContextAccessTokenProvider : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>Initializes a new instance of <see cref="HttpContextAccessTokenProvider"/>.</summary>
    public HttpContextAccessTokenProvider(IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        _accessor = accessor;
    }

    /// <inheritdoc />
    public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var ctx = _accessor.HttpContext;
        if (ctx is null)
        {
            return ValueTask.FromResult<string?>(null);
        }

        if (ctx.Request.Headers.TryGetValue("Authorization", out var header))
        {
            var raw = header.ToString();
            if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult<string?>(raw[7..]);
            }
        }

        return ValueTask.FromResult<string?>(null);
    }
}
