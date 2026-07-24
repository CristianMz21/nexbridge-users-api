using System.Collections.Concurrent;

using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.Data;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public IReadOnlyCollection<User> GetAll()
    {
        return _users.Values
            .OrderBy(user => user.CreatedAt)
            .ToArray();
    }

    public User? GetById(Guid id)
    {
        return _users.GetValueOrDefault(id);
    }

    public User? GetByEmail(string email)
    {
        return _users.Values.FirstOrDefault(
            user => string.Equals(
                user.Email,
                email,
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    public User Create(User user)
    {
        if (!_users.TryAdd(user.Id, user))
        {
            throw new InvalidOperationException(
                $"Could not create the user with ID '{user.Id}'."
            );
        }

        return user;
    }

    public bool Update(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            return false;
        }

        _users[user.Id] = user;
        return true;
    }

    public bool Delete(Guid id)
    {
        return _users.TryRemove(id, out _);
    }
}
