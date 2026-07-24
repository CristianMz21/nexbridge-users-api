using Nexbridge.UsersApi.Application.Results;
using Nexbridge.UsersApi.Contracts.Users;

namespace Nexbridge.UsersApi.Application.Interfaces;

public interface IUserService
{
    IReadOnlyCollection<UserResponse> GetAll();

    UserResult<UserResponse> GetById(Guid id);

    UserResult<UserResponse> Create(CreateUserRequest request);

    UserResult<UserResponse> Update(Guid id, UpdateUserRequest request);

    UserResult<bool> Delete(Guid id);
}
