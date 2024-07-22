namespace ApiEngine.Core.Job;

public interface ILogJob : ITransient
{
    Task RunJob();
}

public class LogJob(IServiceScopeFactory scopeFactory, IOptions<AppInfoOptions> options, ISqlSugarClient db) : ILogJob
{
    private readonly AppInfoOptions _options = options.Value;

    public async Task RunJob()
    {
        switch (_options.Log.LogType)
        {
            case LogTypeEnum.Seq:
                break;
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
            if (!Directory.Exists(dir)) return;

            var now = DateTime.Now;
            foreach (var f in Directory.GetFileSystemEntries(dir).Where(File.Exists))
            {
                var t = File.GetCreationTime(f);
                var elapsedTicks = now.Ticks - t.Ticks;
                var elaspsedSpan = new TimeSpan(elapsedTicks);
                if (elaspsedSpan.TotalDays > retainDays) File.Delete(f);
            }
        }
        catch (Exception ex)
        {
            ex.Message.LogError<LogJob>(ex);
        }
    }
}