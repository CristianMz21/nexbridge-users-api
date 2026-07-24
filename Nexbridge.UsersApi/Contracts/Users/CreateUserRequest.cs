namespace Nexbridge.UsersApi.Contracts.Users;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    int Age
);
