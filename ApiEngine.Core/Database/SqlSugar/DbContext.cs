using Furion;
using Furion.Logging.Extensions;
using SqlSugar;

namespace ApiEngine.Core.Database.SqlSugar;

public class DbContext
{
    public static readonly IReadOnlyList<OwnConnectionConfig> ConnectionConfigs =
        App.GetConfig<List<OwnConnectionConfig>>("ConnectionConfigs");

    public static readonly ICacheService CacheService = new DbCache();

    public static SqlSugarScope GetNew()
    {
        return new SqlSugarScope([.. ConnectionConfigs], db =>
        {
            db.CurrentConnectionConfig.IsAutoCloseConnection = true;
            db.CurrentConnectionConfig.LanguageType = LanguageType.Chinese;
            db.CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices
            {
                DataInfoCacheService = CacheService
            };
            db.CurrentConnectionConfig.MoreSettings = new ConnMoreSettings
            {
                IsAutoRemoveDataCache = true,
                IsWithNoLockQuery = true,
                DisableWithNoLockWithTran = true
            };

            db.Ado.CommandTimeOut = 120;
            db.Aop.OnError = ex =>
            {
                var sqlLog = UtilMethods.GetNativeSql(ex.Sql, (SugarParameter[])ex.Parametres);
                sqlLog.LogError(ex);
            };
#if DEBUG
            db.Aop.OnLogExecuting = (s, p) =>
            {
                var sqlLog = UtilMethods.GetNativeSql(s, p);
                sqlLog.LogDebug();
            };
#endif
        });
    }
}