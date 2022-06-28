using DataAccess.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class ItemsRepository : GenericRepository<Item>, IItemsRepository
{
    private readonly PersonalBudgetContext _context;

    public ItemsRepository(PersonalBudgetContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> EqualExistsAsync(OperationType type, string name, int id = 0)
    {
        var equal = await _context.Items
            .Where(item => item.Id != id && item.Type == type && item.Name == name)
            .FirstOrDefaultAsync();
        return equal is not null;
    }

    public async Task<IEnumerable<Item>> GetAllByTypesAsync(IEnumerable<OperationType> types)
    {
        return await _context.Items
            .Where(item => types.Contains(item.Type))
            .ToListAsync();
    }
}
