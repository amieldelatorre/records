using Domain.Common;

namespace Application.Repositories.Database;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task Create(T entity, CancellationToken cancellationToken);
    Task<T?> Get(Guid guid, CancellationToken cancellationToken);
    Task Update(T entity, CancellationToken cancellationToken);
    Task Delete(T entity, CancellationToken cancellationToken);
}