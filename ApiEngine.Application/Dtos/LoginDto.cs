namespace ApiEngine.Application.Dtos;

public class LoginDto
{
    /// <summary>
    ///     用户编码
    /// </summary>
    /// <example>000</example>
    [Display(Name = "用户编码")]
    [Required(ErrorMessage = Const.RequiredPrompt)]
    public string account { get; set; }

    /// <summary>
    ///     密码
    /// </summary>
    /// <example>1</example>
    [Display(Name = "密码")]
    [Required(ErrorMessage = Const.RequiredPrompt)]
    public string password { get; set; }

    /// <summary>
    ///     用户名称
    /// </summary>
    [Display(Name = "用户名称")]
    public string name { get; set; }
}