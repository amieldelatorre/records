using Domain.Common;

namespace Application.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T> Create(T entity);
    Task<T> Get(T entity);
    Task<T> Update(T entity);
    Task Delete(T entity);
}