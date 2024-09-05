using System.IO.Compression;
using ApiEngine.Core.Database.SqlSugar;
using ApiEngine.Core.Handler;
using ApiEngine.Core.Option;
using AspNetCoreRateLimit;
using Furion;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using NewLife.Caching;
using NewLife.Log;
using SqlSugar;

namespace ApiEngine.Core.Extension;

public static class ServiceExtension
{
    /// <summary>
    ///     设置限流
    /// </summary>
    internal static void SetRateLimit(this IServiceCollection services)
    {
        services.Configure<IpRateLimitOptions>(App.Configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }

    /// <summary>
    ///     缓存注册（新生命Redis组件）
    /// </summary>
    internal static void SetCache(this IServiceCollection services)
    {
        var cacheOptions = App.GetOptions<AppInfoOptions>().Cache;
        if (cacheOptions.Redis is not null)
        {
            var rds = new FullRedis(cacheOptions.Redis);
#if DEBUG
            XTrace.UseConsole();
            rds.Log = XTrace.Log;
            rds.ClientLog = XTrace.Log; // 调试日志，正式使用时注释
#endif
            Cache.Default = rds;
        }

        services.AddSingleton(Cache.Default);
    }

    /// <summary>
    ///     设置数据库连接
    /// </summary>
    internal static void SetSqlSugar(this IServiceCollection services)
    {
        var db = DbContext.GetNew();
        services.AddSingleton<ISqlSugarClient>(db);
        services.AddScoped(typeof(DbRepository<>));
    }

    /// <summary>
    ///     响应压缩
    /// </summary>
    /// <param name="services"></param>
    internal static void SetResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(opt =>
        {
            // 可以添加多种压缩类型，程序会根据级别自动获取最优方式
            opt.Providers.Add<GzipCompressionProvider>();
            opt.Providers.Add<BrotliCompressionProvider>();
            // 针对指定的 MimeTypes 使用压缩策略
            opt.MimeTypes = ResponseCompressionDefaults.MimeTypes;
            opt.ExcludedMimeTypes = ["text/html"];
            opt.EnableForHttps = true;
        });

        // 针对不同的压缩类型，设置对应的压缩级别
        services.Configure<GzipCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);
        services.Configure<BrotliCompressionProviderOptions>(opt => opt.Level = CompressionLevel.Fastest);
    }

    /// <summary>
    ///     设置事件总线
    /// </summary>
    /// <param name="services"></param>
    internal static void SetEventBus(this IServiceCollection services)
    {
        services.AddEventBus(opt =>
        {
            opt.LogEnabled = true;
            opt.UnobservedTaskExceptionHandler = EventHandles.OptionsUnobservedTaskExceptionHandler;
        });
    }

    /// <summary>
    ///     设置后台任务
    /// </summary>
    internal static void SetHangfire(this IServiceCollection services)
    {
        services.AddHangfire(config =>
            config.UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UseSQLiteStorage());
        services.AddHangfireServer();
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
    }

    /// <summary>
    ///     设置任务队列
    /// </summary>
    internal static void SetTaskQueue(this IServiceCollection services)
    {
        services.AddTaskQueue(opt =>
            opt.UnobservedTaskExceptionHandler = EventHandles.OptionsUnobservedTaskExceptionHandler);
    }
}