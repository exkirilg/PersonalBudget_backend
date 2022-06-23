namespace Domain.Interfaces.Cache;

public interface IBudgetItemsCache
{
    IEnumerable<IBudgetItem> GetItemsCollection(OperationType? type = null);
    void SetItemsCollection(IEnumerable<IBudgetItem> items, OperationType? type = null);
    void RemoveItemsCollection(OperationType? type = null);

    IBudgetItem GetItem(int id);
    void SetItem(IBudgetItem item);
    void RemoveItem(int id);
}
