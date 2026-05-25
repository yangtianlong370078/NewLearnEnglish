using LearnEnglish.Application.Dtos.Word;
using LearnEnglish.Domain.Common;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 单词学习服务接口
    /// </summary>
    public interface IWordService
    {
        /// <summary>
        /// 检查单词是否存在于指定课程中
        /// </summary>
        Task<(bool exists, int isEnAudio, int isUsAudio)> WordExistAsync(int courseId, string en);

        /// <summary>
        /// 获取课程单词分页列表
        /// </summary>
        Task<(PagedList<ShowTranslateDto> pagedList, int newCount, int learningCount, int masteredCount)> GetWordListAsync(int userId, int courseId, int status, int displayType, string? name, int pageIndex, int pageSize);

        /// <summary>
        /// 获取收藏单词分页列表
        /// </summary>
        Task<PagedList<ShowTranslateDto>> GetFavoriteListAsync(int userId, int displayType, string? name, int pageIndex, int pageSize);

        /// <summary>
        /// 校准学习状态（新版：基于 mylexicon 各维度数据）
        /// </summary>
        Task<(int changed, int added, int removed)> CalibrateNewAsync(int userId);

        /// <summary>
        /// 手动设置单词学习状态（1=未学习, 2=未牢记, 3=已掌握）
        /// </summary>
        Task SetWordStatusAsync(int userId, int lexiconId, int status);

        /// <summary>
        /// 批量更新单词各维度练习次数
        /// </summary>
        Task ModifyNumberAsync(int userId, string jsonData);

        /// <summary>
        /// 修改单词英文/中文释义
        /// </summary>
        Task EditWordAsync(int userId, int lexiconId, string en, string cn);

        /// <summary>
        /// 删除课程内容中的单词
        /// </summary>
        Task DeleteWordAsync(int userId, int courseContentId);

        /// <summary>
        /// 获取单词详情（多级缓存：Redis → MongoDB → 外部API）
        /// </summary>
        Task<object?> GetWordDetailAsync(int userId, string word, int courseId);

        /// <summary>
        /// 收藏/取消收藏单词
        /// </summary>
        Task SetCollectAsync(int userId, int lexiconId, int isCollect);

        /// <summary>
        /// 批量更新学习记录数量（updcno 调用，按类型分组批量 upsert）
        /// </summary>
        Task<bool> BatchUpdateNumberAsync(int userId, string jsonData);
    }
}
