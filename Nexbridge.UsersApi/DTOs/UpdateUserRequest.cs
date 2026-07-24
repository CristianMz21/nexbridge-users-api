namespace Nexbridge.UsersApi.DTOs;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    int Age
);
