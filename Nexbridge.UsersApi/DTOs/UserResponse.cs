using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.DTOs;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    int Age,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public static UserResponse FromEntity(User user)
    {
        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Age,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
