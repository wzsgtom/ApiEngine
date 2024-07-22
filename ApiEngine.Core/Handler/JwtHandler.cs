using Furion.Authorization;
using Furion.DataEncryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ApiEngine.Core.Handler;

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
    public override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        return await CheckAuthorizeAsync(context, httpContext);
    }

    /// <summary>
    ///     权限校验核心逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private static async Task<bool> CheckAuthorizeAsync(AuthorizationHandlerContext context,
        DefaultHttpContext httpContext)
    {
        //foreach (var requirement in context.Requirements)
        //{
        //    if (requirement is RolesAuthorizationRequirement roles)
        //    {
        //        var role = AppFunc.Token("role");
        //        if (role.IsNullOrEmpty() || !roles.AllowedRoles.Contains(role))
        //        {
        //            return await Task.FromResult(false);
        //        }
        //    }
        //}

        return await Task.FromResult(true);
    }
}