using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Nexbridge.UsersApi.Middleware;

/// <summary>
/// Minimal API-key middleware used to enforce header-based access control.
/// The API key is required in all non-testing environments unless
/// `Security:ApiKey` is configured in application settings.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string? _expectedApiKey;
    private readonly bool _isTestingEnvironment;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment env)
    {
        _next = next;
        _expectedApiKey = configuration["Security:ApiKey"];
        _isTestingEnvironment = env.IsEnvironment("Testing");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            if (_isTestingEnvironment)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://api.nexbridge.local/problems/unauthorized",
                Title = "Unauthorized",
                Detail = "API key is not configured. Set Security:ApiKey in configuration.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
            });

            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey)
            || !string.Equals(apiKey, _expectedApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://api.nexbridge.local/problems/unauthorized",
                Title = "Unauthorized",
                Detail = "A valid X-Api-Key header is required.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
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
