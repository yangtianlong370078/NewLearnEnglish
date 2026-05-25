namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 用户自定义单词扩展表
    /// </summary>
    public class MyLexicon
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 单词Id
        /// </summary>
        public int LexiconId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 自定义中文释义
        /// </summary>
        public string Cn { get; set; } = string.Empty;

        /// <summary>
        /// 学习状态（1=未认识，2=未牢记，3=已掌握）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 中译英正确次数
        /// </summary>
        public int ZyNumber { get; set; }

        /// <summary>
        /// 英译中正确次数
        /// </summary>
        public int YzNumber { get; set; }

        /// <summary>
        /// 听写正确次数
        /// </summary>
        public int TxNumber { get; set; }

        /// <summary>
        /// 发音正确次数
        /// </summary>
        public int FyNumber { get; set; }

        /// <summary>
        /// 总练习次数
        /// </summary>
        public int NumberSum { get; set; }

        /// <summary>
        /// 是否收藏（0=否，1=是）
        /// </summary>
        public int IsCollect { get; set; }

        /// <summary>
        /// 是否手动操作过
        /// </summary>
        public int IsHand { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 更新日期（不含时分秒）
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}
