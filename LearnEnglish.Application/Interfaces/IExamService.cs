using LearnEnglish.Application.Dtos.Exam;
using LearnEnglish.Domain.Common;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 考试服务接口
    /// </summary>
    public interface IExamService
    {
        /// <summary>
        /// 创建考试（从已掌握单词中抽题）
        /// </summary>
        Task<(bool success, string message, int wordCount)> CreateExamAsync(int userId, int examCount, int limitTime);

        /// <summary>
        /// 获取考试详情（含各维度答题统计）
        /// </summary>
        Task<ExamOutDto?> GetExamDetailAsync(int examId);

        /// <summary>
        /// 获取考试列表（分页）
        /// </summary>
        Task<(List<ExamOutDto> list, int total)> GetExamListAsync(int userId, string? name, int pageIndex, int pageSize);

        /// <summary>
        /// 获取考试基本信息（名称、限时等）
        /// </summary>
        Task<(int id, string name, int limitTime)?> GetExamInfoAsync(int userId, int examId);

        /// <summary>
        /// 获取考试内容详情列表
        /// </summary>
        Task<(List<ExamContentOutDto> items, int? limitTime, int? score)> GetExamContentListAsync(int userId, int examId, int type);

        /// <summary>
        /// 提交考试答案（批量）
        /// </summary>
        Task<bool> SubmitExamAnswersAsync(int userId, string data, int examId, int type, int score);

        /// <summary>
        /// 删除考试
        /// </summary>
        Task<bool> DeleteExamAsync(int examId);

        /// <summary>
        /// 重新考试（清除指定类型的答案和记录）
        /// </summary>
        Task<bool> ReExamAsync(int examId, int type);
    }
}
