using LearnEnglish.Domain.Entities;

namespace LearnEnglish.Infrastructure.MongoDB
{
    /// <summary>
    /// MongoDB 单词详情 Repository 接口
    /// </summary>
    public interface ILexiconDetailRepository
    {
        /// <summary>
        /// 根据单词查询详情
        /// </summary>
        Task<LexiconDetail?> GetByWordAsync(string word);

        /// <summary>
        /// 根据单词查询简单版详情
        /// </summary>
        Task<LexiconDetailSimple?> GetSimpleByWordAsync(string word);

        /// <summary>
        /// 批量查询单词详情
        /// </summary>
        Task<IEnumerable<LexiconDetail>> GetByWordsAsync(IEnumerable<string> words);
    }
}
