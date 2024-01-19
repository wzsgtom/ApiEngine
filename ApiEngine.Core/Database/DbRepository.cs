namespace ApiEngine.Core.Database;

/// <summary>
///     仓储模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class DbRepository<T> : SimpleClient<T> where T : class, new()
{
    public DbRepository(ISqlSugarClient context = null) : base(context)
    {
        Context = context ?? App.GetService<ISqlSugarClient>();
    }
}