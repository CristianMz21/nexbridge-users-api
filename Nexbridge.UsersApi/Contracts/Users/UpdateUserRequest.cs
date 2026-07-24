namespace Nexbridge.UsersApi.Contracts.Users;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    int Age
);
