namespace ApiEngine.Application.Services;

/// <summary>
///     身份认证服务
/// </summary>
[AllowAnonymous]
public class AuthService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _accessor;

    public AuthService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    public Task Login(LoginDto mod)
    {
        // TODO 验证登录用户

        // 生成Token令牌
        var accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            { "account", mod.account },
            { "name", mod.name }
        });

        // 生成刷新Token令牌
        var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, 480);

        // 设置Swagger自动登录
        _accessor.HttpContext.SigninToSwagger(accessToken);

        // 设置响应报文头
        _accessor.HttpContext!.Response.Headers["access-token"] = accessToken;
        _accessor.HttpContext!.Response.Headers["x-access-token"] = refreshToken;

        return Task.CompletedTask;
    }
}