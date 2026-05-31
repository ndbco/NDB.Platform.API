# NDB.Platform.Api.Filters

Standard response envelope and automatic `Result` → `IActionResult` conversion.

---

## Key Types

| Type | Description |
|---|---|
| `ApiResponse` | Standard JSON envelope returned by all NDB Platform endpoints |
| `ResultActionFilter` | Auto-converts `Result`/`Result<T>`/`PagedResult<T>`/`CollectionResult<T>` return values |
| `ResultExtensions` | Manual `ToActionResult()` extension methods |

---

## ApiResponse

Every response from an NDB Platform API is wrapped in `ApiResponse`:

```json
// Success with data
{ "success": true, "data": { ... }, "message": null }

// Success with paged data
{ "success": true, "data": { "items": [...], "pageInfo": { "page": 1, "pageSize": 20, "totalItems": 100, "totalPages": 5 } } }

// Error
{ "success": false, "data": null, "message": "Invoice not found.", "errors": null }

// Validation error
{ "success": false, "data": null, "message": null, "validationErrors": { "email": ["Invalid format"] } }
```

---

## ResultActionFilter (Automatic)

Register globally to convert all handler return values automatically — no `ToActionResult()` call in controllers:

```csharp
builder.Services.AddControllers(opt => opt.Filters.Add<ResultActionFilter>());
```

The filter walks the return value's type hierarchy and converts as follows:

| Return type | HTTP status | Response body |
|---|---|---|
| `Result.Success()` | 200 | `{ success: true }` |
| `Result.Success(dto)` | 200 | `{ success: true, data: dto }` |
| `Result.NotFound("msg")` | 404 | `{ success: false, message: "msg" }` |
| `Result.BadRequest("msg")` | 400 | `{ success: false, message: "msg" }` |
| `Result.Conflict("msg")` | 409 | `{ success: false, message: "msg" }` |
| `Result.Unauthorized("msg")` | 401 | `{ success: false, message: "msg" }` |
| `Result.Forbidden("msg")` | 403 | `{ success: false, message: "msg" }` |
| `Result.Validation(errors)` | 400 | `{ success: false, validationErrors: {...} }` |
| `Result.Error("msg")` | 500 | `{ success: false, message: "msg" }` |
| `PagedResult<T>.Success(...)` | 200 | `{ success: true, data: { items, pageInfo } }` |
| `CollectionResult<T>.Success(...)` | 200 | `{ success: true, data: { items, totalCount } }` |

**Controller example with filter registered globally:**

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    => await Mediator.Send(new GetUserQuery(id), ct);
    // Result<UserResponse> returned directly — filter handles the conversion
```

---

## ResultExtensions (Manual)

When you need explicit control, call `ToActionResult()` directly:

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
{
    var result = await Mediator.Send(new CreateUserCommand(req), ct);
    return result.ToActionResult();
}

[HttpGet]
public async Task<IActionResult> ListUsers([FromQuery] PagingRequest paging, CancellationToken ct)
{
    var result = await Mediator.Send(new GetUsersPagedQuery(paging), ct);
    return result.ToActionResult();
    // PagedResult<UserResponse> → { success: true, data: { items, pageInfo } }
}
```
