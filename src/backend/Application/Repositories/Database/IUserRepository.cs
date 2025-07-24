using Domain.Entities;

namespace Application.Repositories.Database;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmail(string email, CancellationToken token);
    Task<bool> EmailExists(string email, CancellationToken token);
}