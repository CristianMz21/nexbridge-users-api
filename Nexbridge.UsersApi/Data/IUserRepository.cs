using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.Data;

/// <summary>
/// Abstraction for user persistence used by endpoints and future storage engines.
/// </summary>
public interface IUserRepository
{
    // Full read list used for the collection endpoint.
    IReadOnlyCollection<User> GetAll();

    // Read one user by its identifier.
    User? GetById(Guid id);

    // Read one user by email, used for uniqueness checks.
    User? GetByEmail(string email);

    // Insert and return the created entity.
    User Create(User user);

    // Replace an existing user and return whether the write succeeded.
    bool Update(User user);

    // Delete one user by identifier.
    bool Delete(Guid id);
}
