using Nexbridge.UsersApi.Domain.Entities;

namespace Nexbridge.UsersApi.Domain.Abstractions;

public interface IUserRepository
{
    IReadOnlyCollection<User> GetAll();

    User? GetById(Guid id);

    User? GetByEmail(string email);

    User? Create(User user);

    bool Update(User user);

    bool Delete(Guid id);
}
