namespace ApiEngine.Application.Dtos;

public class LoginDto
{
    /// <summary>
    ///     用户编码
    /// </summary>
    /// <example>000</example>
    [Required(ErrorMessage = "用户编码不能为空")]
    public string account { get; set; }

    /// <summary>
    ///     密码
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "密码不能为空")]
    public string password { get; set; }

    /// <summary>
    ///     用户名称
    /// </summary>
    public string name { get; set; }
}