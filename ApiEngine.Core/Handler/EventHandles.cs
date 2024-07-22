using Furion.Logging.Extensions;

namespace ApiEngine.Core.Handler;

public class EventHandles
{
    public static void OptionsUnobservedTaskExceptionHandler(object obj, UnobservedTaskExceptionEventArgs args)
    {
        $"观察到任务异常 —> {args.Exception.Message}".LogError(args.Exception);
        args.SetObserved();
    }
}