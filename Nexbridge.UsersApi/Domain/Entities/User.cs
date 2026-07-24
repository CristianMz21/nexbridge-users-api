namespace Nexbridge.UsersApi.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string FirstName
    {
        get; set;
    }

    public required string LastName
    {
        get; set;
    }

    public required string Email
    {
        get; set;
    }

    public int Age
    {
        get; set;
    }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt
    {
        get; set;
    }
}
