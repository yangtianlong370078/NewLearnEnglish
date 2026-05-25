namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 考卷答题结果表
    /// </summary>
    public class ExamAnswer
    {
        /// <summary>
        /// 答案Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 考试类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 考卷Id
        /// </summary>
        public int ExamId { get; set; }

        /// <summary>
        /// 考卷详情Id
        /// </summary>
        public int ExamDetailId { get; set; }

        /// <summary>
        /// 是否正确
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// 答题内容
        /// </summary>
        public string Answer { get; set; } = string.Empty;
    }
}
