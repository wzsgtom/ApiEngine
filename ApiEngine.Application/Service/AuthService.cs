using ApiEngine.Application.Dto;
using ApiEngine.Application.Service.AuthServiceDto;
using Furion.DataEncryption;
using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEngine.Application.Service;

/// <summary>
///     身份认证服务
/// </summary>
[AllowAnonymous]
[ApiDescriptionSettings(Name = "auth", Version = "1")]
public class AuthService(IHttpContextAccessor accessor) : IDynamicApiController, IScoped
{
    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task Login(BaseDto<LoginDto> dto)
    {
        await Task.Delay(100);

        var accessToken = JWTEncryption.Encrypt(dto.User.ToDictionary());
        var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, 480);

        accessor.HttpContext.SetTokensOfResponseHeaders(accessToken, refreshToken);
    }

    /// <summary>
    ///     修改密码
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task ModifyPwd(BaseDto<ModifyPasswordDto> dto)
    {
        await Task.Delay(100);
    }
}