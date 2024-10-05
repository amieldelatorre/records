using Domain.Common;

namespace Application.Repositories.Database;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T> Create(T entity);
    Task<T?> Get(Guid guid);
    Task<T> Update(T entity);
    Task Delete(T entity);
}