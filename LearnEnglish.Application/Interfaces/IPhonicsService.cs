namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 按自然拼读规则将英文单词拆分为字母组合及其对应音标。
    /// </summary>
    public interface IPhonicsService
    {
        /// <summary>
        /// 按原词顺序返回自然拼读拆分结果。
        /// </summary>
        IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> Split(string word);

        /// <summary>
        /// 以词典音节边界为约束返回自然拼读拆分结果。
        /// </summary>
        IReadOnlyList<(string LetterCombine, string PhoneticSymbol)> Split(string word, IReadOnlyList<string> syllables);
    }
}