namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 课程内容详情表
    /// </summary>
    public class CourseContent
    {
        /// <summary>
        /// 详情Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int LexiconId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
