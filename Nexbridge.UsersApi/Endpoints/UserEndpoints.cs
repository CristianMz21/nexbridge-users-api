using Microsoft.AspNetCore.Http;

using Nexbridge.UsersApi.Data;
using Nexbridge.UsersApi.DTOs;
using Nexbridge.UsersApi.Models;
using Nexbridge.UsersApi.Validation;

namespace Nexbridge.UsersApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers");

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById");

        group.MapPost("/", CreateUser)
            .WithName("CreateUser");

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser");

        group.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser");
    }

    private static IResult GetAllUsers(IUserRepository repository)
    {
        var users = repository
            .GetAll()
            .Select(UserResponse.FromEntity)
            .ToArray();

        return TypedResults.Ok(users);
    }

    private static IResult GetUserById(
        Guid id,
        IUserRepository repository
    )
    {
        var user = repository.GetById(id);

        if (user is null)
        {
            return TypedResults.NotFound(new
            {
                message = $"User '{id}' was not found."
            });
        }

        return TypedResults.Ok(UserResponse.FromEntity(user));
    }

    private static IResult CreateUser(
        CreateUserRequest request,
        IUserRepository repository
    )
    {
        var normalizedFirstName = UserValidator.NormalizeName(request.FirstName);
        var normalizedLastName = UserValidator.NormalizeName(request.LastName);
        var normalizedEmail = UserValidator.NormalizeEmail(request.Email);

        var validationErrors = UserValidator.Validate(
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail,
            request.Age
        );

        if (validationErrors.Any())
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var emailAlreadyExists = repository.GetByEmail(normalizedEmail);

        if (emailAlreadyExists is not null)
        {
            return TypedResults.Problem(
                detail: "A user with this email already exists.",
                title: "Email already exists.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var user = new User
        {
            FirstName = normalizedFirstName,
            LastName = normalizedLastName,
            Email = normalizedEmail,
            Age = request.Age
        };

        var created = repository.Create(user);

        return TypedResults.Created($"/users/{created.Id}", UserResponse.FromEntity(created));
    }

    private static IResult UpdateUser(
        Guid id,
        UpdateUserRequest request,
        IUserRepository repository
    )
    {
        var normalizedFirstName = UserValidator.NormalizeName(request.FirstName);
        var normalizedLastName = UserValidator.NormalizeName(request.LastName);
        var normalizedEmail = UserValidator.NormalizeEmail(request.Email);

        var validationErrors = UserValidator.Validate(
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail,
            request.Age
        );

        if (validationErrors.Any())
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var currentUser = repository.GetById(id);

        if (currentUser is null)
        {
            return TypedResults.NotFound(new
            {
                message = $"User '{id}' was not found."
            });
        }

        var emailOwner = repository.GetByEmail(normalizedEmail);

        if (emailOwner is not null && emailOwner.Id != id)
        {
            return TypedResults.Problem(
                detail: "Another user already uses this email.",
                title: "Email already exists.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var updatedUser = new User
        {
            Id = currentUser.Id,
            FirstName = normalizedFirstName,
            LastName = normalizedLastName,
            Email = normalizedEmail,
            Age = request.Age,
            CreatedAt = currentUser.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updateSucceeded = repository.Update(updatedUser);

        if (!updateSucceeded)
        {
            return TypedResults.Problem(
                detail: "User was not updated. It may have changed during the request.",
                title: "Concurrent update conflict.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        return TypedResults.Ok(UserResponse.FromEntity(updatedUser));
    }

    private static IResult DeleteUser(
        Guid id,
        IUserRepository repository
    )
    {
        var deleted = repository.Delete(id);

        if (!deleted)
        {
            return TypedResults.NotFound(new
            {
                message = $"User '{id}' was not found."
            });
        }

        return TypedResults.NoContent();
    }
}
