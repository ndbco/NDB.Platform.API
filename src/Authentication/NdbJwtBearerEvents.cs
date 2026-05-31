using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace NDB.Platform.Api.Authentication;

/// <summary>JWT bearer events — appends a <c>Token-Expired: true</c> response header when a token has expired.</summary>
public sealed class NdbJwtBearerEvents : JwtBearerEvents
{
    /// <inheritdoc />
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Exception is SecurityTokenExpiredException)
        {
            context.Response.Headers["Token-Expired"] = "true";
        }

        return Task.CompletedTask;
    }
}
