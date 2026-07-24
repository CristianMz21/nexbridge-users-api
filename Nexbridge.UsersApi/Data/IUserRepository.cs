using Nexbridge.UsersApi.Models;

namespace Nexbridge.UsersApi.Data;

public interface IUserRepository
{
    IReadOnlyCollection<User> GetAll();

    User? GetById(Guid id);

    User? GetByEmail(string email);

    User Create(User user);

    bool Update(User user);

    bool Delete(Guid id);
}
