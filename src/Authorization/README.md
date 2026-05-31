# NDB.Platform.Api.Authorization

Permission-based and role-based authorization for NDB Platform APIs.

---

## Key Types

| Type | Description |
|---|---|
| `RequirePermissionAttribute` | `[RequirePermission("users.create")]` — enforces a granular permission key |
| `PermissionPolicyProvider` | Generates `"perm:{key}"` authorization policies on demand |
| `PermissionAuthorizationHandler` | Evaluates permission via `IPermissionResolver`; supports superadmin bypass |
| `PermissionRequirement` | ASP.NET Core `IAuthorizationRequirement` carrying a permission key |
| `NdbPermissionOptions` | Configures superadmin claim name and bypass roles |
| `NdbRequireRoleAttribute` | `[NdbRequireRole("ADMIN")]` — shorthand for role-based authorization |
| `AuthorizationExtensions` | `AddNdbAuthorization()` — require-auth-by-default policy |

---

## Permission-Based Authorization

### Setup

```csharp
// Program.cs
builder.Services.AddNdbPermissionAuthorization(o =>
{
    o.SuperAdminClaim = "is_superadmin";  // JWT claim whose value "true" bypasses all checks
    o.BypassRoles     = ["SUPER_ADMIN"];  // roles that also bypass all checks
});

// Register your IPermissionResolver implementation:
builder.Services.AddScoped<IPermissionResolver, PermissionResolver>();
```

### Usage

```csharp
[RequirePermission("invoices.create")]
[HttpPost]
public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest req, CancellationToken ct)
    => (await Mediator.Send(new CreateInvoiceCommand(req), ct)).ToActionResult();

// Multiple permissions — all must pass (AND logic):
[RequirePermission("reports.view")]
[RequirePermission("reports.export")]
[HttpGet("{id}/export")]
public async Task<IActionResult> ExportReport(Guid id, CancellationToken ct) { ... }
```

### How it works

1. `RequirePermissionAttribute` sets `Policy = "perm:{key}"` on the `[Authorize]` attribute.
2. `PermissionPolicyProvider.GetPolicyAsync("perm:{key}")` builds an `AuthorizationPolicy` with a `PermissionRequirement` — **no pre-registration** needed per key.
3. `PermissionAuthorizationHandler` resolves the user ID from `ClaimTypes.NameIdentifier` and calls `IPermissionResolver.HasPermissionAsync(userId, key)`.
4. If the JWT contains `is_superadmin: "true"`, or the user has a configured bypass role, the check is skipped entirely.

---

## Role-Based Authorization

```csharp
[NdbRequireRole("ADMIN")]
public IActionResult AdminOnly() { ... }

[NdbRequireRole("ADMIN", "MANAGER")]
public IActionResult AdminOrManager() { ... }
```

`NdbRequireRoleAttribute` is shorthand for `[Authorize(Roles = "ADMIN,MANAGER")]`. It throws `ArgumentException` at startup if called with no roles.

---

## Require Authentication on All Endpoints

```csharp
builder.Services.AddNdbAuthorization();
```

This sets both `DefaultPolicy` and `FallbackPolicy` to require an authenticated user. Every endpoint is protected unless explicitly decorated with `[AllowAnonymous]`.

You can add named policies alongside this:

```csharp
builder.Services.AddNdbAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
});
```

---

## IPermissionResolver

`IPermissionResolver` is defined in `NDB.Platform.Core.Abstraction.Security`. Implement it in your consuming project to look up permissions from your database or cache.

```csharp
public class PermissionResolver(AppDbContext db, IMemoryCache cache) : IPermissionResolver
{
    public async Task<bool> HasPermissionAsync(Guid userId, string key, CancellationToken ct = default)
    {
        var cacheKey = $"perms:{userId}";
        var perms = await cache.GetOrSetAsync(
            cacheKey,
            () => db.UserPermissions
                .Where(p => p.UserId == userId && p.Effect == "ALLOW")
                .Select(p => p.Key)
                .ToHashSetAsync(ct),
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return perms.Contains(key) || perms.Contains("*");
    }

    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default)
        => await db.UserPermissions
            .Where(p => p.UserId == userId)
            .Select(p => p.Key)
            .ToHashSetAsync(ct);

    public Task InvalidateAsync(Guid userId, CancellationToken ct = default)
    {
        cache.Remove($"perms:{userId}");
        return Task.CompletedTask;
    }
}
```

---

## NdbPermissionOptions

| Property | Default | Description |
|---|---|---|
| `SuperAdminClaim` | `"is_superadmin"` | JWT claim name; value `"true"` bypasses all permission checks |
| `BypassRoles` | `[]` | Role codes that receive a full bypass (same as superadmin) |
