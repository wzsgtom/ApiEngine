namespace ApiEngine.Core;

public static class Startup
{
    public static RunOptions InjectCore(this RunOptions runOptions)
    {
        return runOptions.AddWebComponent<StartupWebComponent>()
            .AddComponent<StartupServiceComponent>()
            .UseComponent<StartupApplicationComponent>();
    }
}