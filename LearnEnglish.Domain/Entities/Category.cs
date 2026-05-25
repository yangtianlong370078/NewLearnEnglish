namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 课程分类表
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 分类Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类类型
        /// </summary>
        public int Type { get; set; }
    }
}
