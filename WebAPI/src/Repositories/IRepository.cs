


namespace WebAPI.Repositories;

public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
