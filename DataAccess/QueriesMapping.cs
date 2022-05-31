using Dapper;
using static Dapper.SqlMapper;
using System.Reflection;

namespace DataAccess;

public static class QueriesMapping
{
    private static readonly Dictionary<string, string> budgetItemsColumnsMaps = new()
    {
        { "item_id", "Id" },
        { "item_name", "Name" },
        { "item_type", "Type" }
    };
    private static readonly Dictionary<string, string> budgetOperationsColumnsMaps = new()
    {
        { "operation_id", "Id" },
        { "operation_date", "Date" },
        { "operation_type", "Type" },
        { "operation_sum", "Sum" }
    };

    public static void ConfigureQueriesMapping()
    {
        Func<Type, string, PropertyInfo> budgetItemMapper = MapBudgetItemProperty;
        Func<Type, string, PropertyInfo> budgetOperationMapper = MapBudgetOperationProperty;

        var budgetItemsMap = new CustomPropertyTypeMap(typeof(BudgetItem), budgetItemMapper);
        SetTypeMap(typeof(BudgetItem), budgetItemsMap);

        var budgetOperationsMap = new CustomPropertyTypeMap(typeof(BudgetOperation), budgetOperationMapper);
        SetTypeMap(typeof(BudgetOperation), budgetOperationsMap);
    }

    private static PropertyInfo MapBudgetItemProperty(Type type, string columnName)
    {
        if (budgetItemsColumnsMaps.ContainsKey(columnName))
            return type.GetProperty(budgetItemsColumnsMaps[columnName]);
        else
            return type.GetProperty(columnName);
    }
    private static PropertyInfo MapBudgetOperationProperty(Type type, string columnName)
    {
        if (budgetOperationsColumnsMaps.ContainsKey(columnName))
            return type.GetProperty(budgetOperationsColumnsMaps[columnName]);
        else
            return type.GetProperty(columnName);
    }
}
