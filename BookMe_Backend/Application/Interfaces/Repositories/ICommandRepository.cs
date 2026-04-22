namespace Application.Interfaces
{
    public interface ICommandRepository<T>
    {
        Task CreateAsync(T entity);
        Task UpdateAsync(Guid id, T entity);
        Task DeleteAsync(Guid id);
    }
}
