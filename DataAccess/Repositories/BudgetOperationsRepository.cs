using Microsoft.Extensions.Configuration;

namespace DataAccess.Repositories;

public class BudgetOperationsRepository : IBudgetOperationsRepository
{
    private readonly string _connectionString;

    public BudgetOperationsRepository(IConfiguration config)
    {
        _connectionString = config["ConnectionStrings:PersonalBudgetConnection"];
    }

    public Task<BudgetOperation?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BudgetOperation>> GetAllOverTimePeriodWithPagingAsync(
        IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<BudgetOperation?> PostAsync(BudgetOperation entity)
    {
        throw new NotImplementedException();
    }

    public Task<BudgetOperation?> PutAsync(int id, BudgetOperation entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
