namespace LearnEnglish.Models.MongoDB
{
    public class lexiconMongoDBOptions
    {
        /// <summary>
        /// 商品集合名
        /// </summary>
        public string lexiconCollectionName { get; set; }

        /// <summary>
        /// 连接MongoDB字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 商品数据库名
        /// </summary>
        public string DatabaseName { get; set; }
    }
}
