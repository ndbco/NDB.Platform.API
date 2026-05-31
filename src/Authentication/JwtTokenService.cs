using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NDB.Platform.Api.Authentication;

/// <summary>JWT token service using a symmetric HMAC-SHA256 signing key.</summary>
public sealed class JwtTokenService : ITokenIssuer
{
    private readonly NdbJwtOptions _options;
    private readonly SymmetricSecurityKey _key;

    /// <summary>Initializes a new instance of <see cref="JwtTokenService"/> with the given JWT options.</summary>
    public JwtTokenService(IOptions<NdbJwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    }

    /// <inheritdoc />
    public string IssueAccessToken(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(_options.AccessTokenLifetime),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    /// <inheritdoc />
    public string IssueRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = _options.ClockSkew
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}
