namespace Domain.Interfaces.DataAccess;

public interface IBudgetOperationsRepository : IGenericRepository<IBudgetOperation>
{
    public Task<IEnumerable<IBudgetOperation>> GetAllOverTimePeriodWithPagingAsync(IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo, int pageNumber, int pageSize);
}
