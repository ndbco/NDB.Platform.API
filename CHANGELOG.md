# Changelog — NDB.Platform.Api

All notable changes are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] — 2026-05-31

### First stable release

- **Dependency**: `NDB.Platform.Core 1.0.0` and `NDB.Platform.Data 1.0.0` switched from `ProjectReference` to `PackageReference` (NuGet)
- Authors & Company updated to **PT. Navigate Digital Boundaries**
- Package license updated to `GPL-3.0-only` (aligned with Core and Data)
- Version reset to `1.0.0` (previous alpha versioning retired)
- Comprehensive XML documentation added to all public API surface
- Per-namespace README files added: Authentication, Authorization, Filters, Middleware, Hangfire, Swagger
- All source comments and error messages translated to English

---

## [2.0.0-alpha.4] — 2026-05-24

### Added
- **Authorization**: `NdbRequireRoleAttribute` — `[Authorize]` wrapper that accepts `params string[] roles`, joined as a comma-separated list
  - Extends `AuthorizeAttribute` — works with the standard ASP.NET Core pipeline
  - Throws `ArgumentException` if called without any role
  - `AttributeUsage`: `AttributeTargets.Class | AttributeTargets.Method`
- **Authorization**: `AddNdbAuthorization(Action<AuthorizationOptions>?)` — registers authorization with a `DefaultPolicy` and `FallbackPolicy` both requiring an authenticated user
- **Authentication**: `HttpContextActorAccessor` — `IActorAccessor` implementation that reads claims from `HttpContext`
  - `ClaimTypes.Name` → `Actor`, `ClaimTypes.NameIdentifier` → `ActorId`, `ClaimTypes.Role` → `Role`
  - Falls back to `AuditActor.System` for unauthenticated requests
  - Registered as Scoped by `AddNdbJwt()`, overriding the `SystemActorAccessor` fallback from Core
- **Filters**: `ResultExtensions.ToActionResult<T>(this CollectionResult<T>)` — converts `CollectionResult` to `OkObjectResult` with shape `{ items, totalCount }`
- **Filters**: `ResultExtensions.ToActionResult<T>(this PagedResult<T>)` — converts `PagedResult` to `OkObjectResult` with shape `{ items, pageInfo }`
- **Filters**: `ResultActionFilter` (rewritten) — `TryFindGenericBase` walks the type hierarchy to detect `PagedResult<T>`, `CollectionResult<T>`, and `Result<T>` via reflection

### Changed (BREAKING)
- **Authorization**: `NdbAuthorizeFilter` removed — incompatible with the minimal ASP.NET Core authorization pipeline
  - **Migration**: Replace `[NdbAuthorizeFilter]` with `[Authorize]` or `[NdbRequireRole("ROLE")]`
  - **Migration**: Replace `services.AddScoped<NdbAuthorizeFilter>()` with `services.AddNdbAuthorization()`
- **Authorization**: `NdbRoleAuthorizeFilter` removed — replaced by `NdbRequireRoleAttribute`
  - **Migration**: Replace `[NdbRoleAuthorizeFilter("ADMIN")]` with `[NdbRequireRole("ADMIN")]`

### Fixed
- `NdbAuthorizeFilter` and `NdbRoleAuthorizeFilter` removed in favor of the standard `[Authorize]` pattern
- `HttpContextActorAccessor` registered as Scoped via `AddNdbJwt()` — overrides the `SystemActorAccessor` singleton from Core
- `ResultActionFilter` now supports `PagedResult<T>` and `CollectionResult<T>` via type hierarchy traversal

---

## [2.0.0-alpha.3] — 2026-05-24

### Added
- **Authentication**: `NdbJwtOptions.RefreshClientName` — named `HttpClient` used by `DefaultTokenRefresher`
- **Authentication**: `NdbJwtOptions.RefreshEndpoint` — configurable refresh endpoint path or URL
- **Authentication**: `NdbJwtOptions.RefreshBaseAddress` — base address of the auth server for refresh requests
- **Authentication**: `NdbJwtOptionsValidator` — `IValidateOptions<NdbJwtOptions>` fail-fast validator
  - Validates: `SigningKey` is required and must be at least 32 characters
  - Validates: `Issuer` and `Audience` are required
  - Registered via `ValidateOnStart()` — configuration errors surface at startup, not at runtime
- **ForwardedHeaders**: `NdbForwardedHeadersOptions` — configuration for `KnownProxies` and `KnownNetworks`
- **ForwardedHeaders**: `AddNdbForwardedHeaders` now accepts an optional `Action<NdbForwardedHeadersOptions>`

### Changed (BREAKING)
- **CORS**: `NdbCorsOptions.AllowCredentials` default changed from `true` → `false`
  - **Migration**: If credentials are needed, set `AllowCredentials = true` AND populate `AllowedOrigins` explicitly
- **CORS**: `AddNdbCors` calls `opts.Validate()` before registration — throws `InvalidOperationException` if `AllowCredentials = true` with an empty `AllowedOrigins`
- **Hangfire**: `UseNdbHangfireDashboard` calls `opts.Validate()` before registering the dashboard
  - **Migration**: Ensure `BasicAuthPassword` is set in production
- **Authentication**: `DefaultTokenRefresher` uses the named `HttpClient` from `options.RefreshClientName`
  - Default client name: `"NdbTokenRefresher"` (backward compatible — not breaking if not set)
- **ForwardedHeaders**: `AddNdbForwardedHeaders` no longer clears `KnownProxies`/`KnownNetworks` by default
  - **Migration**: If you relied on the previous clear-all behavior, set `ClearExistingKnownHosts = true`

### Fixed
- `DefaultTokenRefresher` endpoint was hardcoded to `/api/v1/auth/refresh` — now configurable via `NdbJwtOptions`
- `NdbJwtOptions.SigningKey` was not validated for minimum length — now fails fast at startup
- `AddNdbForwardedHeaders` cleared `KnownProxies`/`KnownNetworks` without any configuration option — now configurable
- Hangfire Basic Auth used `==` for password comparison — replaced with `CryptographicOperations.FixedTimeEquals`
- `NdbCorsOptions` allowed `AllowCredentials = true` with `AllowAnyOrigin` — now explicitly blocked

---

## [2.0.0-alpha.1] — 2026-05-20

### Added
- `NdbJwtOptions` — JWT configuration (issuer, audience, signing key, lifetime, clock skew)
- `NdbJwtBearerEvents` — JWT bearer events with `Token-Expired` header on token expiry
- `ITokenIssuer` — contract for issuing and validating JWT tokens
- `JwtTokenService` — HMAC-SHA256 JWT token service implementation
- `AuthenticationExtensions.AddNdbJwt` — registers JWT Bearer authentication and `ITokenIssuer`
- `HttpContextAccessTokenProvider` — `IAccessTokenProvider` from the HTTP `Authorization: Bearer` header
- `AccessTokenExtensions.AddNdbAccessTokenProvider` — registers `HttpContextAccessTokenProvider`
- `ApiResponse` — standard response envelope for all NDB endpoints
- `ResultExtensions` — extension methods for converting `Result`/`Result<T>` to `IActionResult`
- `ResultActionFilter` — auto-converts handler `Result` return values to the `ApiResponse` envelope
- `BaseController` — abstract base controller with lazy `IMediator` and current-user helpers
- `NdbHangfireOptions` — Hangfire dashboard and server configuration
- `NdbHangfireBasicAuthFilter` — Basic Auth filter for the Hangfire dashboard
- `HangfireExtensions.AddNdbHangfire` — registers Hangfire (provider-agnostic)
- `HangfireExtensions.UseNdbHangfireDashboard` — maps the Hangfire dashboard middleware
- `NdbSwaggerOptions` — Swagger configuration options
- `SwaggerJwtSecuritySchemeFilter` — operation filter for JWT Bearer in Swagger
- `SwaggerExtensions.AddNdbSwagger` — registers Swashbuckle
- `SwaggerExtensions.UseNdbSwaggerUI` — maps the Swagger UI middleware
- `VersioningExtensions.AddNdbApiVersioning` — URL segment API versioning
- `CorrelationIdMiddleware` — reads or generates `X-Correlation-ID` and propagates it
- `RequestLoggingMiddleware` — logs method, path, duration, and status code per request
- `GlobalExceptionHandler` — converts unhandled exceptions to a JSON 500 `ApiResponse`
- `MiddlewareExtensions.AddNdbMiddleware` / `UseNdbMiddleware` — DI and pipeline registration
- `NdbCorsOptions` + `CorsExtensions.AddNdbCors` — CORS policy configuration
- `ForwardedHeadersExtensions` — reverse proxy support (Nginx, Traefik, YARP)
- `HealthCheckExtensions.AddNdbHealthChecks` / `MapNdbHealthChecks` — `/health/live` + `/health/ready`
- Multi-target: `net8.0` + `net10.0`
- Central Package Management (CPM)
- Hangfire storage is provider-agnostic — install the storage provider in the consuming project
