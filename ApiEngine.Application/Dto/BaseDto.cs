using ApiEngine.Core.Gen;
using System.ComponentModel.DataAnnotations;

namespace ApiEngine.Application.Dto;

public class BaseDto<T> where T : class, new()
{
    /// <summary>
    ///     用户信息
    /// </summary>
    [Display(Name = "用户信息")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public UserClass User { get; init; }

    /// <summary>
    ///     数据
    /// </summary>
    [Display(Name = "参数data")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public T Data { get; set; }
}

public class UserClass
{
    /// <summary>
    ///     科室编码
    /// </summary>
    /// <example>0001</example>
    [Display(Name = "科室编码")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public string Udept { get; set; }

    /// <summary>
    ///     用户类别（1-经办人；2-自助终端；3-移动终端）
    /// </summary>
    /// <example>1</example>
    [Display(Name = "用户类别")]
    public string Utype { get; init; } = "1";

    /// <summary>
    ///     用户编码
    /// </summary>
    /// <example>000</example>
    [Display(Name = "用户编码")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public string Ucode { get; init; }

    /// <summary>
    ///     用户姓名
    /// </summary>
    /// <example>系统管理</example>
    [Display(Name = "用户姓名")]
    [Required(ErrorMessage = GenConst.RequiredPrompt)]
    public string Uname { get; init; }

    /// <summary>
    ///     使用时间
    /// </summary>
    [Display(Name = "使用时间")]
    public DateTime UDateTime { get; init; } = DateTime.Now;
}