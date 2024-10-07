using Application.Repositories;
using Application.Repositories.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories.Database;

public class PostgresUserRepository(DataContext dbContext) : IUserRepository
{
    public async Task Create(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> Get(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user;
    }

    public async Task Update(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task Delete(User entity, CancellationToken cancellationToken)
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