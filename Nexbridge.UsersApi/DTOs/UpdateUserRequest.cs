namespace Nexbridge.UsersApi.DTOs;

/// <summary>
/// Input payload used when updating an existing user.
/// </summary>
public sealed record UpdateUserRequest(
    // Same fields as creation; validated and normalized before applying changes.
    string FirstName,
    string LastName,
    // Email updates are checked for collisions with other users.
    string Email,
    // Age range is enforced by UserValidator.
    int Age
);
