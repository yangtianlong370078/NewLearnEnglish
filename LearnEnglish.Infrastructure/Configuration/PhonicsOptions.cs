namespace LearnEnglish.Infrastructure.Configuration
{
    /// <summary>
    /// 自然拼读拆分显示选项。
    /// </summary>
    public sealed class PhonicsOptions
    {
        /// <summary>
        /// 相比 CMU 标准音节数允许额外显示的拼读块数。
        /// 0 表示严格音节模式；2 表示适度拼读模式；较大值接近自由拼读模式。
        /// </summary>
        public int Tolerance { get; set; } = 2;
    }
}