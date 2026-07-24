using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nexbridge.UsersApi.Errors;

public sealed class ConflictApiException(string title, string detail)
    : ApiErrorException(title, StatusCodes.Status409Conflict, detail)
{
    public override string Type => "https://api.nexbridge.local/problems/conflict";

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
