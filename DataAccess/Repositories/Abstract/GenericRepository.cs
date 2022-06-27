using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Abstract;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly PersonalBudgetContext _context;

    public GenericRepository(PersonalBudgetContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> PostAsync(T entity)
    {
        var result = await _context.Set<T>().AddAsync(entity);
        return result.Entity;
    }

    public async Task<T?> PutAsync(int id, IDTO dto)
    {
        var entity = await _context.FindAsync<T>(id);

        if (entity is null)
            return null;

        var entry = _context.Entry(entity);
        entry.CurrentValues.SetValues(dto);
        return entry.Entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.FindAsync<T>(id);

        if (entity is null)
            return false;

        var entry = _context.Entry(entity);
        entry.State = EntityState.Deleted;

        return true;
    }
}
