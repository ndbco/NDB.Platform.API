# NDB.Platform.Api.Authentication

JWT Bearer authentication — dual-token, HMAC-SHA256, fail-fast config validation, and token auto-refresh.

---

## Key Types

| Type | Description |
|---|---|
| `NdbJwtOptions` | Configuration: issuer, audience, signing key, lifetimes, refresh endpoint |
| `NdbJwtOptionsValidator` | Fail-fast startup validation — rejects misconfigured JWT before serving requests |
| `ITokenIssuer` | Contract for issuing and validating JWT tokens |
| `JwtTokenService` | HMAC-SHA256 implementation of `ITokenIssuer` |
| `NdbJwtBearerEvents` | Appends `Token-Expired: true` header on expired-token 401 responses |
| `HttpContextActorAccessor` | Reads actor identity (name, ID, role) from JWT claims in `HttpContext` |
| `DefaultTokenRefresher` | Posts to the refresh endpoint to exchange a refresh token for new tokens |
| `InMemoryTokenStorage` | Thread-safe in-memory `ITokenStorage` for server-side use |

---

## Setup

```csharp
builder.Services.AddNdbJwt(o =>
{
    o.Issuer               = configuration["Jwt:Issuer"]!;
    o.Audience             = configuration["Jwt:Audience"]!;
    o.SigningKey           = configuration["Jwt:Key"]!;       // min 32 characters
    o.AccessTokenLifetime  = TimeSpan.FromMinutes(15);         // default
    o.RefreshTokenLifetime = TimeSpan.FromDays(7);             // default
    o.ClockSkew            = TimeSpan.Zero;                    // strict (default)
    o.RequireHttpsMetadata = true;                             // set false for dev
});
```

`AddNdbJwt` also registers:
- `ITokenIssuer` → `JwtTokenService` (Singleton)
- `ITokenStorage` → `InMemoryTokenStorage` (Scoped)
- `ITokenRefresher` → `DefaultTokenRefresher` (Scoped)
- `IActorAccessor` → `HttpContextActorAccessor` (Scoped) — overrides Core's `SystemActorAccessor`

---

## Configurable Token Refresh

```csharp
builder.Services.AddNdbJwt(o =>
{
    // ...
    o.RefreshBaseAddress = "https://auth.myapp.com";
    o.RefreshEndpoint    = "/api/v1/auth/refresh";  // relative path, or absolute URL
    o.RefreshClientName  = "NdbTokenRefresher";     // named HttpClient (default)
});
```

`DefaultTokenRefresher` POSTs `{ refreshToken }` to the resolved endpoint and stores the new tokens in `ITokenStorage`.

---

## Fail-Fast Validation

`NdbJwtOptionsValidator` is registered via `ValidateOnStart()`. The following are rejected at startup:

- `SigningKey` is null/empty → **error**
- `SigningKey` length < 32 characters → **error** (256-bit minimum)
- `Issuer` is null/empty → **error**
- `Audience` is null/empty → **error**

This means misconfiguration surfaces immediately, not on the first authenticated request.

---

## Token-Expired Header

When a valid JWT has expired, `NdbJwtBearerEvents.AuthenticationFailed` intercepts the event and sets `Token-Expired: true` in the response headers. The client can inspect this header to decide whether to attempt a refresh (expired token) or show a login prompt (invalid token).

```
HTTP/1.1 401 Unauthorized
Token-Expired: true
```

---

## Issuing Tokens

```csharp
public class LoginHandler(ITokenIssuer tokenIssuer, AppDbContext db)
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == cmd.Email, ct);
        if (user is null || !PasswordHasher.Verify(cmd.Password, user.PasswordHash))
            return Result.Unauthorized("Invalid credentials.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Name),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("is_superadmin",           user.IsSuperAdmin.ToString().ToLower())
        };

        return Result.Success(new LoginResponse(
            AccessToken:  tokenIssuer.IssueAccessToken(claims),
            RefreshToken: tokenIssuer.IssueRefreshToken(),
            ExpiresIn:    (int)TimeSpan.FromMinutes(15).TotalSeconds));
    }
}
```

---

## HttpContextActorAccessor

Automatically registered by `AddNdbJwt()`. Reads actor identity from JWT claims:

| Claim | Property |
|---|---|
| `ClaimTypes.Name` | `AuditActor.Actor` (username) |
| `ClaimTypes.NameIdentifier` | `AuditActor.ActorId` (user ID) |
| `ClaimTypes.Role` | `AuditActor.Role` |

Falls back to `AuditActor.System` for unauthenticated requests. This feeds `IAuditContext` via `AuditActorBehavior` in `NDB.Platform.Data`.

---

## InMemoryTokenStorage

Thread-safe, suitable for background services and service-to-service clients. For web applications where each request may carry different tokens, consider a cookie-backed or per-request implementation.

```csharp
// Override the default registration:
builder.Services.AddScoped<ITokenStorage, MySessionTokenStorage>();
```
