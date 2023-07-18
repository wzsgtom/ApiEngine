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
        // 定时任务
        SetSchedule(services);
        // 任务队列
        SetTaskQueue(services);
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
    ///     设置数据库连接
    /// </summary>
    private static void SetSqlSugar(IServiceCollection services)
    {
        var sugar = new SqlSugarScope(new List<ConnectionConfig>(App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs")), db =>
        {
            db.CurrentConnectionConfig.IsAutoCloseConnection = true;
            db.CurrentConnectionConfig.LanguageType = LanguageType.Chinese;
#if DEBUG
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                var str = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                str.LogInformation();
            };
            db.Aop.OnError = ex =>
            {
                var str = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, ex.Sql, (SugarParameter[])ex.Parametres);
                str.LogError();
            };
#endif
        });

        services.AddSingleton<ISqlSugarClient>(sugar);
        services.AddScoped(typeof(Repository<>));
    }

    /// <summary>
    ///     设置后台任务
    /// </summary>
    private static void SetSchedule(IServiceCollection services)
    {
        var appInfo = App.GetOptions<AppInfoOptions>();
        if (appInfo.Log.LogType == LogTypeEnum.Db)
        {
            services.AddSchedule(options =>
            {
                options.LogEnabled = true;
                options.UnobservedTaskExceptionHandler = EventHandles.OptionsUnobservedTaskExceptionHandler;

                options.AddJob<LogJob>(nameof(LogJob), Triggers.Monthly().SetStartNow(true));
            });
        }
    }

    /// <summary>
    ///     设置任务队列
    /// </summary>
    private static void SetTaskQueue(IServiceCollection services)
    {
        services.AddTaskQueue(options => options.UnobservedTaskExceptionHandler = EventHandles.OptionsUnobservedTaskExceptionHandler);
    }

    /// <summary>
    ///     设置日志
    /// </summary>
    private static void SetLog(IServiceCollection services)
    {
        var appInfo = App.GetOptions<AppInfoOptions>();
        switch (appInfo.Log.LogType)
        {
            case LogTypeEnum.Db:
                var dbLog = App.GetService<ISqlSugarClient>().AsTenant().GetConnection("log");
                dbLog.DbMaintenance.CreateDatabase();
                dbLog.CodeFirst.As<LogMod>(appInfo.Log.LogDbSet.TableName).InitTables<LogMod>();

                LogManager.Setup().LoadConfigurationFromFile("nlog-db.config");
                LogManager.Configuration.Variables["ConnectionString"] = dbLog.CurrentConnectionConfig.ConnectionString;
                LogManager.Configuration.Variables["TableName"] = appInfo.Log.LogDbSet.TableName;

                services.AddLogDashboard(opt =>
                {
                    opt.UseDataBase(() => new SqlConnection(dbLog.CurrentConnectionConfig.ConnectionString), appInfo.Log.LogDbSet.TableName);
                    opt.CustomLogModel<RequestTraceLogModel>();
                });
                break;
            case LogTypeEnum.File:
            default:
                LogManager.Setup().LoadConfigurationFromFile("nlog-file.config");
                services.AddLogDashboard(opt => opt.CustomLogModel<RequestTraceLogModel>());
                break;
        }
    }

    #endregion
}