using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Nexbridge.UsersApi.DTOs;
using Nexbridge.UsersApi.Tests.Testing;

namespace Nexbridge.UsersApi.Tests.Integration;

public class UserApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task CreateUser_WhenPayloadIsValid_ReturnsCreatedUser()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var request = new CreateUserRequest(" Ana ", "Taylor", "ANA@EXAMPLE.COM", 34);

        // Act
        using var response = await client.PostAsJsonAsync("/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal("Ana", created.FirstName);
        Assert.Equal("Taylor", created.LastName);
        Assert.Equal("ana@example.com", created.Email);
        Assert.Equal(34, created.Age);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/users/{created.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetAllUsers_WhenUsersExist_ReturnsAllUsers()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var createdA = await CreateUserAsync(client, "Alice", "Roe", "alice@example.com", 28);
        var createdB = await CreateUserAsync(client, "Bob", "Lane", "bob@example.com", 31);

        // Act
        using var response = await client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Equal(2, users!.Count);
        Assert.Contains(users, user => user.Id == createdA.Id);
        Assert.Contains(users, user => user.Id == createdB.Id);
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsUser()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var created = await CreateUserAsync(client, "Carol", "White", "carol@example.com", 29);

        // Act
        using var response = await client.GetAsync($"/users/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions);
        Assert.NotNull(user);
        Assert.Equal(created.Id, user!.Id);
        Assert.Equal(created.FirstName, user.FirstName);
        Assert.Equal(created.Email, user.Email);
    }

    [Fact]
    public async Task UpdateUser_WhenPayloadIsValid_ReturnsUpdatedUser()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var created = await CreateUserAsync(client, "Dave", "Black", "dave@example.com", 25);

        var update = new UpdateUserRequest("David", "Blackman", "david@example.com", 26);

        // Act
        using var response = await client.PutAsJsonAsync($"/users/{created.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal("David", updated.FirstName);
        Assert.Equal("Blackman", updated.LastName);
        Assert.Equal("david@example.com", updated.Email);
        Assert.Equal(26, updated.Age);
        Assert.True(updated.UpdatedAt.HasValue);
    }

    [Fact]
    public async Task DeleteUser_WhenUserExists_ReturnsNoContentAndRemovesUser()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var created = await CreateUserAsync(client, "Eve", "Green", "eve@example.com", 41);

        // Act
        using var deleteResponse = await client.DeleteAsync($"/users/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getResponse = await client.GetAsync($"/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WhenPayloadIsInvalid_ReturnsValidationProblem()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var request = new CreateUserRequest("", "", "not-an-email", 0);

        // Act
        using var response = await client.PostAsJsonAsync("/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemPayload>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem!.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.Equal("One or more validation errors were found.", problem.Detail);
        Assert.NotNull(problem!.Errors);
        Assert.Contains("firstName", problem.Errors.Keys);
        Assert.Contains("lastName", problem.Errors.Keys);
        Assert.Contains("email", problem.Errors.Keys);
        Assert.Contains("age", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetUserById_WhenUserIsMissing_ReturnsNotFound()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        var missingId = Guid.Parse("d20a84dc-8fd2-4f8d-9b8c-9a53f4f4ec8c");

        // Act
        using var response = await client.GetAsync($"/users/{missingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.NotFound, problem!.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal($"User '{missingId}' was not found.", problem.Detail);
        Assert.Equal($"/users/{missingId}", problem.Instance);
        Assert.Equal("https://api.nexbridge.local/problems/not-found", problem.Type);
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var request = new CreateUserRequest("Frank", "Miller", "frank@example.com", 40);

        using var firstResponse = await client.PostAsJsonAsync("/users", request);
        firstResponse.EnsureSuccessStatusCode();

        // Act
        using var response = await client.PostAsJsonAsync("/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal("Email already exists.", problem!.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
        Assert.Equal("A user with this email already exists.", problem.Detail);
        Assert.Equal("https://api.nexbridge.local/problems/conflict", problem.Type);
    }

    [Fact]
    public async Task UpdateUser_WhenEmailConflictsWithAnotherUser_ReturnsConflict()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var first = await CreateUserAsync(client, "Gina", "Doe", "gina@example.com", 20);
        _ = await CreateUserAsync(client, "Henry", "Page", "henry@example.com", 22);

        var update = new UpdateUserRequest("Gina", "Doe", "henry@example.com", 21);

        // Act
        using var response = await client.PutAsJsonAsync($"/users/{first.Id}", update);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal("Email already exists.", problem!.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
        Assert.Equal("Another user already uses this email.", problem.Detail);
        Assert.Equal("https://api.nexbridge.local/problems/conflict", problem.Type);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WhenApiKeyMissing_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory("secret-key");
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        // Act
        using var response = await client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Unauthorized", payload!.Title);
        Assert.Equal((int)HttpStatusCode.Unauthorized, payload.Status);
        Assert.Equal("A valid X-Api-Key header is required.", payload.Detail);
        Assert.Equal("/users", payload.Instance);
        Assert.Equal("https://api.nexbridge.local/problems/unauthorized", payload.Type);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WhenApiKeyValid_ReturnsSuccess()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory("secret-key");
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");

        // Act
        using var response = await client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiKeyMiddleware_WhenApiKeyDisabled_ReturnsSuccess()
    {
        // Arrange
        using var factory = new UserApiApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        // Act
        using var response = await client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<UserResponseDto> CreateUserAsync(
        HttpClient client,
        string firstName,
        string lastName,
        string email,
        int age
    )
    {
        using var response = await client.PostAsJsonAsync(
            "/users",
            new CreateUserRequest(firstName, lastName, email, age));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions);
        Assert.NotNull(created);

        return created;
    }
}
