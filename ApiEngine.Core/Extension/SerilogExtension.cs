using ApiEngine.Core.Option;
using Furion;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ApiEngine.Core.Extension;

public static class SerilogExtension
{
    public static void UseOwnSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, logger) =>
        {
            logger.MinimumLevel.Information()
                .MinimumLevel.Override("Hangfire.Storage.SQLite", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("ApiEngine", LogEventLevel.Verbose)
                .Enrich.FromLogContext()
                .WriteTo.Async(config =>
                {
                    config.Console();

                    var log = App.GetOptions<AppInfoOptions>().Log;
                    switch (log.LogType)
                    {
                        case LogTypeEnum.Seq:
                            config.Seq(log.LogSeqSet.ServerUrl, apiKey: log.LogSeqSet.ApiKey,
                                eventBodyLimitBytes: 10485760);
                            break;
                        case LogTypeEnum.File:
                        default:
                            config.File(AppContext.BaseDirectory + "logs/.log",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 90,
                                outputTemplate:
                                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}||{Level}||{SourceContext:l}||{Message}||{Exception}||{RequestId}||end{NewLine}");
                            break;
                    }
                });
        });
    }
}