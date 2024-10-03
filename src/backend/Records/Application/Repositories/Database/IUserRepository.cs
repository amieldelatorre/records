using Domain.Entities;

namespace Application.Repositories.Database;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);
    Task<bool> EmailExists(string email, CancellationToken cancellationToken);
}