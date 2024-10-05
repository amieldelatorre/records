using Application.Repositories;
using Application.Repositories.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories.Database;

public class PostgresUserRepository(DataContext dbContext) : IUserRepository
{
    public async Task<User> Create(User entity)
    {
        await dbContext.Users.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<User?> Get(Guid userId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user;
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
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        return user;
    }

    public async Task<bool> EmailExists(string email, CancellationToken cancellationToken)
    {
        var user = await GetByEmail(email, cancellationToken);
        return user != null;
    }
}