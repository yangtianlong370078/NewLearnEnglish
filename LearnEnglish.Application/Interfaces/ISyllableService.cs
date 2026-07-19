namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 提供英文单词的音节拆分。
    /// </summary>
    public interface ISyllableService
    {
        /// <summary>
        /// 将英文单词拆分为按原始顺序排列的音节。
        /// </summary>
        IReadOnlyList<string> GetSyllables(string word);
    }
}
