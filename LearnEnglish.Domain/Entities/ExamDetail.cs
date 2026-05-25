namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 考卷详情表
    /// </summary>
    public class ExamDetail
    {
        /// <summary>
        /// 详情Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 考卷Id
        /// </summary>
        public int ExamId { get; set; }

        /// <summary>
        /// 学习记录Id
        /// </summary>
        public int LearnId { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int LexiconId { get; set; }

        /// <summary>
        /// 英文（JOIN查询填充，非持久化字段）
        /// </summary>
        public string? En { get; set; }

        /// <summary>
        /// 中文（JOIN查询填充，非持久化字段）
        /// </summary>
        public string? Cn { get; set; }
    }
}
