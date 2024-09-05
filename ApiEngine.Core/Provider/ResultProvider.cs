using ApiEngine.Core.Extension;
using ApiEngine.Core.Gen;
using ApiEngine.Core.Option;
using Furion;
using Furion.DataValidation;
using Furion.FriendlyException;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace ApiEngine.Core.Provider;

[UnifyModel(typeof(RESTfulResult<>))]
public class ResultProvider(ILogger<ResultProvider> logger, IOptionsMonitor<AppInfoOptions> options) : IUnifyResultProvider
{
    private readonly AppInfoOptions _options = options.CurrentValue;

    public IActionResult OnSucceeded(ActionExecutedContext context, object data)
    {
        var result = NewResult(StatusCodes.Status200OK, true, data);
        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
    {
        //var errs = context.ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage);
        var result = NewResult(StatusCodes.Status400BadRequest, data: metadata.Data, errors: GenConst.MessagePrefix + metadata.Message);
        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnAuthorizeException(DefaultHttpContext context, ExceptionMetadata metadata)
    {
        var exception = metadata.Exception.GetTrue();

        var result = NewResult(metadata.StatusCode, data: metadata.Data, errors: GenConst.MessagePrefix + exception.Message);
        if (exception is not AppFriendlyException and not SqlSugarException)
            logger.LogError(metadata.Exception, "{c}", metadata.Exception.Message);

        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        var exception = metadata.Exception.GetTrue();

        var result = NewResult(metadata.StatusCode, data: metadata.Data, errors: GenConst.MessagePrefix + exception.Message);
        if (exception is not AppFriendlyException and not SqlSugarException)
            logger.LogError(metadata.Exception, "{c}", metadata.Exception.Message);

        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings = null)
    {
        UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);
        switch (statusCode)
        {
            case StatusCodes.Status401Unauthorized:
            {
                await context.Response.WriteAsJsonAsync(
                    NewResult(statusCode, errors: GenConst.MessagePrefix + "401 登录已过期，请重新登录"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            }
            case StatusCodes.Status403Forbidden:
            {
                await context.Response.WriteAsJsonAsync(
                    NewResult(statusCode, errors: GenConst.MessagePrefix + "403 禁止访问，没有权限"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            }
        }
    }

    private void Log(ActionContext context, RESTfulResult<object> result)
    {
        var requestUrl = context.HttpContext.Request.GetRequestUrlAddress();
        var allowLog = !_options.Log.IgnoreKeys.Exists(requestUrl.Contains);

        if (!allowLog) return;

        if (result.Succeeded && _options.Log.Response) logger.LogInformation("{c}", result.ToJson());
        else logger.LogError("{c}", result.ToJson());
    }

    private static RESTfulResult<object> NewResult(int statusCode, bool succeeded = false, object data = null, object errors = null)
    {
        var result = new RESTfulResult<object>
        {
            StatusCode = statusCode,
            Succeeded = succeeded,
            Data = data,
            Extras = UnifyContext.Take(),
            Errors = errors,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        return result;
    }
}