# NDB.Platform.Api.Swagger

Pre-configured Swashbuckle Swagger with JWT Bearer support.

---

## Key Types

| Type | Description |
|---|---|
| `NdbSwaggerOptions` | Title, version, description, JWT toggle, route prefix |
| `SwaggerJwtSecuritySchemeFilter` | Adds JWT Bearer security requirement to every operation |
| `SwaggerExtensions` | `AddNdbSwagger()` and `UseNdbSwaggerUI()` |

---

## Setup

```csharp
// Program.cs
builder.Services.AddNdbSwagger(o =>
{
    o.Title          = "My API";
    o.Version        = "v1";
    o.Description    = "NDB Platform sample API — see docs for authentication details.";
    o.JwtAuthEnabled = true;    // adds Bearer security definition (default: true)
    o.RoutePrefix    = "swagger"; // default — Swagger UI at /swagger
});

app.UseNdbSwaggerUI(o =>
{
    o.Title       = "My API";
    o.Version     = "v1";
    o.RoutePrefix = "docs";  // override to /docs
});
```

---

## JWT in Swagger UI

When `JwtAuthEnabled = true`:
- A **Bearer** security scheme is added to the OpenAPI document
- `SwaggerJwtSecuritySchemeFilter` adds the security requirement to every operation
- The Swagger UI shows an **Authorize** button — paste your JWT token to authenticate all requests

---

## API Versioning Integration

`AddNdbApiVersioning` (from `NDB.Platform.Api.Versioning`) pairs with Swagger to expose per-version documents:

```csharp
builder.Services.AddNdbApiVersioning();
builder.Services.AddNdbSwagger(o => o.Title = "My API");
```

Controllers declare their version:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : BaseController { ... }

[ApiVersion("2.0")]
public class UsersV2Controller : BaseController { ... }
```

Swagger generates separate documents per version: `/swagger/v1/swagger.json`, `/swagger/v2/swagger.json`.

---

## NdbSwaggerOptions Reference

| Property | Default | Description |
|---|---|---|
| `Title` | `NDB API` | API title displayed in Swagger UI |
| `Version` | `v1` | API version and document name |
| `Description` | `null` | Optional description below the title |
| `JwtAuthEnabled` | `true` | Add JWT Bearer security definition |
| `RoutePrefix` | `swagger` | Route prefix for the Swagger UI |
