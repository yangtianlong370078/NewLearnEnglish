namespace LearnEnglish.Infrastructure.Data
{
    /// <summary>
    /// 数据库配置选项
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// MySQL 连接字符串
        /// </summary>
        public string MySqlConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// 获取包含连接池参数的完整连接字符串
        /// </summary>
        public string GetConnectionString()
        {
            if (string.IsNullOrEmpty(MySqlConnectionString))
                return string.Empty;

            var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(MySqlConnectionString)
            {
                MinimumPoolSize = (uint)MinPoolSize,
                MaximumPoolSize = (uint)MaxPoolSize,
                ConnectionTimeout = (uint)ConnectionTimeout,
                Pooling = true
            };
            return builder.ConnectionString;
        }
    }
}
