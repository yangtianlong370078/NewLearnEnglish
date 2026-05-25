namespace LearnEnglish.Models.Entitys
{
    /// <summary>
    /// 考卷详情表
    /// </summary>
    public class examdetail
    {
        [IsEffective(false)]
        public int id { get; set; }

        /// <summary>
        /// 考卷Id
        /// </summary>
        public int examid { get; set; }

        /// <summary>
        /// 学习表
        /// </summary>
        public int learnid { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int lexiconId { get; set; }
    }
}
