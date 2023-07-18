namespace ApiEngine.Application.Services;

/// <summary>
///     基础服务
/// </summary>
[AllowAnonymous]
public class BaseService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;

    public BaseService(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    ///     服务器日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDate()
    {
        return _db.GetDate();
    }
}