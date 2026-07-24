using Nexbridge.UsersApi.Application.Interfaces;
using Nexbridge.UsersApi.Application.Results;
using Nexbridge.UsersApi.Application.Validation;
using Nexbridge.UsersApi.Contracts.Users;
using Nexbridge.UsersApi.Domain.Abstractions;
using Nexbridge.UsersApi.Domain.Entities;

namespace Nexbridge.UsersApi.Application.Services;

public sealed class UserService(IUserRepository repository) : IUserService
{
    private static readonly string ValidationTitle = "Validation failed";
    private static readonly string ValidationDetail = "One or more validation errors were found.";

    private readonly IUserRepository _repository = repository;

    public IReadOnlyCollection<UserResponse> GetAll()
    {
        return _repository
            .GetAll()
            .Select(UserResponse.FromEntity)
            .ToArray();
    }

    public UserResult<UserResponse> GetById(Guid id)
    {
        var user = _repository.GetById(id);

        if (user is null)
        {
            return UserResult<UserResponse>.NotFoundResult(
                title: "Not Found",
                detail: $"User '{id}' was not found.");
        }

        return UserResult<UserResponse>.SuccessResult(UserResponse.FromEntity(user));
    }

    public UserResult<UserResponse> Create(CreateUserRequest request)
    {
        var prepared = PrepareNormalizedAndValidatedData(request.FirstName, request.LastName, request.Email, request.Age);

        if (prepared.Status != UserResultStatus.Success)
        {
            return UserResult<UserResponse>.InvalidResult(
                prepared.ValidationErrors ?? new Dictionary<string, string[]>(),
                prepared.Title,
                prepared.Detail);
        }

        if (_repository.GetByEmail(prepared.Email) is not null)
        {
            return UserResult<UserResponse>.EmailConflictResult(
                title: "Email already exists.",
                detail: "A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = prepared.FirstName,
            LastName = prepared.LastName,
            Email = prepared.Email,
            Age = request.Age
        };

        var created = _repository.Create(user);
        if (created is null)
        {
            return UserResult<UserResponse>.EmailConflictResult(
                title: "Email already exists.",
                detail: "A user with this email already exists.");
        }

        return UserResult<UserResponse>.SuccessResult(UserResponse.FromEntity(created));
    }

    public UserResult<UserResponse> Update(Guid id, UpdateUserRequest request)
    {
        var prepared = PrepareNormalizedAndValidatedData(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Age
        );

        if (prepared.Status != UserResultStatus.Success)
        {
            return UserResult<UserResponse>.InvalidResult(
                prepared.ValidationErrors ?? new Dictionary<string, string[]>(),
                prepared.Title,
                prepared.Detail);
        }

        var currentUser = _repository.GetById(id);

        if (currentUser is null)
        {
            return UserResult<UserResponse>.NotFoundResult(
                title: "Not Found",
                detail: $"User '{id}' was not found.");
        }

        var emailOwner = _repository.GetByEmail(prepared.Email);
        if (emailOwner is not null && emailOwner.Id != id)
        {
            return UserResult<UserResponse>.EmailConflictResult(
                title: "Email already exists.",
                detail: "Another user already uses this email.");
        }

        var updatedUser = new User
        {
            Id = currentUser.Id,
            FirstName = prepared.FirstName,
            LastName = prepared.LastName,
            Email = prepared.Email,
            Age = request.Age,
            CreatedAt = currentUser.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updated = _repository.Update(updatedUser);
        if (!updated)
        {
            var updateEmailOwner = _repository.GetByEmail(prepared.Email);
            if (updateEmailOwner is not null && updateEmailOwner.Id != id)
            {
                return UserResult<UserResponse>.EmailConflictResult(
                    title: "Email already exists.",
                    detail: "Another user already uses this email.");
            }

            return UserResult<UserResponse>.UpdateConflictResult(
                title: "Concurrent update conflict.",
                detail: "User was not updated. It may have changed during the request.");
        }

        return UserResult<UserResponse>.SuccessResult(UserResponse.FromEntity(updatedUser));
    }

    private static PreparedUserData PrepareNormalizedAndValidatedData(
        string? firstName,
        string? lastName,
        string? email,
        int age
    )
    {
        var normalizedFirstName = UserValidator.NormalizeName(firstName);
        var normalizedLastName = UserValidator.NormalizeName(lastName);
        var normalizedEmail = UserValidator.NormalizeEmail(email);

        var validationErrors = UserValidator.Validate(
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail,
            age
        );

        if (validationErrors.Any())
        {
            return new PreparedUserData(
                UserResultStatus.InvalidInput,
                validationErrors,
                ValidationTitle,
                ValidationDetail,
                string.Empty,
                string.Empty,
                string.Empty
            );
        }

        return new PreparedUserData(
            UserResultStatus.Success,
            null,
            string.Empty,
            string.Empty,
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail
        );
    }

    private sealed record PreparedUserData(
        UserResultStatus Status,
        IReadOnlyDictionary<string, string[]>? ValidationErrors,
        string Title,
        string Detail,
        string FirstName,
        string LastName,
        string Email
    );

    public UserResult<bool> Delete(Guid id)
    {
        var deleted = _repository.Delete(id);

        if (!deleted)
        {
            return UserResult<bool>.NotFoundResult(
                title: "Not Found",
                detail: $"User '{id}' was not found.");
        }

        return UserResult<bool>.SuccessResult(true);
    }
}
