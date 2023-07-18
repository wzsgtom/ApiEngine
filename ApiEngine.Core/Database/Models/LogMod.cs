namespace ApiEngine.Core.Database.Models;

public class LogMod
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(ColumnDataType = "datetime2(7)")]
    public DateTime LongDate { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(50)", IsNullable = true)]
    public string Level { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(MAX)", IsNullable = true)]
    public string Message { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(250)", IsNullable = true)]
    public string Logger { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(100)", IsNullable = true)]
    public string TraceIdentifier { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(MAX)", IsNullable = true)]
    public string Exception { get; set; }
}