using ApiEngine.Core.Gen;
using ApiEngine.Core.Option;
using Furion;
using Furion.DataValidation;
using Furion.FriendlyException;
using Furion.JsonSerialization;
using Furion.Logging.Extensions;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace ApiEngine.Core.Provider;

[UnifyModel(typeof(RESTfulResult<>))]
public class ResultProvider(IOptionsMonitor<AppInfoOptions> options, IJsonSerializerProvider jsonSerializer)
    : IUnifyResultProvider
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
        var result = NewResult(StatusCodes.Status400BadRequest,
            data: metadata.Data,
            errors: GenConst.MessagePrefix + metadata.Message);
        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnAuthorizeException(DefaultHttpContext context, ExceptionMetadata metadata)
    {
        var exception = metadata.Exception;
        while (exception.InnerException != null) exception = exception.InnerException;

        var result = NewResult(metadata.StatusCode, data: metadata.Data,
            errors: GenConst.MessagePrefix + exception.Message);
        if (exception is not AppFriendlyException and not SqlSugarException)
            metadata.Exception.Message.LogError<ResultProvider>(metadata.Exception);

        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        var exception = metadata.Exception;
        while (exception.InnerException != null) exception = exception.InnerException;

        var result = NewResult(metadata.StatusCode, data: metadata.Data,
            errors: GenConst.MessagePrefix + exception.Message);
        if (exception is not AppFriendlyException and not SqlSugarException)
            metadata.Exception.Message.LogError<ResultProvider>(metadata.Exception);

        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public async Task OnResponseStatusCodes(HttpContext context, int statusCode,
        UnifyResultSettingsOptions unifyResultSettings = null)
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

        if (result.Succeeded && _options.Log.Response)
            jsonSerializer.Serialize(result).LogInformation<ResultProvider>();
        else jsonSerializer.Serialize(result).LogError<ResultProvider>();
    }

    private static RESTfulResult<object> NewResult(int statusCode, bool succeeded = default, object data = default,
        object errors = default)
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