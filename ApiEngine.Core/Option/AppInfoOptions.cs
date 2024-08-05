using Furion.ConfigurableOptions;
using NewLife.Caching;
using System.ComponentModel;

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
        ///     缓存前缀
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///     缓存类型
        /// </summary>
        public CacheTypeEnum CacheType { get; set; } = CacheTypeEnum.Memory;

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

/// <summary>
///     缓存类型枚举
/// </summary>
[Description("缓存类型枚举")]
public enum CacheTypeEnum
{
    [Description("内存缓存")] Memory,

    [Description("Redis缓存")] Redis
}