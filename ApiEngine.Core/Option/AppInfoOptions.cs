using Furion.ConfigurableOptions;
using NewLife.Caching;

namespace ApiEngine.Core.Option;

public class AppInfoOptions : IConfigurableOptions
{
    /// <summary>
    ///     全局授权
    /// </summary>
    public bool GlobalAuthorize { get; set; }

    /// <summary>
    ///     缓存
    /// </summary>
    public CacheOptions Cache { get; set; } = new();

    /// <summary>
    ///     日志
    /// </summary>
    public LogOptions Log { get; set; } = new();

    public class CacheOptions
    {
        /// <summary>
        ///     Redis缓存选项
        /// </summary>
        public RedisOptions Redis { get; set; }
    }

    public class LogOptions
    {
        /// <summary>
        ///     请求日志
        /// </summary>
        public bool Request { get; set; }

        /// <summary>
        ///     响应日志
        /// </summary>
        public bool Response { get; set; }

        /// <summary>
        ///     忽略关键字
        /// </summary>
        public List<string> IgnoreKeys { get; set; } = [];
    }
}