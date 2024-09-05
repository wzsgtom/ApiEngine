using ApiEngine.Core.Extension;
using ApiEngine.Core.Option;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiEngine.Core.Aop;

public class AuditAop(ILogger<AuditAop> logger, IOptionsMonitor<AppInfoOptions> options) : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requestUrl = context.HttpContext.Request.GetRequestUrlAddress();
        var allowLog = !options.CurrentValue.Log.IgnoreKeys.Exists(requestUrl.Contains);

        if (context.ActionArguments.Count > 0 && options.CurrentValue.Log.Request && allowLog)
            logger.LogInformation("{c}", context.ActionArguments.ToJson());

        return base.OnActionExecutionAsync(context, next);
    }
}