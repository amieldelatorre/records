using Application.Repositories.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories.Database;

public class PostgreSqlUserRepository(DataContext dbContext) : IUserRepository
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

    public async Task Delete(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        return user;
    }

    public async Task<bool> EmailExists(string email, CancellationToken cancellationToken)
    {
        // Use AnyAsync instead of SingleOrDefaultAsync because we don't need to load the whole entity
        var exists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        return exists;
    }
    
    public async Task<User?> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var user  = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);
        return user;
    }

    public async Task<bool> UsernameExists(string username, CancellationToken cancellationToken)
    {
        // Use AnyAsync instead of SingleOrDefaultAsync because we don't need to load the whole entity
        var exists = await dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);
        return exists;
    }
}