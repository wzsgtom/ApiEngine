using ApiEngine.Application.Service.BaseServiceDto;
using ApiEngine.Application.Util;
using ApiEngine.Core.Database.SqlSugar;
using ApiEngine.Core.Extension;
using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using Yitter.IdGenerator;

namespace ApiEngine.Application.Service;

/// <summary>
///     系统基础服务
/// </summary>
[ApiDescriptionSettings(Name = "base", Version = "1")]
public class BaseService(ISqlSugarClient db, DbFunc dbFunc) : IDynamicApiController, IScoped
{
    /// <summary>
    ///     服务器日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDate()
    {
        return db.GetDate();
    }

    /// <summary>
    ///     唯一值🔖
    /// </summary>
    /// <returns></returns>
    public long GetNextId()
    {
        return YitIdHelper.NextId();
    }

    /// <summary>
    ///     动态表格查询🔖
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<List<object>> DynamicTableQuery([Required] DynamicTableQueryDto dto)
    {
        // 创建查询对象
        var queryable = db.Queryable<object>().AS(dto.Name, "t1");

        // 添加关联表信息
        if (dto.JoinInfos?.Count > 0)
            dto.JoinInfos?.Aggregate(queryable,
                (current, dtoJoinInfo) => current.AddJoinInfo(dtoJoinInfo.TableName, dtoJoinInfo.ShortName,
                    dtoJoinInfo.JoinWhere, dtoJoinInfo.JoinType));

        // 选择需要查询的字段
        queryable.Select<object>(dto.QueryFields is { Count: > 0 } ? dto.QueryFields.StringJoin() : "t1.*");

        // 将 JSON 字符串转换为条件模型
        var conditionals = db.Utilities.JsonToConditionalModels(dto.Json);
        if (conditionals.Count > 0) queryable.Where(conditionals);

        // 拼接查询条件
        queryable.WhereIF(!dto.WhereString.IsNullOrEmpty(), dto.WhereString);

        // 根据排序字段进行排序
        if (dto.OrderFields is { Count: > 0 }) queryable.OrderBy(dto.OrderFields.StringJoin());

        // 获取指定数量的记录
        if (dto.Take > 0) queryable.Take(dto.Take);

        // 根据缓存类型添加缓存配置
        switch (dto.Cache)
        {
            case CacheEnum.S300:
                queryable.WithCache300();
                break;
            case CacheEnum.S1800:
                queryable.WithCache1800();
                break;
            case CacheEnum.S7200:
                queryable.WithCache7200();
                break;
            case CacheEnum.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dto.Cache));
        }

        // 执行分页查询并返回结果
        var list = await dbFunc.TryPageAsync(queryable, dto.Page);
        return list;
    }
}