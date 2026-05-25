using LearnEnglish.Application.Dtos.Statistics;

namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 学习统计服务接口
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// 查询本周及总学习单词数量
        /// </summary>
        Task<(int weekCount, int totalCount, string userName)> QueryLearnCountAsync(int userId, DateTime startDate);

        /// <summary>
        /// 按月份统计学习情况（含任务对比）
        /// </summary>
        Task<List<StatisticsLearnGroupDto>> GetMonthlyStatisticsAsync(int userId, DateTime startDate);

        /// <summary>
        /// 保存/更新月度学习任务
        /// </summary>
        Task SaveTaskAsync(int userId, int count, int weekend, DateTime date);
    }
}
