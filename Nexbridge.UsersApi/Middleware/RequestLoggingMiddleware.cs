using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Nexbridge.UsersApi.Middleware;

/// <summary>
/// Middleware that writes one line when a request starts and one line
/// when the response finishes, including duration and status code.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();

        _logger.LogInformation(
            "Incoming request {Method} {Path}{QueryString}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        await _next(context);

        var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        _logger.LogInformation(
            "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            Math.Round(elapsedMs, 2));
    }
}

/// <summary>
/// Extension methods to keep middleware registration explicit and self-documented.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
