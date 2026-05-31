# NDB.Platform.Api.Middleware

Correlation ID propagation, structured request logging, and global exception handling.

---

## Key Types

| Type | Description |
|---|---|
| `CorrelationIdMiddleware` | Reads or generates `X-Correlation-ID` and echoes it in the response |
| `RequestLoggingMiddleware` | Logs method, path, duration, status code, and actor per request |
| `GlobalExceptionHandler` | Converts unhandled exceptions to a JSON 500 `ApiResponse` |
| `MiddlewareExtensions` | `AddNdbMiddleware()` and `UseNdbMiddleware()` |

---

## Setup

```csharp
// Program.cs — registration
builder.Services.AddNdbMiddleware();

// Program.cs — pipeline (before UseAuthentication / UseAuthorization)
app.UseNdbMiddleware();
```

`UseNdbMiddleware()` registers the three components in order:
1. `GlobalExceptionHandler` (ASP.NET Core `IExceptionHandler`)
2. `CorrelationIdMiddleware`
3. `RequestLoggingMiddleware`

---

## CorrelationIdMiddleware

Reads the incoming `X-Correlation-ID` header. If absent, generates a new ID via `CorrelationId.GetOrCreate()` (format: `req-{ulid}`). The resolved ID is:
- Stored in `CorrelationId.Value` (AsyncLocal — accessible throughout the request)
- Echoed in the response via `X-Correlation-ID`

Clients can include this header in outbound requests to maintain a trace chain across services. Log aggregators (Seq, Grafana Loki, etc.) can correlate log entries using this ID.

```
GET /api/v1/users HTTP/1.1
X-Correlation-ID: req-01HZWK9KT1F8GVMX3RQJB2E4P

HTTP/1.1 200 OK
X-Correlation-ID: req-01HZWK9KT1F8GVMX3RQJB2E4P
```

---

## RequestLoggingMiddleware

Logs one structured entry per request after the response is sent:

```
[INF] HTTP GET /api/v1/users → 200 in 42ms actor=3f8a9c2d ip=10.0.0.1 trace=0HN12345:00000001
```

Slow requests (above the threshold) produce an additional Warning entry:

```
[WRN] HTTP GET /api/v1/reports/export → 200 SLOW 1234ms (threshold 500ms)
```

**Configure the slow request threshold:**

```csharp
RequestLoggingMiddleware.SlowRequestThresholdMs = 300; // default: 500ms
```

Request and response bodies are intentionally never logged to avoid capturing PII.

---

## GlobalExceptionHandler

Catches all unhandled exceptions and returns a consistent JSON response with status 500:

```json
{
  "success": false,
  "data": null,
  "message": "An internal error occurred. Please try again.",
  "errors": null
}
```

The exception is logged at `Error` level with the full stack trace. The response body intentionally omits exception details to avoid leaking internals to clients.
