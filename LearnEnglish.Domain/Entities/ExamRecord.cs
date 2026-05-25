namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 考试成绩记录表
    /// </summary>
    public class ExamRecord
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 考卷Id
        /// </summary>
        public int ExamId { get; set; }

        /// <summary>
        /// 考试类型（1=中译英，2=英译中，3=听写，4=发音）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 限时（秒）
        /// </summary>
        public int LimitTime { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
