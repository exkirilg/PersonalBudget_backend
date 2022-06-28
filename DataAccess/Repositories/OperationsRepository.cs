using DataAccess.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class OperationsRepository : GenericRepository<Operation>, IOperationsRepository
{
    private readonly PersonalBudgetContext _context;

    public OperationsRepository(PersonalBudgetContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Operation>> GetAllByTypesOverTimePeriodAsync(
        string userId, bool userIsAdmin, IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo)
    {
        return await _context.Operations
            .Where(operation =>
                (userIsAdmin || operation.AuthorId == userId) &&
                types.Contains(operation.Type) &&
                operation.Date >= dateFrom &&
                operation.Date <= dateTo)
            .Include(operation => operation.Item)
            .ToListAsync();
    }

    public new async Task<Operation?> GetByIdAsync(int id)
    {
        return await _context.Operations
            .Where(operation => operation.Id == id)
            .Include(operation => operation.Item)
            .FirstOrDefaultAsync();
    }
}
