namespace Nexbridge.UsersApi.DTOs;

/// <summary>
/// Input payload used when creating a new user.
/// </summary>
public sealed record CreateUserRequest(
    // Value required by the API consumer. Leading/trailing spaces are trimmed in validation.
    string FirstName,
    string LastName,
    // Stored in lowercase through normalization for deterministic lookups.
    string Email,
    // Inclusive business range is enforced by UserValidator.
    int Age
);
