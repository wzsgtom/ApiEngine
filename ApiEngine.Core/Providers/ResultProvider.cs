namespace ApiEngine.Core.Providers;

[UnifyModel(typeof(RESTfulResult<>))]
public class ResultProvider(IOptionsMonitor<AppInfoOptions> options) : IUnifyResultProvider
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
        var result = NewResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.FirstErrorMessage);
        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        var exception = context.Exception;
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
        }

        var result = NewResult(metadata.StatusCode, data: metadata.Data, errors: exception.Message);
        if (exception is not AppFriendlyException && exception is not SqlSugarException)
        {
            context.Exception.Message.LogError<ResultProvider>(context.Exception);
        }

        Log(context, result);
        return new JsonResult(result, UnifyContext.GetSerializerSettings(context));
    }

    public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings = null)
    {
        // 设置响应状态码
        UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

        switch (statusCode)
        {
            // 处理 401 状态码
            case StatusCodes.Status401Unauthorized:
            {
                await context.Response.WriteAsJsonAsync(NewResult(statusCode, errors: "401 Unauthorized")
                    , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            }
            // 处理 403 状态码
            case StatusCodes.Status403Forbidden:
                await context.Response.WriteAsJsonAsync(NewResult(statusCode, errors: "403 Forbidden")
                    , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
        }
    }

    private void Log(ActionContext context, RESTfulResult<object> result)
    {
        var requestUrl = context.HttpContext.Request.GetRequestUrlAddress();
        var allowLog = !_options.Log.IgnoreKeys.Exists(e => requestUrl.Contains(e));

        if (allowLog)
        {
            if (result.Succeeded && _options.Log.Response)
            {
                result.ToJson().LogInformation<ResultProvider>();
            }
            else
            {
                result.ToJson().LogError<ResultProvider>();
            }
        }
    }

    private static RESTfulResult<object> NewResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
    {
        var result = new RESTfulResult<object>
        {
            StatusCode = statusCode,
            Data = data,
            Succeeded = succeeded,
            Errors = errors,
            Extras = UnifyContext.Take(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        return result;
    }
}