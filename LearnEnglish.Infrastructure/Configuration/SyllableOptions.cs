namespace LearnEnglish.Infrastructure.Configuration
{
    /// <summary>
    /// 音节拆分服务配置。
    /// </summary>
    public sealed class SyllableOptions
    {
        /// <summary>
        /// CMU 发音词典路径。相对路径以应用程序工作目录为基准。
        /// </summary>
        public string CmuDictionaryPath { get; set; } = "../CMU/cmudict-0.7b";
    }
}
