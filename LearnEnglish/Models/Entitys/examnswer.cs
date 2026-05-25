namespace LearnEnglish.Models.Entitys
{
    /// <summary>
    /// 考卷结果表
    /// </summary>
    public class examnswer
    {
        [IsEffective(false)]
        public int id { get; set; }

        /// <summary>
        /// 考试类型
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 考卷Id
        /// </summary>
        public int examid { get; set; }

        /// <summary>
        /// 考卷详情Id
        /// </summary>
        public int examdetailid { get; set; }

        /// <summary>
        /// 是否正确
        /// </summary>
        public bool isok { get; set; }

        /// <summary>
        /// 答题内容
        /// </summary>
        public string answer { get; set; }
    }
}
