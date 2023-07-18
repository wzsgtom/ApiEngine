namespace ApiEngine.Core.Handlers;

public class EventHandles
{
    public static void OptionsUnobservedTaskExceptionHandler(object obj, UnobservedTaskExceptionEventArgs args)
    {
        args.SetObserved();
        $"观察到任务异常 => {args.Exception.Message}".LogError(args.Exception);
    }
}