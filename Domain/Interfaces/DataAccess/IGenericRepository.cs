namespace Domain.Interfaces.DataAccess;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> PostAsync(T entity);
    Task<T?> PutAsync(int id, T entity);
    Task<bool> DeleteAsync(int id);
}
