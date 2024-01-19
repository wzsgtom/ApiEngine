namespace ApiEngine.Core;

public class Startup
{
    public static RunOptions InjectCore(RunOptions runOptions)
    {
        return runOptions.AddWebComponent<StartupWebComponent>()
            .AddComponent<StartupServiceComponent>()
            .UseComponent<StartupApplicationComponent>();
    }
}