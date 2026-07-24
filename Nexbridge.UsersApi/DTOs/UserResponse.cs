using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.DTOs;

/// <summary>
/// Public shape returned to API clients.
/// </summary>
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
    // Maps internal entity state to the API contract, avoiding leakage of
    // persistence details into transport responses.
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
