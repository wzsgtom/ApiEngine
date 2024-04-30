namespace ApiEngine.Core;

public sealed class StartupServiceComponent : IServiceComponent
{
    public void Load(IServiceCollection services, ComponentContext componentContext)
    {
        // 配置
        services.AddConfigurableOptions<AppInfoOptions>();
        // 跨域
        services.AddCorsAccessor();
        // 限流
        SetRateLimit(services);
        // 健康检查
        services.AddHealthChecks();
        // 设置缓存
        SetCache(services);
        // 设置数据库
        SetSqlSugar(services);
        // 远程请求
        services.AddRemoteRequest();
        // JWT授权
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: App.GetOptions<AppInfoOptions>().GlobalAuthorize);
        // 审计
        services.AddMvcFilter<AuditFilter>();
        // 控制器.设置JSON.规范化结果
        services.AddControllers().AddNewtonsoftJson(SetJsonOptions).AddInjectWithUnifyResult<ResultProvider>();
        // 响应压缩
        SetResponseCompression(services);
        // 事件总线
        SetEventBus(services);
        // 任务队列
        SetTaskQueue(services);
        // 定时任务
        SetHangfire(services);
        // 日志
        SetLog(services);
    }

    #region ohh

    /// <summary>
    ///     设置Json序列化
    /// </summary>
    /// <param name="jsonOptions"></param>
    private static void SetJsonOptions(MvcNewtonsoftJsonOptions jsonOptions)
    {
        jsonOptions.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        jsonOptions.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        jsonOptions.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    }

    /// <summary>
    ///     设置限流
    /// </summary>
    private static void SetRateLimit(IServiceCollection services)
    {
        services.Configure<IpRateLimitOptions>(App.Configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }

    /// <summary>
    ///     缓存注册（新生命Redis组件）
    /// </summary>
    public static void SetCache(IServiceCollection services)
    {
        var cache = NewLife.Caching.Cache.Default;
        var cacheOptions = App.GetOptions<AppInfoOptions>().Cache;
        if (cacheOptions.CacheType == CacheTypeEnum.Redis)
        {
            cache = new FullRedis(new RedisOptions
            {
                Configuration = cacheOptions.Redis.Configuration,
                Prefix = cacheOptions.Redis.Prefix
            });
        }

        services.AddSingleton(cache);
    }

    /// <summary>
    ///     设置数据库连接
    /// </summary>
    private static void SetSqlSugar(IServiceCollection services)
    {
        services.AddSingleton<ISqlSugarClient>(DbContext.GetNew());
        services.AddScoped(typeof(DbRepository<>));
        services.AddUnitOfWork<DbUnitOfWork>();
    }

    /// <summary>
    ///     响应压缩
    /// </summary>
    /// <param name="services"></param>
    private static void SetResponseCompression(IServiceCollection services)
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
    private static void SetEventBus(IServiceCollection services)
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
    private static void SetHangfire(IServiceCollection services)
    {
        services.AddHangfire(cfg => cfg.UseMemoryStorage());
        services.AddHangfireServer();
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
    }

    /// <summary>
    ///     设置任务队列
    /// </summary>
    private static void SetTaskQueue(IServiceCollection services)
    {
        services.AddTaskQueue(opt => opt.UnobservedTaskExceptionHandler = EventHandles.OptionsUnobservedTaskExceptionHandler);
    }

    /// <summary>
    ///     设置日志
    /// </summary>
    private static void SetLog(IServiceCollection services)
    {
        services.AddLogDashboard(opt => opt.CustomLogModel<RequestTraceLogModel>());
    }

    #endregion
}