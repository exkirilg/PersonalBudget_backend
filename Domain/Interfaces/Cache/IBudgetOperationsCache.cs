namespace Domain.Interfaces.Cache;

public interface IBudgetOperationsCache
{
    IEnumerable<IBudgetOperation> GetOperationsCollection(DateTime dateFrom, DateTime dateTo, OperationType? type = null);
    void SetOperationsCollection(IEnumerable<IBudgetOperation> operations, DateTime dateFrom, DateTime dateTo, OperationType? type = null);

    IBudgetOperation GetOperation(int id);
    void SetOperation(IBudgetOperation operation);
    void RemoveOperation(int id);
}
