using SqlSugar;

namespace ApiEngine.Core.Database.SqlSugar;

public class OwnConnectionConfig : ConnectionConfig
{
    public string Version { get; set; } = "2008";
}