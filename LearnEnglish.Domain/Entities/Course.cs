namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 课程表
    /// </summary>
    public class Course
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所属用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 课程分类Id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
