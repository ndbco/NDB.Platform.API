using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NDB.Platform.Api.Logging;

namespace NDB.Platform.Api.Middleware;

/// <summary>
/// Logs method, path, duration, status code, and actor for every request.
/// Request and response bodies are intentionally not logged to avoid capturing PII.
/// Actor ID is resolved from JWT claims; unauthenticated requests log as <c>"anonymous"</c>.
/// Slow requests (above <see cref="SlowRequestThresholdMs"/>) produce an additional warning log entry.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>Slow request threshold in milliseconds. Default: 500ms.</summary>
    public static long SlowRequestThresholdMs { get; set; } = 500;

    /// <summary>Initializes a new instance of <see cref="RequestLoggingMiddleware"/>.</summary>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            var status = context.Response.StatusCode;
            var duration = sw.ElapsedMilliseconds;

            // Resolve actor from JWT claims; fall back to "anonymous" for unauthenticated requests
            var actorId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "-";
            var traceId = context.TraceIdentifier;

            RequestLogMessages.Completed(_logger, method, path, status, duration, actorId, ip, traceId);

            if (duration > SlowRequestThresholdMs)
                RequestLogMessages.Slow(_logger, method, path, status, duration, SlowRequestThresholdMs);
        }
    }
}
