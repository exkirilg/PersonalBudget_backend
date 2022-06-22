namespace Domain.Interfaces.DataAccess;

public interface IBudgetOperationsRepository : IGenericRepository<IBudgetOperation>
{
    public Task<IEnumerable<IBudgetOperation>> GetAllOverTimePeriodAsync(IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo);
}
