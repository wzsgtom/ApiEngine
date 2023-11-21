namespace ApiEngine.Core.Background;

public class LogJob(IServiceScopeFactory scopeFactory, IOptions<AppInfoOptions> options) : IJob
{
    private readonly AppInfoOptions _options = options.Value;

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        switch (_options.Log.LogType)
        {
            case LogTypeEnum.Db:
            {
                using var scope = scopeFactory.CreateScope();
                using var db = (scope.ServiceProvider.GetService<ISqlSugarClient>() as SqlSugarScope)?.AsTenant().GetConnection("log");
                if (db != null)
                {
                    var keepDate = db.GetDate().AddDays(_options.Log.RetainDays * -1);
                    await db.Deleteable<LogMod>().Where(w => w.LongDate < keepDate).ExecuteCommandAsync(stoppingToken);
                }

                break;
            }
            case LogTypeEnum.File:
            default:
                DeleteFileLogs(Path.Combine(AppContext.BaseDirectory, "logs"), _options.Log.RetainDays);
                break;
        }
    }

    private static void DeleteFileLogs(string dir, int retainDays)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            var now = DateTime.Now;
            foreach (var f in Directory.GetFileSystemEntries(dir).Where(File.Exists))
            {
                var t = File.GetCreationTime(f);
                var elapsedTicks = now.Ticks - t.Ticks;
                var elaspsedSpan = new TimeSpan(elapsedTicks);
                if (elaspsedSpan.TotalDays > retainDays)
                {
                    File.Delete(f);
                }
            }
        }
        catch (Exception ex)
        {
            ex.Message.LogError<LogJob>(ex);
        }
    }
}