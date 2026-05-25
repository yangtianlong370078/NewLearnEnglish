namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 学习任务表
    /// </summary>
    public class LearnTask
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 每日任务数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 周末是否学习（0=否，1=是）
        /// </summary>
        public int Weekend { get; set; }
    }
}
