using Nexbridge.UsersApi.Contracts.Users;

namespace Nexbridge.UsersApi.Application.Results;

public enum UserResultStatus
{
    Success,
    InvalidInput,
    NotFound,
    EmailConflict,
    UpdateConflict
}

public sealed record UserResult<T>(
    UserResultStatus Status,
    T? Value = default,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null,
    string? Title = null,
    string? Detail = null
)
{
    public static UserResult<T> SuccessResult(T value) => new(UserResultStatus.Success, value);

    public static UserResult<T> InvalidResult(
        IReadOnlyDictionary<string, string[]> validationErrors,
        string title = "Validation failed",
        string detail = "One or more validation errors were found."
    )
    {
        return new(UserResultStatus.InvalidInput, ValidationErrors: validationErrors, Title: title, Detail: detail);
    }

    public static UserResult<T> NotFoundResult(
        string title = "Not Found",
        string detail = "Resource was not found."
    )
    {
        return new(UserResultStatus.NotFound, Title: title, Detail: detail);
    }

    public static UserResult<T> EmailConflictResult(
        string title = "Email already exists.",
        string detail = "Email conflicts with an existing user."
    )
    {
        return new(UserResultStatus.EmailConflict, Title: title, Detail: detail);
    }

    public static UserResult<T> UpdateConflictResult(
        string title = "Concurrent update conflict.",
        string detail = "User was not updated. It may have changed during the request."
    )
    {
        return new(UserResultStatus.UpdateConflict, Title: title, Detail: detail);
    }
}
