using Domain.Interfaces.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace Domain.Models.Cache;

public class OperationsCache: IOperationsCache
{
    const long CacheSizeLimit = 100;

    private readonly MemoryCache _cache;

    public OperationsCache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = CacheSizeLimit
        });
    }

    public IEnumerable<Operation> GetOperationsCollection(DateTime dateFrom, DateTime dateTo, OperationType? type = null)
    {
        _cache.TryGetValue(GetOperationsCollectionCacheKey(dateFrom, dateTo, type), out IEnumerable<Operation> operations);
        return operations;
    }
    public void SetOperationsCollection(IEnumerable<Operation> operations, DateTime dateFrom, DateTime dateTo, OperationType? type = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(new TimeSpan(hours: 0, minutes: 0, seconds: 1))
            .SetSize(operations.Count());
        _cache.Set(GetOperationsCollectionCacheKey(dateFrom, dateTo, type), operations, cacheEntryOptions);
    }

    public Operation GetOperation(int id)
    {
        _cache.TryGetValue(GetOperationCacheKey(id), out Operation operation);
        return operation;
    }
    public void SetOperation(Operation operation)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSize(1);
        _cache.Set(GetOperationCacheKey(operation.Id), operation, cacheEntryOptions);
    }
    public void RemoveOperation(int id)
    {
        _cache.Remove(GetOperationCacheKey(id));
    }

    private string GetOperationCacheKey(int id) => $"Operation-{id}";
    private string GetOperationsCollectionCacheKey(DateTime dateFrom, DateTime dateTo, OperationType? type) => type is null ?
        $"Operations-{dateFrom}-{dateTo}" : $"Operations-{type}-{dateFrom}-{dateTo}";
}
