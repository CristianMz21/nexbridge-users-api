namespace Nexbridge.UsersApi.Tests.Testing;

public sealed record UserResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    int Age,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public sealed record ValidationProblemPayload(
    string? Title,
    string? Detail,
    int? Status,
    string? Type,
    Dictionary<string, string[]>? Errors,
    string? Instance
);

public sealed record ProblemPayload(
    string? Title,
    string? Detail,
    int? Status,
    string? Type,
    string? Instance
);
