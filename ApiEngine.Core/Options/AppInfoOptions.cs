namespace ApiEngine.Core.Options;

public class AppInfoOptions : IConfigurableOptions
{
    /// <summary>
    ///     全局授权
    /// </summary>
    public bool GlobalAuthorize { get; set; }

    /// <summary>
    ///     日志
    /// </summary>
    public LogClass Log { get; set; } = new();

    public class LogClass
    {
        /// <summary>
        ///     日志类型
        /// </summary>
        public LogTypeEnum LogType { get; set; } = LogTypeEnum.File;

        /// <summary>
        ///     请求日志
        /// </summary>
        public bool Request { get; set; }

        /// <summary>
        ///     响应日志
        /// </summary>
        public bool Response { get; set; }
        
        /// <summary>
        ///     忽略关键字
        /// </summary>
        public List<string> IgnoreKeys { get; set; } = new();

        /// <summary>
        ///     数据库日志
        /// </summary>
        public LogDbSetClass LogDbSet { get; set; }

        public class LogDbSetClass
        {
            /// <summary>
            ///     表名
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            ///     保留几个月
            /// </summary>
            public int KeepMonths { get; set; } = 3;
        }
    }
}

/// <summary>
///     日志类型
/// </summary>
public enum LogTypeEnum
{
    File,
    Db
}