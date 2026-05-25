namespace LearnEnglish.Domain.Entities
{
    /// <summary>
    /// 单词表
    /// </summary>
    public class Lexicon
    {
        /// <summary>
        /// 单词Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 中文释义
        /// </summary>
        public string Cn { get; set; } = string.Empty;

        /// <summary>
        /// 英文单词
        /// </summary>
        public string En { get; set; } = string.Empty;

        /// <summary>
        /// 创建用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 是否有英式发音（0=否，1=是）
        /// </summary>
        public int IsEnAudio { get; set; }

        /// <summary>
        /// 是否有美式发音（0=否，1=是）
        /// </summary>
        public int IsUsAudio { get; set; }

        /// <summary>
        /// 词频
        /// </summary>
        public int Frequency { get; set; }
    }
}
