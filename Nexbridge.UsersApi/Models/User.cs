namespace Nexbridge.UsersApi.Models;

/// <summary>
/// Domain entity for users kept in the repository.
/// </summary>
public sealed class User
{
    // Identifier is generated when the entity is created in this in-memory model.
    public Guid Id { get; init; } = Guid.NewGuid();

    // Required identity attributes for business and presentation layers.
    public required string FirstName
    {
        get; set;
    }

    // Required identity attributes for business and presentation layers.
    public required string LastName
    {
        get; set;
    }

    // Required identity attributes for business and presentation layers.
    public required string Email
    {
        get; set;
    }

    // Mutable value by design to allow future business rules like updates.
    public int Age
    {
        get; set;
    }

    // Created time is set once by default.
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    // Updated timestamp is null until an update occurs.
    public DateTimeOffset? UpdatedAt
    {
        get; set;
    }
}
