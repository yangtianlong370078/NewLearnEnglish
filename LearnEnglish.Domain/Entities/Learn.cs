namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 学习记录表
    /// </summary>
    public class Learn
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 课程内容Id（可以根据这个对应到具体的单词）
        /// </summary>
        public int CourseContentId { get; set; }

        /// <summary>
        /// 学习状态（2=未牢记，3=已掌握）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 错误次数统计
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 是否收藏（0=否，1=是）
        /// </summary>
        public int IsCollect { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

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
