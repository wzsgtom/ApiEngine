namespace ApiEngine.Core.Aop;

public class AuditFilter(IOptionsMonitor<AppInfoOptions> options) : ActionFilterAttribute
{
    private readonly AppInfoOptions _options = options.CurrentValue;

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requestUrl = context.HttpContext.Request.GetRequestUrlAddress();
        var allowLog = !_options.Log.IgnoreKeys.Exists(requestUrl.Contains);

        if (context.ActionArguments.Count > 0 && _options.Log.Request && allowLog)
            context.ActionArguments.ToJson().LogInformation<AuditFilter>();

        return base.OnActionExecutionAsync(context, next);
    }
}