using ApiEngine.Core.Option;
using Furion.JsonSerialization;
using Furion.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ApiEngine.Core.Aop;

public class AuditFilter(IOptionsMonitor<AppInfoOptions> options, IJsonSerializerProvider jsonSerializer)
    : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requestUrl = context.HttpContext.Request.GetRequestUrlAddress();
        var allowLog = !options.CurrentValue.Log.IgnoreKeys.Exists(requestUrl.Contains);

        if (context.ActionArguments.Count > 0 && options.CurrentValue.Log.Request && allowLog)
            jsonSerializer.Serialize(context.ActionArguments).LogInformation<AuditFilter>();

        return base.OnActionExecutionAsync(context, next);
    }
}