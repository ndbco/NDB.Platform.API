using System.Security.Claims;

namespace NDB.Platform.Api.Authentication;

/// <summary>Contract for issuing and validating JWT tokens.</summary>
public interface ITokenIssuer
{
    /// <summary>Issues a signed access token for the given claims.</summary>
    string IssueAccessToken(IEnumerable<Claim> claims);

    /// <summary>Issues a refresh token — an opaque, cryptographically random Base64 string.</summary>
    string IssueRefreshToken();

    /// <summary>Validates the given token. Returns the <see cref="ClaimsPrincipal"/> if valid, or <c>null</c> if the token is invalid or expired.</summary>
    ClaimsPrincipal? ValidateToken(string token);
}
