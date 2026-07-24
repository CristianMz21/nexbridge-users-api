using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nexbridge.UsersApi.Errors;

public sealed class NotFoundApiException(string resourceType, object? resourceKey)
    : ApiErrorException(
        "Not Found",
        StatusCodes.Status404NotFound,
        $"{resourceType} '{resourceKey}' was not found.")
{
    public override string Type => "https://api.nexbridge.local/problems/not-found";

    public override ProblemDetails ToProblemDetails(string instance)
    {
        return new ProblemDetails
        {
            Type = Type,
            Title = Title,
            Detail = Detail,
            Status = StatusCode,
            Instance = instance
        };
    }
}
