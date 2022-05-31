using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DataAccess.Repositories;

public class BudgetItemsRepository : IBudgetItemsRepository
{
    private readonly string _connectionString;

    public BudgetItemsRepository(IConfiguration config)
    {
        _connectionString = config["ConnectionStrings:PersonalBudgetConnection"];
    }

    public async Task<bool> EqualExistsAsync(OperationType type, string name, int id = 0)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstAsync<bool>(
            "SELECT * FROM budget_items_equalexists(@Type, @Id, @Name)",
            new { Type = (int)type, Id = id, Name = name });
    }

    public async Task<IBudgetItem?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<BudgetItem>(
            "SELECT * FROM budget_items_getbyid(@Id)",
            new { Id = id });
    }

    public async Task<IEnumerable<IBudgetItem>> GetAllWithPagingAsync(int pageNumber, int pageSize, IEnumerable<OperationType> types)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<BudgetItem>(
            "SELECT * FROM budget_items_getallwithpaging(@Types, @PageNumber, @PageSize)",
            new { Types = types.Select(t => (int)t).ToArray(), PageNumber = pageNumber, PageSize = pageSize });
    }

    public async Task<IBudgetItem?> PostAsync(IBudgetItem entity)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<BudgetItem>(
            "SELECT * FROM budget_items_post(@Name, @Type)",
            new { Name = entity.Name, Type = (int)entity.Type });
    }

    public async Task<IBudgetItem?> PutAsync(int id, IBudgetItem entity)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<BudgetItem>(
            "SELECT * FROM budget_items_put(@Id, @Name)",
            new { Id = id, Name = entity.Name });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstAsync<bool>(
            "SELECT * FROM budget_items_delete(@Id)",
            new { Id = id });
    }
}
