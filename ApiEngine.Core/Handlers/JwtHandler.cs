namespace ApiEngine.Core.Handlers;

public class JwtHandler : AppAuthorizeHandler
{
    /// <summary>
    ///     重写 Handler 添加自动刷新收取逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public override async Task HandleAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        if (JWTEncryption.AutoRefreshToken(context, context.GetCurrentHttpContext(), refreshTokenExpiredTime: 480))
        {
            await AuthorizeHandleAsync(context);
        }
        else
        {
            context.Fail();
            context.GetCurrentHttpContext()?.SignoutToSwagger();
        }
    }

    /// <summary>
    ///     请求管道
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public override Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        return Task.FromResult(true);
    }
}