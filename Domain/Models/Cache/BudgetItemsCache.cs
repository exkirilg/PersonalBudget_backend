using Domain.Interfaces.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace Domain.Models.Cache;

public class BudgetItemsCache : IBudgetItemsCache
{
    const long CacheSizeLimit = 100;

    private readonly MemoryCache _cache;

    public BudgetItemsCache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = CacheSizeLimit
        });
    }

    public IEnumerable<IBudgetItem> GetItemsCollection(OperationType? type = null)
    {
        _cache.TryGetValue(GetItemsCollectionCacheKey(type), out IEnumerable<IBudgetItem> items);
        return items;
    }
    public void SetItemsCollection(IEnumerable<IBudgetItem> items, OperationType? type = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSize(items.Count());
        _cache.Set(GetItemsCollectionCacheKey(type), items, cacheEntryOptions);
    }
    public void RemoveItemsCollection(OperationType? type = null)
    {
        _cache.Remove(GetItemsCollectionCacheKey(type));
    }

    public IBudgetItem GetItem(int id)
    {
        _cache.TryGetValue(GetItemCacheKey(id), out IBudgetItem item);
        return item;
    }
    public void SetItem(IBudgetItem item)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSize(1);
        _cache.Set(GetItemCacheKey(item.Id), item, cacheEntryOptions);
    }
    public void RemoveItem(int id)
    {
        _cache.Remove(GetItemCacheKey(id));
    }

    private string GetItemCacheKey(int id) => $"Item-{id}";
    private string GetItemsCollectionCacheKey(OperationType? type) => type is null ? "Items" : $"Items-{type}";
}
