using WebApi.Entities;

namespace WebApi.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T entity);
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<T> GetAsync(string id);
        Task RemoveAsync(string id);
        Task UpdateAsync(T entity);
    }
}