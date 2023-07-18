namespace ApiEngine.Core.Background;

public class LogJob : IJob
{
    private readonly AppInfoOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public LogJob(IServiceScopeFactory scopeFactory, IOptions<AppInfoOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    /// <summary>
    ///     清理数据库日志（仅数据库模式有用）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        if (_options.Log.LogType == LogTypeEnum.Db)
        {
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            using var db = (serviceProvider.GetService<ISqlSugarClient>() as SqlSugarScope)?.AsTenant().GetConnection("log");
            if (db == null)
            {
                return;
            }

            var keepDate = db.GetDate().AddMonths(_options.Log.LogDbSet.KeepMonths * -1);
            await db.Deleteable<LogMod>().Where(w => w.LongDate < keepDate).ExecuteCommandAsync(stoppingToken);
        }
    }
}