using ApiEngine.Core.Extension;
using ApiEngine.Core.Gen;
using Furion.DependencyInjection;
using Furion.LinqBuilder;
using Furion.UnifyResult;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace ApiEngine.Core.Database.SqlSugar;

/// <summary>
///     通用数据库方法
/// </summary>
public class DbFunc(ISqlSugarClient db) : IScoped
{
    /// <summary>
    ///     检查表是否存在，不存在则创建
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public void CheckTable(params Type[] types)
    {
        var listType = (from type in types
            let tableName = db.EntityMaintenance.GetTableName(type)
            where !db.DbMaintenance.IsAnyTable(tableName)
            select type).ToList();
        if (listType.Count > 0) db.CodeFirst.InitTables(listType.ToArray());
    }

    /// <summary>
    ///     通用查询（主键）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pkValue"></param>
    /// <returns></returns>
    public async Task<T> QueryMod<T>(object pkValue) where T : class, new()
    {
        return await db.Queryable<T>().InSingleAsync(pkValue);
    }

    /// <summary>
    ///     通用查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryMod"></param>
    /// <param name="pageMod"></param>
    /// <param name="unifyFillPage"></param>
    /// <returns></returns>
    public List<T> QueryList<T>(QueryMod<T> queryMod, PageMod pageMod = null, bool unifyFillPage = true)
        where T : class, new()
    {
        var exp = GetExpressionable(queryMod);

        var queryable = queryMod.Alias.IsNullOrEmpty()
            ? db.Queryable<T>().AS(queryMod.Alias).Where(exp.ToExpression())
            : db.Queryable<T>().Where(exp.ToExpression());

        return TryPage(queryable, pageMod, unifyFillPage);
    }

    /// <summary>
    ///     通用分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageMod"></param>
    /// <param name="unifyFillPage"></param>
    /// <returns></returns>
    public List<T> TryPage<T>(ISugarQueryable<T> queryable, PageMod pageMod = null, bool unifyFillPage = true)
        where T : class, new()
    {
        if (pageMod is not { PageNumber: > 0, PageSize: > 0 }) return queryable.ToList();

        var totalNumber = 0;
        var totalPage = 0;
        var pageList =
            queryable.VersionToPageList(pageMod.PageNumber, pageMod.PageSize, ref totalNumber, ref totalPage);

        pageMod.TotalNumber = totalNumber;
        pageMod.TotalPage = totalPage;

        if (unifyFillPage) UnifyContext.Fill(new { page = pageMod });

        return pageList;
    }

    /// <summary>
    ///     通用查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryMod"></param>
    /// <param name="pageMod"></param>
    /// <param name="unifyFillPage"></param>
    /// <returns></returns>
    public async Task<List<T>> QueryListAsync<T>(QueryMod<T> queryMod, PageMod pageMod = null,
        bool unifyFillPage = true) where T : class, new()
    {
        var exp = GetExpressionable(queryMod);

        var queryable = queryMod.Alias.IsNullOrEmpty()
            ? db.Queryable<T>().AS(queryMod.Alias).Where(exp.ToExpression())
            : db.Queryable<T>().Where(exp.ToExpression());

        return await TryPageAsync(queryable, pageMod, unifyFillPage);
    }

    /// <summary>
    ///     通用分页
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageMod"></param>
    /// <param name="unifyFillPage"></param>
    /// <returns></returns>
    public async Task<List<T>> TryPageAsync<T>(ISugarQueryable<T> queryable, PageMod pageMod = null,
        bool unifyFillPage = true) where T : class, new()
    {
        if (pageMod is not { PageNumber: > 0, PageSize: > 0 }) return await queryable.ToListAsync();

        RefAsync<int> totalNumber = 0;
        RefAsync<int> totalPage = 0;
        var pageList =
            await queryable.VersionToPageListAsync(pageMod.PageNumber, pageMod.PageSize, totalNumber, totalPage);

        pageMod.TotalNumber = totalNumber.Value;
        pageMod.TotalPage = totalPage.Value;

        if (unifyFillPage) UnifyContext.Fill(new { page = pageMod });

        return pageList;
    }

    /// <summary>
    ///     获取表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryMod"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Expressionable<T> GetExpressionable<T>(QueryMod<T> queryMod) where T : class, new()
    {
        var exp = new Expressionable<T>();
        switch (queryMod.ExpressConnType)
        {
            case ExpressConnType.AndIf:
                foreach (var (isAnd, expression) in queryMod.WhereExpression) exp.AndIF(isAnd, expression);
                break;
            case ExpressConnType.OrIf:
                foreach (var (isAnd, expression) in queryMod.WhereExpression) exp.OrIF(isAnd, expression);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return exp;
    }

    /// <summary>
    ///     通用新增
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mod"></param>
    /// <returns></returns>
    public async Task<int> Insert<T>(T mod) where T : class, new()
    {
        return await db.Insertable(mod).ExecuteCommandAsync();
    }

    /// <summary>
    ///     通用更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mod"></param>
    /// <returns></returns>
    public async Task<int> Update<T>(T mod) where T : class, new()
    {
        return await db.Updateable(mod).ExecuteCommandAsync();
    }

    /// <summary>
    ///     通用删除（主键）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pkValue"></param>
    /// <returns></returns>
    public async Task<int> Delete<T>(object pkValue) where T : class, new()
    {
        return await db.Deleteable<T>(pkValue).ExecuteCommandAsync();
    }

    /// <summary>
    ///     通用保存（判断主键，新增和更新）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task<int> Save<T>(T t) where T : class, new()
    {
        var storage = await db.Storageable(t).ToStorageAsync();
        if (storage.InsertList.Count > 0) await storage.AsInsertable.ExecuteCommandAsync();
        if (storage.UpdateList.Count > 0) await storage.AsUpdateable.ExecuteCommandAsync();

        return storage.TotalList.Count;
    }
}

/// <summary>
///     查询类
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryMod<T> where T : class
{
    public QueryMod()
    {
        WhereExpression = [];
    }

    public QueryMod(params (bool, Expression<Func<T, bool>>)[] whereExpression)
    {
        WhereExpression = [.. whereExpression];
    }

    /// <summary>
    ///     别名
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    ///     表达式连接类型
    /// </summary>
    public ExpressConnType ExpressConnType { get; set; } = ExpressConnType.AndIf;

    /// <summary>
    ///     查询条件表达式集合
    /// </summary>
    public ICollection<(bool, Expression<Func<T, bool>>)> WhereExpression { get; set; }
}

/// <summary>
///     分页类
/// </summary>
public class PageMod : IValidatableObject
{
    public PageMod()
    {
    }

    public PageMod(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    ///     第几页
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    ///     每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     总记录数
    /// </summary>
    public int TotalNumber { get; set; }

    /// <summary>
    ///     总页数
    /// </summary>
    public int TotalPage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PageNumber <= 0) PageNumber = 1;
        if (PageSize > 1000) yield return new ValidationResult("分页大小不能超过 1000条/页");
    }
}