namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 考卷主表
    /// </summary>
    public class Exam
    {
        /// <summary>
        /// 考卷Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 考卷名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 考生用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 考试单词数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 限时（秒）
        /// </summary>
        public int LimitTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
