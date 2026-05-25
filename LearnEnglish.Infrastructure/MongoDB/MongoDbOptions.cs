namespace LearnEnglish.Infrastructure.MongoDB
{
    /// <summary>
    /// MongoDB 配置选项
    /// </summary>
    public class MongoDbOptions
    {
        /// <summary>
        /// 集合名称
        /// </summary>
        public string LexiconCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}
