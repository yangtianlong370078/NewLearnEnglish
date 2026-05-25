namespace LearnEnglish.Models.Entitys
{
    /// <summary>
    /// 考卷主表
    /// </summary>
    public class exam
    {
        /// <summary>
        /// 考卷Id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 考卷名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 考生
        /// </summary>
        public int userid { get; set; }

        /// <summary>
        /// 考试单词数量
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// 考卷创建时间
        /// </summary>
        public DateTime createdate { get; set; }
    }
}
