using Microsoft.AspNetCore.Mvc;

namespace Nexbridge.UsersApi.Errors;

public abstract class ApiErrorException : Exception
{
    private const string ProblemTypePrefix = "https://api.nexbridge.local/problems";

    protected ApiErrorException(
        string title,
        int statusCode,
        string detail
    ) : base(detail)
    {
        Title = title;
        StatusCode = statusCode;
        Detail = detail;
    }

    public string Title
    {
        get;
    }
    public int StatusCode
    {
        get;
    }
    public string Detail
    {
        get;
    }

    public virtual string Type => $"{ProblemTypePrefix}/api";

    public abstract ProblemDetails ToProblemDetails(string instance);
}
