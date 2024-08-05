using ApiEngine.Core.Database.SqlSugar;
using SqlSugar;
using SqlSugar.Extensions;

namespace ApiEngine.Core.Extension;

public static class SqlSugarExtension
{
    /// <summary>
    ///     30秒
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> WithCache30<T>(this ISugarQueryable<T> queryable) where T : class, new()
    {
        return queryable.WithCache(30);
    }

    /// <summary>
    ///     5分钟
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> WithCache300<T>(this ISugarQueryable<T> queryable) where T : class, new()
    {
        return queryable.WithCache(300);
    }

    /// <summary>
    ///     30分钟
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> WithCache1800<T>(this ISugarQueryable<T> queryable) where T : class, new()
    {
        return queryable.WithCache(1800);
    }

    /// <summary>
    ///     2小时
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> WithCache7200<T>(this ISugarQueryable<T> queryable) where T : class, new()
    {
        return queryable.WithCache(7200);
    }

    /// <summary>
    ///     获取配置
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static OwnConnectionConfig GetDbConfig(this ISqlSugarClient db)
    {
        var config = DbContext.ConnectionConfigs.First(f => f.ConfigId == db.CurrentConnectionConfig.ConfigId);
        return config;
    }

    /// <summary>
    ///     分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalNumber"></param>
    /// <param name="totalPage"></param>
    /// <returns></returns>
    public static List<T> VersionToPageList<T>(this ISugarQueryable<T> queryable, int pageNumber, int pageSize,
        ref int totalNumber, ref int totalPage) where T : class, new()
    {
        var dbConfig = queryable.Context.GetDbConfig();
        if (dbConfig.DbType == DbType.SqlServer && dbConfig.Version.ObjToInt() > 2008)
            return queryable.ToOffsetPage(pageNumber, pageSize, ref totalNumber, ref totalPage);

        return queryable.ToPageList(pageNumber, pageSize, ref totalNumber, ref totalPage);
    }

    /// <summary>
    ///     分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalNumber"></param>
    /// <param name="totalPage"></param>
    /// <returns></returns>
    public static async Task<List<T>> VersionToPageListAsync<T>(this ISugarQueryable<T> queryable, int pageNumber,
        int pageSize, RefAsync<int> totalNumber, RefAsync<int> totalPage) where T : class, new()
    {
        var dbConfig = queryable.Context.GetDbConfig();
        if (dbConfig.DbType == DbType.SqlServer && dbConfig.Version.ObjToInt() > 2008)
            return await queryable.ToOffsetPageAsync(pageNumber, pageSize, totalNumber, totalPage);

        return await queryable.ToPageListAsync(pageNumber, pageSize, totalNumber, totalPage);
    }
}