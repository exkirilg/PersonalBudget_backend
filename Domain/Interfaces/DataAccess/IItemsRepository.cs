namespace Domain.Interfaces.DataAccess;

public interface IItemsRepository : IGenericRepository<Item>
{
    public Task<bool> EqualExistsAsync(OperationType type, string name, int id = 0);
    public Task<IEnumerable<Item>> GetAllByTypesAsync(IEnumerable<OperationType> types);
}
