using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Nexbridge.UsersApi.Middleware;

/// <summary>
/// Minimal API-key middleware used to show a custom authentication-style gate.
/// It is intentionally small and project-safe: when no key is configured,
/// the middleware is effectively bypassed.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string? _expectedApiKey;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _expectedApiKey = configuration["Security:ApiKey"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey)
            || !string.Equals(apiKey, _expectedApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                detail = "A valid X-Api-Key header is required."
            });

            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods to keep authentication middleware registration explicit.
/// </summary>
public static class ApiKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
