using Nexbridge.UsersApi.Data;
using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.Tests.Unit;

public class InMemoryUserRepositoryTests
{
    [Fact]
    public void CreateAndQueryUsers_ReturnsExpectedValues()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
            FirstName = "Unit",
            LastName = "Tester",
            Email = "unit@example.com",
            Age = 33
        };

        // Act
        var created = repository.Create(user);

        // Assert
        Assert.Equal(user.Id, created.Id);
        Assert.NotNull(repository.GetById(created.Id));
        Assert.Equal(user.Email, repository.GetByEmail(user.Email)?.Email);
        Assert.Single(repository.GetAll());
        Assert.Equal(user.Id, repository.GetAll().First().Id);
    }

    [Fact]
    public void GetAll_ReturnsUsersInCreatedAtOrder()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var earlier = new User
        {
            FirstName = "Earlier",
            LastName = "User",
            Email = "earlier@example.com",
            Age = 22,
            CreatedAt = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var later = new User
        {
            FirstName = "Later",
            LastName = "User",
            Email = "later@example.com",
            Age = 28,
            CreatedAt = new DateTimeOffset(2023, 1, 2, 12, 0, 0, TimeSpan.Zero)
        };

        repository.Create(later);
        repository.Create(earlier);

        // Act
        var users = repository.GetAll().ToArray();

        // Assert
        Assert.Equal(2, users.Length);
        Assert.Equal(earlier.Id, users[0].Id);
        Assert.Equal(later.Id, users[1].Id);
    }

    [Fact]
    public void Update_WhenUserExists_UpdatesRecord()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
            FirstName = "Before",
            LastName = "Name",
            Email = "before@example.com",
            Age = 24
        };

        var created = repository.Create(user);

        var replacement = new User
        {
            Id = created.Id,
            FirstName = "After",
            LastName = "Update",
            Email = "after@example.com",
            Age = 25,
            CreatedAt = created.CreatedAt
        };

        // Act
        var updated = repository.Update(replacement);

        // Assert
        Assert.True(updated);

        var latest = repository.GetById(created.Id);
        Assert.NotNull(latest);
        Assert.Equal("After", latest!.FirstName);
        Assert.Equal("Update", latest.LastName);
        Assert.Equal("after@example.com", latest.Email);
        Assert.Equal(25, latest.Age);
    }

    [Fact]
    public void Update_WhenMissingUser_ReturnsFalse()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Missing",
            LastName = "Update",
            Email = "missing@example.com",
            Age = 30
        };

        // Act
        var updated = repository.Update(user);

        // Assert
        Assert.False(updated);
    }

    [Fact]
    public void Delete_WhenUserExists_RemovesUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = repository.Create(new User
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete@example.com",
            Age = 29
        });

        // Act
        var removed = repository.Delete(user.Id);

        // Assert
        Assert.True(removed);
        Assert.Null(repository.GetById(user.Id));
    }
}
