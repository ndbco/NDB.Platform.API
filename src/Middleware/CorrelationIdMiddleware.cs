using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NDB.Platform.Kit.Identifiers;

namespace NDB.Platform.Api.Middleware;

/// <summary>Reads or generates an <c>X-Correlation-ID</c> header and propagates it through the request pipeline.</summary>
public sealed partial class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>Initializes a new instance of <see cref="CorrelationIdMiddleware"/>.</summary>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
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

        string correlationId;
        if (context.Request.Headers.TryGetValue(CorrelationId.HeaderName, out var existing)
            && !string.IsNullOrEmpty(existing))
        {
            correlationId = existing.ToString();
            CorrelationId.Value = correlationId;
        }
        else
        {
            correlationId = CorrelationId.GetOrCreate();
        }

        context.Response.Headers[CorrelationId.HeaderName] = correlationId;
        LogCorrelation(_logger, correlationId);

        await _next(context);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Correlation-ID: {CorrelationId}")]
    private static partial void LogCorrelation(ILogger logger, string correlationId);
}
