using Hangfire.Dashboard;
using System.Security.Cryptography;
using System.Text;

namespace NDB.Platform.Api.Hangfire;

/// <summary>
/// Basic authentication filter for the Hangfire dashboard.
/// Uses <see cref="CryptographicOperations.FixedTimeEquals"/> for timing-safe credential comparison.
/// </summary>
public sealed class NdbHangfireBasicAuthFilter : IDashboardAuthorizationFilter
{
    private readonly byte[] _userBytes;
    private readonly byte[] _passwordBytes;

    /// <summary>Initializes the filter with the expected username and password.</summary>
    public NdbHangfireBasicAuthFilter(string user, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(user);
        ArgumentNullException.ThrowIfNull(password);
        _userBytes = Encoding.UTF8.GetBytes(user);
        _passwordBytes = Encoding.UTF8.GetBytes(password);
    }

    /// <inheritdoc />
    public bool Authorize(DashboardContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            SetChallenge(httpContext);
            return false;
        }

        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var colonIdx = decoded.IndexOf(':', StringComparison.Ordinal);
            if (colonIdx > 0)
            {
                var user = decoded[..colonIdx];
                var pass = decoded[(colonIdx + 1)..];

                // Timing-safe comparison — prevents timing attacks on the credential check
                var userBytes = Encoding.UTF8.GetBytes(user);
                var passBytes = Encoding.UTF8.GetBytes(pass);

                var userMatch = CryptographicOperations.FixedTimeEquals(userBytes, _userBytes);
                var passMatch = CryptographicOperations.FixedTimeEquals(passBytes, _passwordBytes);

                if (userMatch && passMatch)
                    return true;
            }
        }
        catch (FormatException)
        {
            // Invalid base64 — fall through to challenge
        }

        SetChallenge(httpContext);
        return false;
    }

    private static void SetChallenge(Microsoft.AspNetCore.Http.HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
    }
}
