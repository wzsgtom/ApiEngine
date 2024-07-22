namespace ApiEngine.Application.Services;

/// <summary>
///     系统缓存服务
/// </summary>
public class SysCacheService(ICache cache, IOptions<AppInfoOptions> appOptions) : IDynamicApiController, ISingleton
{
    private readonly AppInfoOptions.CacheOptions _cacheOptions = appOptions.Value.Cache;

    /// <summary>
    ///     获取缓存键名集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetKeys()
    {
        // 键名去掉全局缓存前缀
        return cache.Keys.Where(u => u.StartsWith(_cacheOptions.Prefix)).Select(u => u[_cacheOptions.Prefix.Length..])
            .OrderBy(u => u);
    }

    /// <summary>
    ///     增加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public bool Set(string key, object value)
    {
        return cache.Set($"{_cacheOptions.Prefix}{key}", value);
    }

    /// <summary>
    ///     增加缓存并设置过期时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    /// <returns></returns>
    [NonAction]
    public bool Set(string key, object value, TimeSpan expire)
    {
        return cache.Set($"{_cacheOptions.Prefix}{key}", value, expire);
    }

    /// <summary>
    ///     获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    [NonAction]
    public T Get<T>(string key)
    {
        return cache.Get<T>($"{_cacheOptions.Prefix}{key}");
    }

    /// <summary>
    ///     检查缓存是否存在
    /// </summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    [NonAction]
    public bool ExistKey(string key)
    {
        return cache.ContainsKey($"{_cacheOptions.Prefix}{key}");
    }

    /// <summary>
    ///     删除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    public int Remove(string key)
    {
        return cache.Remove($"{_cacheOptions.Prefix}{key}");
    }

    /// <summary>
    ///     根据键名前缀删除缓存
    /// </summary>
    /// <param name="prefixKey">键名前缀</param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteByPreKey")]
    [HttpPost]
    public int RemoveByPrefixKey(string prefixKey)
    {
        var delKeys = cache.Keys.Where(u => u.StartsWith(prefixKey)).ToArray();
        return delKeys.Length == 0 ? 0 : cache.Remove(delKeys);
    }

    /// <summary>
    ///     根据键名前缀获取键名集合
    /// </summary>
    /// <param name="prefixKey">键名前缀</param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "GetKeysByPrefixKey")]
    public IEnumerable<string> GetKeysByPrefixKey(string prefixKey)
    {
        return cache.Keys.Where(u => u.StartsWith(prefixKey));
    }

    /// <summary>
    ///     获取缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetValue(string key)
    {
        return cache == Cache.Default
            ? cache.Get<object>($"{_cacheOptions.Prefix}{key}")
            : cache.Get<string>($"{_cacheOptions.Prefix}{key}");
    }

    /// <summary>
    ///     获取或添加缓存，在数据不存在时执行委托请求数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    /// <param name="expire">过期时间，单位秒</param>
    /// <returns></returns>
    [NonAction]
    public T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
    {
        return cache.GetOrAdd($"{_cacheOptions.Prefix}{key}", callback, expire);
    }
}