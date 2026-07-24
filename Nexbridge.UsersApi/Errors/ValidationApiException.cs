using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nexbridge.UsersApi.Errors;

public sealed class ValidationApiException(
    IReadOnlyDictionary<string, string[]> errors,
    string? title = null,
    string? detail = null
) : ApiErrorException(
    title ?? "Validation failed",
    StatusCodes.Status400BadRequest,
    detail ?? "One or more validation errors were found.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;

    public override string Type => "https://api.nexbridge.local/problems/validation";

    public override ProblemDetails ToProblemDetails(string instance)
    {
        var details = new ValidationProblemDetails
        {
            Type = Type,
            Title = Title,
            Detail = Detail,
            Status = StatusCode,
            Instance = instance
        };

        foreach (var error in Errors)
        {
            details.Errors.Add(error.Key, error.Value);
        }

        return details;
    }
}
