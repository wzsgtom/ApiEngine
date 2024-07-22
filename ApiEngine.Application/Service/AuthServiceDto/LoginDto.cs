using ApiEngine.Core.Gen;
using System.ComponentModel.DataAnnotations;

namespace ApiEngine.Application.Service.AuthServiceDto;

public class LoginDto
{
    /// <summary>
    ///     密码
    /// </summary>
    /// <example>1</example>
    [Display(Name = "密码")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public string Pwd { get; set; }

    /// <summary>
    ///     Mac
    /// </summary>
    public string Mac { get; init; }
}

public class ModifyPasswordDto : LoginDto
{
    /// <summary>
    ///     新密码
    /// </summary>
    /// <example>1</example>
    [Display(Name = "新密码")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public string NewPwd { get; set; }
}