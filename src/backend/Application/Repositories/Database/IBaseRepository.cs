using Domain.Entities;

namespace Application.Repositories.Database;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task Create(T entity, CancellationToken token);
    Task<T?> Get(Guid guid, CancellationToken token);
    Task Update(T entity, CancellationToken token);
    Task Delete(T entity, CancellationToken token);
}