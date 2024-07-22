using ApiEngine.Application.Util;
using ApiEngine.Core.Database.SqlSugar;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace ApiEngine.Application.Service.BaseServiceDto;

public class DynamicTableQueryDto : IValidatableObject
{
    /// <summary>
    ///     动态表名
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    ///     查询语句
    /// </summary>
    [Required]
    public required string Json { get; set; }

    /// <summary>
    ///     查询字段
    /// </summary>
    public List<string> QueryFields { get; set; }

    /// <summary>
    ///     排序字段
    /// </summary>
    public List<string> OrderFields { get; set; }

    /// <summary>
    ///     缓存
    /// </summary>
    public CacheEnum Cache { get; set; } = CacheEnum.None;

    /// <summary>
    ///     取前几行
    /// </summary>
    [Range(0, 1000)]
    public int Take { get; set; }

    /// <summary>
    ///     分页
    /// </summary>
    public PageMod Page { get; set; }

    /// <summary>
    ///     关联表信息
    /// </summary>
    public List<JoinInfo> JoinInfos { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Page is not null && Cache is not CacheEnum.None) yield return new ValidationResult("分页查询暂不支持使用缓存！");
    }

    public class JoinInfo
    {
        public string TableName { get; set; }
        public string ShortName { get; set; }
        public string JoinWhere { get; set; }
        public JoinType JoinType { get; set; } = JoinType.Left;
    }
}