namespace ApiEngine.Core;

public class StartupWebComponent : IWebComponent
{
    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        builder.Configuration.Get<WebHostBuilder>().ConfigureKestrel(u =>
        {
            u.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            u.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            u.Limits.MaxRequestBodySize = null;
        });

        builder.Host.UseSerilog((context, logger) =>
        {
            logger.MinimumLevel.Information()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("ApiEngine", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(config =>
                {
                    config.Console();
                    config.File(AppContext.BaseDirectory + "logs/.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 60,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}||{Level}||{SourceContext:l}||{Message}||{Exception}||{RequestId}||end{NewLine}");
                });
        });
    }
}