using System.Collections.Concurrent;

using Nexbridge.UsersApi.Errors;
using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.Data;

/// <summary>
/// In-memory repository used as a lightweight development persistence layer.
/// It is thread-safe to keep endpoint calls safe under concurrent requests.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    // ConcurrentDictionary provides lock-free reads and atomic updates for this stage.
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    // Returns users ordered by creation date to provide deterministic list output.
    public IReadOnlyCollection<User> GetAll()
    {
        return _users.Values
            .OrderBy(user => user.CreatedAt)
            .ToArray();
    }

    // O(1) lookup by identifier.
    public User? GetById(Guid id)
    {
        return _users.GetValueOrDefault(id);
    }

    // Case-insensitive email lookup to support realistic login/identity behavior.
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

    // Inserts a new user. If the generated id already exists, this is considered
    // a rare conflict and is surfaced as a ConflictApiException.
    public User Create(User user)
    {
        if (!_users.TryAdd(user.Id, user))
        {
            throw new ConflictApiException(
                "Concurrent create conflict.",
                $"Could not create the user with ID '{user.Id}'."
            );
        }

        return user;
    }

    // Updates atomically with optimistic concurrency-style retry: read current value
    // and replace only if it has not changed between read and write.
    public bool Update(User user)
    {
        while (_users.TryGetValue(user.Id, out var currentUser))
        {
            if (_users.TryUpdate(user.Id, user, currentUser))
            {
                return true;
            }
        }

        return false;
    }

    // Removes a user by id and returns whether a row was actually deleted.
    public bool Delete(Guid id)
    {
        return _users.TryRemove(id, out _);
    }
}
