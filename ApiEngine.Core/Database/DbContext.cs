namespace ApiEngine.Core.Database;

public class DbContext
{
    public static readonly ICacheService CacheService = new SqlSugarCache();

    public static SqlSugarScope GetNew()
    {
        return new SqlSugarScope([.. App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs")], db =>
        {
            var cfg = db.CurrentConnectionConfig;
            cfg.IsAutoCloseConnection = true;
            cfg.LanguageType = LanguageType.Chinese;
            cfg.ConfigureExternalServices = new ConfigureExternalServices
            {
                DataInfoCacheService = CacheService
            };
            cfg.MoreSettings = new ConnMoreSettings
            {
                IsAutoRemoveDataCache = true,
                IsWithNoLockQuery = true
            };

            db.Ado.CommandTimeOut = 120;
            db.Aop.OnError = ex =>
            {
                var sqlLog = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, ex.Sql,
                    (SugarParameter[])ex.Parametres);
                sqlLog.LogError(ex);
            };
#if DEBUG
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                var sqlLog = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                sqlLog.LogInformation();
            };
#endif
        });
    }
}