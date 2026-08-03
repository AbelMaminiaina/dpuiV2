using Microsoft.Extensions.Caching.Memory;

namespace LignageApp.Services;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    bool TryGet<T>(string key, out T? value);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _defaultOptions;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
        _defaultOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.Normal
        };
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = expiration.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
            : _defaultOptions;

        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public bool TryGet<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }
}

/// <summary>
/// Cache keys - uses hash to avoid overly long keys
/// </summary>
public static class CacheKeys
{
    public const string DistinctNodes = "distinct_nodes";

    public static string LignageRow(string lanUid, string linUid, string edgDir)
        => $"row_{Hash(lanUid, linUid, edgDir)}";

    public static string Successors(string lanUid, string linUid, string edgDir)
        => $"succ_{Hash(lanUid, linUid, edgDir)}";

    public static string Predecessors(string lanUid, string linUid, string edgDir)
        => $"pred_{Hash(lanUid, linUid, edgDir)}";

    public static string Program(string lanUid)
        => $"prog_{lanUid.GetHashCode():X8}";

    private static string Hash(string lanUid, string linUid, string edgDir)
        => $"{lanUid.GetHashCode():X8}_{linUid.GetHashCode():X8}_{edgDir}";
}
