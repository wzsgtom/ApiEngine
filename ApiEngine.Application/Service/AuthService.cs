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
    #region 私有方法

    /// <summary>
    ///     验证用户
    /// </summary>
    /// <param name="code"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    [NonAction]
    private async Task AthensUser(string code, string pwd)
    {
        await Task.Delay(100);
    }

    #endregion

    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task Login(BaseDto<LoginDto> dto)
    {
        await AthensUser(dto.User.Ucode, dto.Data.Pwd);

        // 生成Token令牌
        var dict = dto.User.ToDictionary();

        var accessToken = JWTEncryption.Encrypt(dict);

        // 生成刷新Token令牌
        var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, 480);

        // 设置Swagger自动登录
        accessor.HttpContext.SigninToSwagger(accessToken);

        // 设置响应报文头
        accessor.HttpContext!.Response.Headers["access-token"] = accessToken;
        accessor.HttpContext!.Response.Headers["x-access-token"] = refreshToken;
    }

    /// <summary>
    ///     修改密码
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task ModifyPwd(BaseDto<ModifyPasswordDto> dto)
    {
        await AthensUser(dto.User.Ucode, dto.Data.Pwd);
        //await dbFunc.usp_czy_changepassword(dto.User.Ucode, dto.Data.NewPwd);
    }
}