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
/// Clés de cache - utilise un hash pour éviter les clés trop longues
/// </summary>
public static class CacheKeys
{
    public const string NoeudsDistincts = "noeuds_distincts";

    public static string LignageRow(string lnaUid, string linUid, string edgDir)
        => $"row_{Hash(lnaUid, linUid, edgDir)}";

    public static string Successeurs(string lnaUid, string linUid, string edgDir)
        => $"succ_{Hash(lnaUid, linUid, edgDir)}";

    public static string Predecesseurs(string lnaUid, string linUid, string edgDir)
        => $"pred_{Hash(lnaUid, linUid, edgDir)}";

    public static string Programme(string lnaUid)
        => $"prog_{lnaUid.GetHashCode():X8}";

    private static string Hash(string lnaUid, string linUid, string edgDir)
        => $"{lnaUid.GetHashCode():X8}_{linUid.GetHashCode():X8}_{edgDir}";
}
