namespace Domain.Interfaces.Cache;

public interface IItemsCache
{
    IEnumerable<Item>? GetItemsCollection(OperationType? type = null);
    void SetItemsCollection(IEnumerable<Item> items, OperationType? type = null);
    void RemoveItemsCollection(OperationType? type = null);

    Item? GetItem(int id);
    void SetItem(Item item);
    void RemoveItem(int id);
}
