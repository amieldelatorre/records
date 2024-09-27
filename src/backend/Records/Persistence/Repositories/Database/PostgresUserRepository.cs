using Application.Repositories;
using Application.Repositories.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories.Database;

public class PostgresUserRepository(DataContext dbcontext) : IBaseRepository<User>, IUserRepository
{
    public async Task<User> Create(User entity)
    {
        await dbcontext.Users.AddAsync(entity);
        await dbcontext.SaveChangesAsync();
        return entity;
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

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await dbcontext.Users.SingleOrDefaultAsync(u => u.Email == email);
        return user;
    }
}