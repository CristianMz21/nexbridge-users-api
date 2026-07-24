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
    Dictionary<string, string[]>? Errors
);

public sealed record ProblemPayload(
    string? Title,
    string? Detail,
    int? Status
);
