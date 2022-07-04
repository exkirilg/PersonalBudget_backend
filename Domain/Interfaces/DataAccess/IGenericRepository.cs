namespace Domain.Interfaces.DataAccess;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T> PostAsync(T entity);
    Task<T?> PutAsync(int id, IDTO dto);
    Task<bool> DeleteAsync(int id);
}
