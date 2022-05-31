namespace Domain.Interfaces.DataAccess;

public interface IBudgetOperationsRepository : IGenericRepository<BudgetOperation>
{
    public Task<IEnumerable<BudgetOperation>> GetAllOverTimePeriodWithPagingAsync(IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo, int pageNumber, int pageSize);
}
