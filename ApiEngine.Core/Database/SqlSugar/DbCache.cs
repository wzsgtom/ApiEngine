using Furion;
using NewLife.Caching;
using SqlSugar;

namespace ApiEngine.Core.Database.SqlSugar;

/// <summary>
///     SqlSugar二级缓存
/// </summary>
public class DbCache : ICacheService
{
    private readonly Lazy<ICache> _lazy = new(() => App.GetService<ICache>() ?? NewLife.Caching.Cache.Default);
    private ICache Cache => _lazy.Value;

    public void Add<T>(string key, T value)
    {
        Cache.Set(key, value);
    }

    public void Add<T>(string key, T value, int cacheDurationInSeconds)
    {
        Cache.Set(key, value, cacheDurationInSeconds);
    }

    public bool ContainsKey<T>(string key)
    {
        return Cache.ContainsKey(key);
    }

    public T Get<T>(string key)
    {
        return Cache.Get<T>(key);
    }

    public IEnumerable<string> GetAllKey<T>()
    {
        return Cache.Keys;
    }

    public T GetOrCreate<T>(string cacheKey, Func<T> create, int cacheDurationInSeconds = 28800)
    {
        if (Cache.TryGetValue(cacheKey, out T value)) return value;

        value = create();
        Cache.Set(cacheKey, value, cacheDurationInSeconds);
        return value;
    }

    public void Remove<T>(string key)
    {
        Cache.Remove(key);
    }
}