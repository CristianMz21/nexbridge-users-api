using System.Collections.Concurrent;

using Nexbridge.UsersApi.Domain.Abstractions;
using Nexbridge.UsersApi.Domain.Entities;

namespace Nexbridge.UsersApi.Infrastructure.Persistence;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly object _sync = new();

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

    public User? Create(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return null;
        }

        lock (_sync)
        {
            if (EmailExists(user.Email))
            {
                return null;
            }

            return _users.TryAdd(user.Id, user)
                ? user
                : null;
        }
    }

    public bool Update(User user)
    {
        lock (_sync)
        {
            if (!_users.TryGetValue(user.Id, out var current))
            {
                return false;
            }

            if (!string.Equals(current.Email, user.Email, StringComparison.OrdinalIgnoreCase)
                && EmailExists(user.Email, user.Id))
            {
                return false;
            }

            return _users.TryUpdate(user.Id, user, current);
        }
    }

    public bool Delete(Guid id)
    {
        return _users.TryRemove(id, out _);
    }

    private bool EmailExists(string email, Guid? excludedUserId = null)
    {
        return _users.Values.Any(
            user => (!excludedUserId.HasValue || user.Id != excludedUserId)
                && string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)
        );
    }
}
