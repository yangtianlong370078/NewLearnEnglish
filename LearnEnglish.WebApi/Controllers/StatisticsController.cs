using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 学习统计 API
    /// </summary>
    [Route("api/[controller]")]
    public class StatisticsController : ApiControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public StatisticsController(
            IStatisticsService statisticsService,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _statisticsService = statisticsService;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>
        /// 加载当前登录用户的服务起始日期。
        /// JWT Claims 中不包含 StartDate，需要从数据库取，避免使用 default(DateTime)
        /// 触发 SQL Server datetime 越界 (0001-01-01)。
        /// </summary>
        private async Task<(int userId, string userName, DateTime startDate)> LoadUserContextAsync()
        {
            var userId = RequireUserId();
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("用户不存在");

            var startDate = user.StartDate == default
                ? DateTime.Today.AddYears(-1)
                : user.StartDate.Date;

            return (userId, user.Name, startDate);
        }

        /// <summary>本周及总学习单词数量</summary>
        [HttpGet("QueryLearnCount")]
        [Authorize]
        public async Task<IActionResult> QueryLearnCount()
        {
            var (userId, userName, startDate) = await LoadUserContextAsync();
            var (weekCount, totalCount, _) = await _statisticsService.QueryLearnCountAsync(userId, startDate);

            return Ok(new { success = true, bzcount = weekCount, count = totalCount, username = userName });
        }

        /// <summary>按月份统计学习情况</summary>
        [HttpGet("StatisticsLearnCountTwo")]
        [Authorize]
        public async Task<IActionResult> StatisticsLearnCountTwo()
        {
            var (userId, _, startDate) = await LoadUserContextAsync();
            var result = await _statisticsService.GetMonthlyStatisticsAsync(userId, startDate);

            var categorys = result.Select(g => new
            {
                date = g.Date,
                totalcount = g.TotalCount,
                statisticsLearns = g.StatisticsLearns.Select(s => new { date = s.Date, count = s.Count }).ToList(),
                task = new
                {
                    id = g.Task.Id,
                    userid = g.Task.UserId,
                    startdate = g.Task.StartDate,
                    count = g.Task.Count,
                    weekend = g.Task.Weekend
                }
            }).ToList();

            return Ok(new { success = true, categorys });
        }

        /// <summary>保存/更新月度学习任务</summary>
        [HttpPost("SaveLearntask")]
        [Authorize]
        public async Task<IActionResult> SaveLearntask(int id, int count, DateTime date, int type, int weekend)
        {
            var userId = RequireUserId();
            await _statisticsService.SaveTaskAsync(userId, count, weekend, date);
            return Ok(new { msg = "操作成功", success = true });
        }
    }
}
