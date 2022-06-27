namespace Domain.Interfaces.Cache;

public interface IOperationsCache
{
    IEnumerable<Operation> GetOperationsCollection(DateTime dateFrom, DateTime dateTo, OperationType? type = null);
    void SetOperationsCollection(IEnumerable<Operation> operations, DateTime dateFrom, DateTime dateTo, OperationType? type = null);

    Operation GetOperation(int id);
    void SetOperation(Operation operation);
    void RemoveOperation(int id);
}
