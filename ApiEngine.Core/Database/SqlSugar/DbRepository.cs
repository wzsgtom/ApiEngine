using Furion;
using SqlSugar;

namespace ApiEngine.Core.Database.SqlSugar;

/// <summary>
///     仓储模式
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class DbRepository<T> : SimpleClient<T> where T : class, new()
{
    public DbRepository(ISqlSugarClient context) : base(context)
    {
        Context = context;
    }

    /// <summary>
    ///     调用 DbFunc QueryList
    /// </summary>
    /// <param name="queryMod"></param>
    /// <param name="pageMod"></param>
    /// <param name="unifyFillPage"></param>
    /// <returns></returns>
    public List<T> QueryList(QueryMod<T> queryMod, PageMod pageMod = null, bool unifyFillPage = true)
    {
        var dbFunc = App.GetService<DbFunc>();
        return dbFunc.QueryList(queryMod, pageMod, unifyFillPage);
    }

    /// <summary>
    ///     Storageable 单个保存
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<T> Save(T dto)
    {
        return await Context.Storageable(dto).ExecuteReturnEntityAsync();
    }
}