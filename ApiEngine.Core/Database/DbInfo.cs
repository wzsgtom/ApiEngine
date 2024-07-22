using Furion.Logging.Extensions;
using SqlSugar;
using SqlSugar.Extensions;

namespace ApiEngine.Core.Database;

public class DbInfo
{
    public static string VersionString { get; set; }
    public static int YearVersionInt { get; set; }

    public static void SetDbVersion(ISqlSugarClient db)
    {
        try
        {
            VersionString = db.Ado.GetString("SELECT @@VERSION");

            var yearVersion = VersionString.SafeSubstring(21, 4);
            $"{yearVersion} ——> {VersionString}".LogInformation<DbInfo>();

            YearVersionInt = yearVersion.ObjToInt();
        }
        catch (Exception e)
        {
            e.Message.LogError(e);
        }
    }
}