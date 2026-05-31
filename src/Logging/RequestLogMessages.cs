using Microsoft.Extensions.Logging;

namespace NDB.Platform.Api.Logging;

/// <summary>
/// Source-generated <see cref="LoggerMessage"/> delegates for HTTP request logging.
/// EventId range: 100–199 (RequestLogging domain).
/// </summary>
internal static partial class RequestLogMessages
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} → {StatusCode} in {DurationMs}ms actor={ActorId} ip={Ip} trace={TraceId}")]
    public static partial void Completed(
        ILogger logger,
        string method,
        string path,
        int statusCode,
        long durationMs,
        string actorId,
        string ip,
        string traceId);

    [LoggerMessage(EventId = 101, Level = LogLevel.Warning,
        Message = "HTTP {Method} {Path} → {StatusCode} SLOW {DurationMs}ms (threshold {ThresholdMs}ms)")]
    public static partial void Slow(
        ILogger logger,
        string method,
        string path,
        int statusCode,
        long durationMs,
        long thresholdMs);

    [LoggerMessage(EventId = 102, Level = LogLevel.Error,
        Message = "HTTP {Method} {Path} UNHANDLED exception")]
    public static partial void Unhandled(
        ILogger logger,
        Exception ex,
        string method,
        string path);
}
