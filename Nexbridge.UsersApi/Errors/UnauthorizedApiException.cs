using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nexbridge.UsersApi.Errors;

public sealed class UnauthorizedApiException(string detail)
    : ApiErrorException("Unauthorized", StatusCodes.Status401Unauthorized, detail)
{
    public override string Type => "https://api.nexbridge.local/problems/unauthorized";

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
