using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DataAccess.Repositories;

public class BudgetOperationsRepository : IBudgetOperationsRepository
{
    private readonly string _connectionString;

    public BudgetOperationsRepository(IConfiguration config)
    {
        _connectionString = config["ConnectionStrings:PersonalBudgetConnection"];
    }

    public async Task<IBudgetOperation?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return (await connection.QueryAsync<BudgetOperation, BudgetItem, BudgetOperation>(
            sql:
                "SELECT * FROM budget_operations_getById(@Id)",
            map:
                (operation, item) =>
                {
                    operation.Item = item;
                    return operation;
                },
            splitOn:
                "item_id",
            param:
                new { Id = id }
        )).FirstOrDefault();
    }

    public async Task<IEnumerable<IBudgetOperation>> GetAllOverTimePeriodWithPagingAsync(
        IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo, int pageNumber, int pageSize)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<BudgetOperation, BudgetItem, BudgetOperation>(
            sql:
                "SELECT * FROM budget_operations_getAllForPeriodExcludingEndWithPaging(@Types, @DateFrom, @DateTo, @PageNumber, @PageSize)",
            map:
                (operation, item) =>
                {
                    operation.Item = item;
                    return operation;
                },
            splitOn:
                "item_id",
            param:
                new { Types = types.Select(t => (int)t).ToArray(), DateFrom = dateFrom, DateTo = dateTo, PageNumber = pageNumber, PageSize = pageSize }
        );
    }

    public async Task<IBudgetOperation?> PostAsync(IBudgetOperation entity)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return (await connection.QueryAsync<BudgetOperation, BudgetItem, BudgetOperation>(
            sql:
                "SELECT * FROM budget_operations_post(@Date, @Type, @Sum, @ItemId)",
            map:
                (operation, item) =>
                {
                    operation.Item = item;
                    return operation;
                },
            splitOn:
                "item_id",
            param:
                new { Date = entity.Date, Type = (int)entity.Type, Sum = entity.Sum, ItemId = entity.Item?.Id }
        )).FirstOrDefault();
    }

    public async Task<IBudgetOperation?> PutAsync(int id, IBudgetOperation entity)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return (await connection.QueryAsync<BudgetOperation, BudgetItem, BudgetOperation>(
            sql:
                "SELECT * FROM budget_operations_put(@Id, @Date, @Sum, @ItemId)",
            map:
                (operation, item) =>
                {
                    operation.Item = item;
                    return operation;
                },
            splitOn:
                "item_id",
            param:
                new { Id = id, Date = entity.Date, Sum = entity.Sum, ItemId = entity.Item?.Id }
        )).FirstOrDefault();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstAsync<bool>(
            "SELECT * FROM budget_operations_delete(@Id)",
            new { Id = id });
    }
}
