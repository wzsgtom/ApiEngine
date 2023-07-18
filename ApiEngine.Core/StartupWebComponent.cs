namespace ApiEngine.Core;

public class StartupWebComponent : IWebComponent
{
    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        builder.Logging.AddConsoleFormatter();
        builder.Host.UseNLog();
    }
}