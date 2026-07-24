using Microsoft.AspNetCore.Http;

using Nexbridge.UsersApi.Data;
using Nexbridge.UsersApi.DTOs;
using Nexbridge.UsersApi.Models;
using Nexbridge.UsersApi.Validation;

namespace Nexbridge.UsersApi.Endpoints;

/// <summary>
/// Registers user CRUD endpoints and keeps transport-level concerns
/// (routing, validation response mapping, conflict handling) together.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Registers all routes under /users in a single feature group.
    /// </summary>
    public static void MapUserEndpoints(this WebApplication app)
    {
        // Grouping endpoints improves discoverability and makes
        // route-level metadata (tags, conventions) easier to apply.
        var group = app.MapGroup("/users").WithTags("Users");

        // GET /users
        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers");

        // GET /users/{id}
        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById");

        // POST /users
        group.MapPost("/", CreateUser)
            .WithName("CreateUser");

        // PUT /users/{id}
        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser");

        // DELETE /users/{id}
        group.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser");
    }

    // Returns all users sorted by CreatedAt, projected to API response DTOs.
    private static IResult GetAllUsers(IUserRepository repository)
    {
        var users = repository
            .GetAll()
            .Select(UserResponse.FromEntity)
            .ToArray();

        return TypedResults.Ok(users);
    }

    // Fetches one user by id; returns 404 when not found.
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

    // Creates a user after trimming/normalizing incoming values and
    // validating business rules before writing to the repository.
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

        // Email uniqueness is a repository-level business rule checked
        // here because it requires reading current persisted data.
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

    // Replaces an existing user while keeping CreatedAt immutable and
    // updating UpdatedAt so clients can detect modifications.
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

        // Must ensure the target exists before updating and protect
        // against swapping email with another existing user.
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

    // Removes one user by id and returns 204 on success or 404 when absent.
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
