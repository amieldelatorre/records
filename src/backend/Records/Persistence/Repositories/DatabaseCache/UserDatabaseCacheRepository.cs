using Application.Repositories.DatabaseCache;
using Domain.Entities;

namespace Persistence.Repositories.DatabaseCache;

public class UserDatabaseCacheRepository : IUserDatabaseCacheRepository
{
    public Task<User> Create(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<User> Get(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<User> Update(User entity)
    {
        throw new NotImplementedException();
    }

    public Task Delete(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}