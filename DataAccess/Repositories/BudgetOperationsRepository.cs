using DataAccess.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class BudgetOperationsRepository : GenericRepository<Operation>, IOperationsRepository
{
    private readonly PersonalBudgetContext _context;

    public BudgetOperationsRepository(PersonalBudgetContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Operation>> GetAllByTypesOverTimePeriodAsync(IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo)
    {
        return await _context.Operations
            .Where(operation => types.Contains(operation.Type) && operation.Date >= dateFrom && operation.Date <= dateTo)
            .ToListAsync();
    }
}
