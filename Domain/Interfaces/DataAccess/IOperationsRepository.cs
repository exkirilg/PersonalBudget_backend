namespace Domain.Interfaces.DataAccess;

public interface IOperationsRepository : IGenericRepository<Operation>
{
    public Task<IEnumerable<Operation>> GetAllByTypesOverTimePeriodAsync(
        string userId, bool userIsAdmin, IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo);
}
