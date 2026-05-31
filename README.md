# NDB.Platform.Api

<div align="center">

**The web API layer library for every NDB Platform project**

Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/) — *Navigate Digital Boundaries*

[![NuGet](https://img.shields.io/nuget/v/NDB.Platform.Api?label=NuGet&color=blue)](https://www.nuget.org/packages/NDB.Platform.Api/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NDB.Platform.Api?color=green)](https://www.nuget.org/packages/NDB.Platform.Api/)
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-purple)](https://dotnet.microsoft.com/)

</div>

---

## What is NDB.Platform.Api?

**NDB.Platform.Api** is the web layer package in the NDB Platform ecosystem. It wraps the repetitive ASP.NET Core wiring that every API project needs into consistent, pre-configured building blocks.

**Depends on:** `NDB.Platform.Core` + `NDB.Platform.Data` (both installed automatically).

| Area | What you get |
|---|---|
| **Authentication** | JWT Bearer dual-token, HMAC-SHA256, fail-fast config validation, token auto-refresh |
| **Authorization** | Permission-based (`[RequirePermission]`) + role-based (`[NdbRequireRole]`) |
| **Result → HTTP** | Auto-converts handler `Result<T>` return values to `ApiResponse` JSON |
| **Middleware** | Correlation ID, request logging, global exception handler |
| **Hangfire** | Provider-agnostic background jobs, timing-safe dashboard auth |
| **Swagger** | Pre-configured Swashbuckle with JWT Bearer support |
| **Extras** | CORS, ForwardedHeaders, health checks, API versioning, PDF renderer abstraction |

---

## Installation

```bash
dotnet add package NDB.Platform.Api
```

> `NDB.Platform.Core` and `NDB.Platform.Data` are installed automatically as dependencies.

---

## Table of Contents

| Namespace | Documentation | Description |
|---|---|---|
| `NDB.Platform.Api.Authentication` | [📄 Authentication](src/Authentication/README.md) | JWT Bearer, token service, token refresh, actor accessor |
| `NDB.Platform.Api.Authorization` | [📄 Authorization](src/Authorization/README.md) | Permission-based + role-based authorization |
| `NDB.Platform.Api.Filters` | [📄 Filters](src/Filters/README.md) | `ApiResponse` envelope, `ResultActionFilter`, `ResultExtensions` |
| `NDB.Platform.Api.Middleware` | [📄 Middleware](src/Middleware/README.md) | Correlation ID, request logging, global exception handler |
| `NDB.Platform.Api.Hangfire` | [📄 Hangfire](src/Hangfire/README.md) | Background jobs, dashboard auth, provider-agnostic setup |
| `NDB.Platform.Api.Swagger` | [📄 Swagger](src/Swagger/README.md) | Swashbuckle pre-config with JWT and API versioning |
| — | [🧪 Tests](tests/README.md) | Test suite — net8.0 + net10.0, coverage ≥ 80% |

---

## Minimum Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNdbCqrs(typeof(Program).Assembly);
builder.Services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddNdbMapping(typeof(Program).Assembly);

builder.Services.AddNdbJwt(o =>
{
    o.Issuer     = builder.Configuration["Jwt:Issuer"]!;
    o.Audience   = builder.Configuration["Jwt:Audience"]!;
    o.SigningKey = builder.Configuration["Jwt:Key"]!;
});

builder.Services.AddNdbMiddleware();
builder.Services.AddNdbSwagger(o => o.Title = "My API");
builder.Services.AddNdbHealthChecks();
builder.Services.AddControllers();

var app = builder.Build();

app.UseNdbMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.UseNdbSwaggerUI();
app.MapControllers();
app.MapNdbHealthChecks();

app.Run();
```

---

## Core Concepts

### 1 · JWT Authentication

Dual-token (access + refresh), HMAC-SHA256, with fail-fast startup validation.

```csharp
builder.Services.AddNdbJwt(o =>
{
    o.Issuer               = "my-api";
    o.Audience             = "my-client";
    o.SigningKey           = "your-32-char-minimum-secret-key!!";
    o.AccessTokenLifetime  = TimeSpan.FromMinutes(15); // default
    o.RefreshTokenLifetime = TimeSpan.FromDays(7);     // default
    o.RequireHttpsMetadata = false;                    // dev only
});
```

`NdbJwtOptionsValidator` fires at startup and rejects if `SigningKey` is under 32 characters or `Issuer`/`Audience` is missing.

**Issue tokens in your login handler:**

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
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role)
        };

        return Result.Success(new LoginResponse(
            AccessToken:  tokenIssuer.IssueAccessToken(claims),
            RefreshToken: tokenIssuer.IssueRefreshToken()));
    }
}
```

When a token is expired, `NdbJwtBearerEvents` appends `Token-Expired: true` to the 401 response — so clients can distinguish expiry from invalid credentials before attempting a refresh.

**Configurable token refresh endpoint:**

```csharp
builder.Services.AddNdbJwt(o =>
{
    // ...
    o.RefreshBaseAddress = "https://auth.myapp.com";
    o.RefreshEndpoint    = "/api/v1/auth/refresh";
    o.RefreshClientName  = "NdbTokenRefresher";
});
```

---

### 2 · Authorization

#### Permission-based (granular RBAC)

```csharp
// Register:
builder.Services.AddNdbPermissionAuthorization(o =>
{
    o.SuperAdminClaim = "is_superadmin"; // JWT claim that bypasses all checks
    o.BypassRoles     = ["SUPER_ADMIN"];
});
builder.Services.AddScoped<IPermissionResolver, PermissionResolver>();

// Use on controllers or actions:
[RequirePermission("users.create")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
    => (await Mediator.Send(new CreateUserCommand(req), ct)).ToActionResult();
```

`PermissionPolicyProvider` generates `"perm:{key}"` policies on demand — no startup boilerplate per key.

#### Role-based (shorthand)

```csharp
[NdbRequireRole("ADMIN")]
public IActionResult AdminOnly() { ... }

[NdbRequireRole("ADMIN", "MANAGER")]
public IActionResult AdminOrManager() { ... }
```

#### Secure all endpoints by default

```csharp
builder.Services.AddNdbAuthorization();
// All endpoints require authentication unless decorated with [AllowAnonymous]
```

---

### 3 · Result → HTTP (ApiResponse Envelope)

`ResultActionFilter` converts handler return values to a consistent JSON envelope automatically.

```csharp
builder.Services.AddControllers(opt => opt.Filters.Add<ResultActionFilter>());
```

| Handler return | HTTP | Response body |
|---|---|---|
| `Result.Success(dto)` | 200 | `{ success: true, data: dto }` |
| `Result.NotFound("...")` | 404 | `{ success: false, message: "..." }` |
| `Result.Validation(errors)` | 400 | `{ success: false, validationErrors: {...} }` |
| `PagedResult<T>.Success(...)` | 200 | `{ success: true, data: { items, pageInfo } }` |

Or call `ToActionResult()` directly when you need explicit control:

```csharp
return (await Mediator.Send(query, ct)).ToActionResult();
```

---

### 4 · BaseController

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : BaseController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => (await Mediator.Send(new GetUserQuery(id), ct)).ToActionResult();

    [HttpDelete("{id}")]
    [RequirePermission("users.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await Mediator.Send(new DeleteUserCommand(id, CurrentUserId!), ct)).ToActionResult();
}
```

`BaseController` provides: `Mediator` (lazy), `CurrentUserId`, `CurrentUserName`, `CurrentUserRole`.

---

### 5 · Middleware

```csharp
app.UseNdbMiddleware();
// 1. GlobalExceptionHandler  — unhandled exceptions → 500 ApiResponse JSON
// 2. CorrelationIdMiddleware — reads/generates X-Correlation-ID, echoes in response
// 3. RequestLoggingMiddleware — logs METHOD PATH → STATUS in Xms actor=userId
```

Slow requests log a Warning at the configured threshold:

```csharp
RequestLoggingMiddleware.SlowRequestThresholdMs = 300; // default: 500ms
```

---

### 6 · Hangfire

```csharp
builder.Services.AddNdbHangfire(
    configure: o =>
    {
        o.WorkerCount       = 10;
        o.Queues            = ["default", "critical"];
        o.BasicAuthPassword = cfg["Hangfire:Password"]!;
    },
    storageCallback: cfg => cfg.UsePostgreSqlStorage(connectionString));

app.UseNdbHangfireDashboard(o =>
{
    o.DashboardUrl      = "/jobs";
    o.BasicAuthUser     = "admin";
    o.BasicAuthPassword = cfg["Hangfire:Password"]!;
});
```

Password is validated at startup. Basic Auth comparison uses `CryptographicOperations.FixedTimeEquals` (timing-safe).

---

### 7 · Swagger

```csharp
builder.Services.AddNdbSwagger(o =>
{
    o.Title          = "My API";
    o.Version        = "v1";
    o.JwtAuthEnabled = true;
});

app.UseNdbSwaggerUI(o => o.RoutePrefix = "docs");
```

---

### 8 · Additional Features

**CORS:**
```csharp
builder.Services.AddNdbCors(o =>
{
    o.AllowedOrigins   = ["https://app.example.com"];
    o.AllowCredentials = true;
});
app.UseCors("NdbDefaultCors");
```

**Forwarded Headers (Nginx, Traefik):**
```csharp
builder.Services.AddNdbForwardedHeaders(o =>
{
    o.ClearExistingKnownHosts = true;
    o.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
});
app.UseNdbForwardedHeaders(); // must be first in the pipeline
```

**Health Checks:**
```csharp
builder.Services.AddNdbHealthChecks();
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, tags: ["ready"]);
app.MapNdbHealthChecks();
// GET /health/live  → liveness
// GET /health/ready → readiness (tags: ["ready"])
```

**API Versioning:**
```csharp
builder.Services.AddNdbApiVersioning();
// /api/v1/users, /api/v2/users — defaults to v1.0
```

**PDF Renderer:**
```csharp
builder.Services.AddScoped<IPdfRenderer, PlaywrightPdfRenderer>();
// In controller:
return PdfService.ToPdfResult(pdfBytes, "invoice-2024-001");
```

---

## Requirements

| Requirement | Detail |
|---|---|
| .NET | 8.0 or 10.0 |
| NDB.Platform.Core | 1.0.0 — installed automatically |
| NDB.Platform.Data | 1.0.0 — installed automatically |
| Hangfire storage | Install separately (`Hangfire.PostgreSql`, `Hangfire.SqlServer`, etc.) |
| `IPermissionResolver` | Required when using `[RequirePermission]` |
| `IPdfRenderer` | Required when using `PdfService.ToPdfResult` |

---

## Ecosystem

| Package | Version | Description |
|---|---|---|
| [NDB.Platform.Core](https://www.nuget.org/packages/NDB.Platform.Core/) | 1.0.0 | Foundation — Result pattern, CQRS, utilities, shared contracts |
| [NDB.Platform.Data](https://www.nuget.org/packages/NDB.Platform.Data/) | 1.0.0 | Data layer — audit trail, EF extensions, CodeGen |
| **NDB.Platform.Api** | 1.0.0 | Web layer ← *you are here* |

---

## Repository

[github.com/ndbco/NDB.Platform.Api](https://github.com/ndbco/NDB.Platform.Api)

---

## About NDB

**PT. Navigate Digital Boundaries** — Indonesian technology company focused on enterprise digital platform development.

🌐 [ndb.co.id](https://ndb.co.id/) — *Navigate Digital Boundaries*

---

## License

[GPL v3](LICENSE) — Copyright © PT. Navigate Digital Boundaries
