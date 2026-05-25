namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 用户学习记录表（按日汇总）
    /// </summary>
    public class MyLearn
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int LexiconId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 修改日期（不含时分秒）
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}
