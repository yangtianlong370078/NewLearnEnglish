namespace LearnEnglish.Application.Dtos.Word
{
    /// <summary>
    /// 单词展示DTO（含学习状态信息）
    /// </summary>
    public class ShowTranslateDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseContentId { get; set; }
        public int LexiconId { get; set; }
        public int IsCollect { get; set; }
        /// <summary>
        /// 总错误次数
        /// </summary>
        public int NumberSum { get; set; }
        /// <summary>
        /// 中译英错误次数
        /// </summary>
        public int ZyNumber { get; set; }
        /// <summary>
        /// 英译中错误次数
        /// </summary>
        public int YzNumber { get; set; }
        /// <summary>
        /// 听写错误次数
        /// </summary>
        public int TxNumber { get; set; }
        /// <summary>
        /// 翻译错误次数
        /// </summary>
        public int FyNumber { get; set; }
        public string En { get; set; } = string.Empty;
        public string Cn { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        /// <summary>
        /// 学习状态
        /// </summary>
        public int Zt { get; set; }
        /// <summary>
        /// 是否有英式发音
        /// </summary>
        public int IsEnAudio { get; set; }
        /// <summary>
        /// 是否有美式发音
        /// </summary>
        public int IsUsAudio { get; set; }
        public bool IsUpdate { get; set; }
    }

    /// <summary>
    /// 校准请求DTO
    /// </summary>
    public class CalibrationDto
    {
        public int LexiconId { get; set; }
        public int Number { get; set; }
        public int Type { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
