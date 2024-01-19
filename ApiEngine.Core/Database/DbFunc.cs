namespace ApiEngine.Core.Database;

/// <summary>
///     通用数据库方法
/// </summary>
public class DbFunc(ISqlSugarClient db) : ITransient
{
    /// <summary>
    ///     服务器时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDate()
    {
        return db.GetDate();
    }

    /// <summary>
    ///     检查表是否存在，不存在则创建
    /// </summary>
    /// <param name="type"></param>
    public void CheckTable(Type type)
    {
        if (!db.DbMaintenance.IsAnyTable(db.EntityMaintenance.GetTableName(type), false))
        {
            db.CodeFirst.InitTables(type);
        }
    }

    /// <summary>
    ///     检查表是否存在，不存在则创建
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public void CheckTable(IList<Type> types)
    {
        var listType = (from type in types
            let tableName = db.EntityMaintenance.GetTableName(type)
            where !db.DbMaintenance.IsAnyTable(tableName, false)
            select type).ToList();
        if (listType.Count > 0)
        {
            db.CodeFirst.InitTables(listType.ToArray());
        }
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

    public async Task<List<T>> QueryList<T>(QueryMod<T> queryMod, PageMod pageMod = null, bool UnifyContextFill = true) where T : class, new()
    {
        var exp = GetExpressionable(queryMod);

        var iQueryable = queryMod.alias.IsNullOrEmpty()
            ? db.Queryable<T>().AS(queryMod.alias).Where(exp.ToExpression())
            : db.Queryable<T>().Where(exp.ToExpression());

        return await TryPage(iQueryable, pageMod, UnifyContextFill);
    }

    public Expressionable<T> GetExpressionable<T>(QueryMod<T> queryMod) where T : class, new()
    {
        var exp = new Expressionable<T>();
        foreach (var (isAnd, expression) in queryMod.whereExpression)
        {
            exp.AndIF(isAnd, expression);
        }

        return exp;
    }

    public async Task<List<T>> TryPage<T>(ISugarQueryable<T> iQueryable, PageMod pageMod = null, bool UnifyContextFill = true) where T : class, new()
    {
        if (pageMod is not { pageNumber: > 0, pageSize: > 0 })
        {
            return await iQueryable.ToListAsync();
        }

        RefAsync<int> totalNumber = 0;
        RefAsync<int> totalPage = 0;
        var pageList = await iQueryable.ToPageListAsync(pageMod.pageNumber, pageMod.pageSize, totalNumber, totalPage);

        pageMod.totalNumber = totalNumber.Value;
        pageMod.totalPage = totalPage.Value;

        if (UnifyContextFill)
        {
            UnifyContext.Fill(new { page = pageMod });
        }

        return pageList;
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
        if (storage.InsertList.Count > 0)
        {
            await storage.AsInsertable.ExecuteCommandAsync();
        }

        if (storage.UpdateList.Count > 0)
        {
            await storage.AsUpdateable.ExecuteCommandAsync();
        }

        return storage.TotalList.Count;
    }
}

/// <summary>
///     查询类
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryMod<T> where T : class
{
    public QueryMod(params (bool, Expression<Func<T, bool>>)[] whereExpression)
    {
        this.whereExpression = whereExpression.ToList();
    }

    /// <summary>
    ///     别名
    /// </summary>
    public string alias { get; set; }

    /// <summary>
    ///     查询条件表达式集合
    /// </summary>
    public List<(bool, Expression<Func<T, bool>>)> whereExpression { get; set; }
}

/// <summary>
///     分页类
/// </summary>
public class PageMod
{
    public PageMod() { }

    public PageMod(int pageNumber, int pageSize)
    {
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
    }

    /// <summary>
    ///     第几页
    /// </summary>
    public int pageNumber { get; set; }

    /// <summary>
    ///     每页大小
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    ///     总记录数
    /// </summary>
    public int totalNumber { get; set; }

    /// <summary>
    ///     总页数
    /// </summary>
    public int totalPage { get; set; }
}