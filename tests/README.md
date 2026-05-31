# NDB.Platform.Api — Tests

Unit and integration test suite for NDB.Platform.Api. Multi-targeted: `net8.0` + `net10.0`.

---

## Structure

```
tests/
├── AccessToken/
│   └── HttpContextAccessTokenProviderTests.cs      — Bearer token extraction from headers
├── Authentication/
│   ├── DefaultTokenRefresherConfigTests.cs         — options + refresh URI resolution
│   ├── DefaultTokenRefresherTests.cs               — refresh flow, error cases
│   ├── InMemoryTokenStorageTests.cs                — thread-safety, get/set/clear
│   ├── JwtTokenServiceTests.cs                     — issue, validate, expired, tampered
│   ├── NdbJwtBearerEventsTests.cs                  — Token-Expired header on expiry
│   ├── NdbJwtOptionsTests.cs                       — defaults, property setters
│   ├── NdbJwtOptionsValidatorTests.cs              — fail-fast: key length, issuer, audience
├── Authorization/
│   ├── PermissionAuthorizationHandlerTests.cs      — HasPermission, superadmin bypass, unauthenticated
│   ├── PermissionPolicyProviderTests.cs            — on-demand policy generation, fallback
│   └── PermissionRequirementTests.cs              — constructor, property
├── Controllers/
│   └── BaseControllerTests.cs                     — CurrentUserId/Name/Role claim extraction
├── Cors/
│   └── NdbCorsOptionsTests.cs                     — Validate() — credentials + AllowAnyOrigin blocked
├── Filters/
│   ├── ApiResponseTests.cs                        — constructor, property defaults
│   ├── NdbAuthorizeFilterTests.cs                 — legacy filter (removed in alpha.4)
│   └── ResultExtensionsTests.cs                  — all ResultStatus → HTTP status mapping
├── ForwardedHeaders/
│   └── NdbForwardedHeadersOptionsTests.cs         — defaults, KnownProxies, ClearExistingKnownHosts
├── Hangfire/
│   ├── NdbHangfireBasicAuthFilterFixedTimeEqualsTests.cs — timing-safe comparison
│   ├── NdbHangfireBasicAuthFilterTests.cs         — valid credentials, missing header, wrong password
│   └── NdbHangfireOptionsTests.cs                — Validate() — empty password/user rejected
├── Integration/
│   ├── ApiPipelineTests.cs                        — full pipeline: auth, middleware, result filter
│   └── TestWebAppFactory.cs                       — WebApplicationFactory setup
├── Middleware/
│   ├── CorrelationIdMiddlewareTests.cs            — read header, generate when missing, echo in response
│   ├── GlobalExceptionHandlerTests.cs            — unhandled exception → 500 ApiResponse
│   ├── RequestLoggingMiddlewareActorTests.cs      — actor extraction from JWT claims
│   └── RequestLoggingMiddlewareTests.cs          — duration, slow warning, anonymous fallback
└── Swagger/
    └── NdbSwaggerOptionsTests.cs                 — defaults, title, JwtAuthEnabled
```

---

## Running Tests

```bash
# From the NDB.Platform.API/ root:
dotnet test --configuration Release

# With coverage:
dotnet test --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

# Filter by namespace:
dotnet test --filter "FullyQualifiedName~Authentication"
dotnet test --filter "FullyQualifiedName~Authorization"
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## Integration Tests

`ApiPipelineTests` spins up a full `WebApplicationFactory` with:
- JWT authentication enabled
- `ResultActionFilter` registered
- `NdbMiddleware` pipeline active
- In-memory Hangfire storage

These tests verify the end-to-end pipeline — authentication, authorization, and response shape — without mocking the middleware.

---

## Coverage Target

≥ 80% line coverage for `NDB.Platform.Api` (auto-generated code excluded).
