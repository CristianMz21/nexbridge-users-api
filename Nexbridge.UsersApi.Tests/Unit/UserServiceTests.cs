using Nexbridge.UsersApi.Application.Interfaces;
using Nexbridge.UsersApi.Application.Results;
using Nexbridge.UsersApi.Application.Services;
using Nexbridge.UsersApi.Contracts.Users;
using Nexbridge.UsersApi.Infrastructure.Persistence;

namespace Nexbridge.UsersApi.Tests.Unit;

public sealed class UserServiceTests
{
    [Fact]
    public void Create_WhenPayloadIsValid_ReturnsCreatedUser()
    {
        // Arrange
        IUserService service = new UserService(new InMemoryUserRepository());
        var request = new CreateUserRequest(" Ana ", "Taylor", "ANA@EXAMPLE.COM", 34);

        // Act
        var result = service.Create(request);

        // Assert
        Assert.Equal(UserResultStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal("Ana", result.Value.FirstName);
        Assert.Equal("Taylor", result.Value.LastName);
        Assert.Equal("ana@example.com", result.Value.Email);
        Assert.Equal(34, result.Value.Age);
    }

    [Fact]
    public void Create_WhenPayloadIsInvalid_ReturnsValidationError()
    {
        // Arrange
        IUserService service = new UserService(new InMemoryUserRepository());
        var request = new CreateUserRequest("", "", "not-an-email", 0);

        // Act
        var result = service.Create(request);

        // Assert
        Assert.Equal(UserResultStatus.InvalidInput, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("firstName", result.ValidationErrors!.Keys);
        Assert.Contains("lastName", result.ValidationErrors.Keys);
        Assert.Contains("email", result.ValidationErrors.Keys);
        Assert.Contains("age", result.ValidationErrors.Keys);
    }

    [Fact]
    public void Create_WhenEmailAlreadyExists_ReturnsEmailConflict()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        IUserService service = new UserService(repository);

        var request = new CreateUserRequest("Frank", "Miller", "frank@example.com", 40);
        service.Create(request);

        // Act
        var result = service.Create(request);

        // Assert
        Assert.Equal(UserResultStatus.EmailConflict, result.Status);
        Assert.Equal("Email already exists.", result.Title);
        Assert.Equal("A user with this email already exists.", result.Detail);
    }

    [Fact]
    public void GetById_WhenUserMissing_ReturnsNotFound()
    {
        // Arrange
        IUserService service = new UserService(new InMemoryUserRepository());

        // Act
        var result = service.GetById(Guid.Parse("d20a84dc-8fd2-4f8d-9b8c-9a53f4f4ec8c"));

        // Assert
        Assert.Equal(UserResultStatus.NotFound, result.Status);
        Assert.Equal("Not Found", result.Title);
        Assert.Equal("User 'd20a84dc-8fd2-4f8d-9b8c-9a53f4f4ec8c' was not found.", result.Detail);
    }

    [Fact]
    public void Update_WhenPayloadIsValid_ReturnsUpdatedUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        IUserService service = new UserService(repository);

        var created = service.Create(new CreateUserRequest("Dave", "Black", "dave@example.com", 25));
        var createdAt = created.Value!.CreatedAt;

        var request = new UpdateUserRequest("David", "Blackman", "david@example.com", 26);

        // Act
        var result = service.Update(created.Value!.Id, request);

        // Assert
        Assert.Equal(UserResultStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(created.Value!.Id, result.Value!.Id);
        Assert.Equal("David", result.Value.FirstName);
        Assert.Equal("Blackman", result.Value.LastName);
        Assert.Equal("david@example.com", result.Value.Email);
        Assert.Equal(26, result.Value.Age);
        Assert.Equal(createdAt, result.Value.CreatedAt);
        Assert.True(result.Value.UpdatedAt.HasValue);
    }

    [Fact]
    public void Update_WhenUserMissing_ReturnsNotFound()
    {
        // Arrange
        IUserService service = new UserService(new InMemoryUserRepository());
        var request = new UpdateUserRequest("Missing", "User", "missing@example.com", 31);

        // Act
        var result = service.Update(Guid.NewGuid(), request);

        // Assert
        Assert.Equal(UserResultStatus.NotFound, result.Status);
        Assert.Equal("Not Found", result.Title);
    }

    [Fact]
    public void Update_WhenEmailBelongsToAnotherUser_ReturnsEmailConflict()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        IUserService service = new UserService(repository);

        var first = service.Create(new CreateUserRequest("Gina", "Doe", "gina@example.com", 20));
        service.Create(new CreateUserRequest("Henry", "Page", "henry@example.com", 22));

        // Act
        var result = service.Update(
            first.Value!.Id,
            new UpdateUserRequest("Gina", "Doe", "henry@example.com", 21));

        // Assert
        Assert.Equal(UserResultStatus.EmailConflict, result.Status);
        Assert.Equal("Email already exists.", result.Title);
        Assert.Equal("Another user already uses this email.", result.Detail);
    }

    [Fact]
    public void Delete_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        IUserService service = new UserService(repository);

        var created = service.Create(new CreateUserRequest("Eve", "Green", "eve@example.com", 41));

        // Act
        var result = service.Delete(created.Value!.Id);

        // Assert
        Assert.Equal(UserResultStatus.Success, result.Status);
        Assert.True(result.Value);
        Assert.Equal(UserResultStatus.NotFound, service.GetById(created.Value!.Id).Status);
    }

    [Fact]
    public void Delete_WhenUserMissing_ReturnsNotFound()
    {
        // Arrange
        IUserService service = new UserService(new InMemoryUserRepository());

        // Act
        var result = service.Delete(Guid.Parse("1f8d9dbf-8f8c-4a5a-81c6-fbe4f58d1a2f"));

        // Assert
        Assert.Equal(UserResultStatus.NotFound, result.Status);
        Assert.Equal("Not Found", result.Title);
    }
}
