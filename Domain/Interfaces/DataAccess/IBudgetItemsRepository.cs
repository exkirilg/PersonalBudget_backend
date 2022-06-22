namespace Domain.Interfaces.DataAccess;

public interface IBudgetItemsRepository : IGenericRepository<IBudgetItem>
{
    public Task<bool> EqualExistsAsync(OperationType type, string name, int id = 0);
    public Task<IEnumerable<IBudgetItem>> GetAllAsync(IEnumerable<OperationType> types);
}
