namespace Nexbridge.UsersApi.DTOs;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    int Age
);
