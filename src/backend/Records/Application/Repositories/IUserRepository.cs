using Domain.Entities;

namespace Application.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);
}