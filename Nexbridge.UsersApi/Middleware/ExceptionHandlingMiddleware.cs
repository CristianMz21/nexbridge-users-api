using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Nexbridge.UsersApi.Middleware;

/// <summary>
/// Unexpected exceptions are translated into a generic 500 ProblemDetails body.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private const string DefaultUnexpectedTitle = "Unexpected server error.";
    private const string DefaultUnexpectedDetail =
        "An unexpected error occurred while processing the request.";
    private const string DefaultUnexpectedType = "https://api.nexbridge.local/problems/unexpected";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Path}",
                context.Request.Path);

            var problem = new ProblemDetails
            {
                Type = DefaultUnexpectedType,
                Title = DefaultUnexpectedTitle,
                Detail = DefaultUnexpectedDetail,
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path
            };

            await WriteProblemAsync(context, problem);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync((object)problem);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
