using LearnEnglish.Application.Interfaces;
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

        public StatisticsController(IStatisticsService statisticsService, ICurrentUserService currentUserService)
        {
            _statisticsService = statisticsService;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>本周及总学习单词数量</summary>
        [HttpGet("QueryLearnCount")]
        [Authorize]
        public async Task<IActionResult> QueryLearnCount()
        {
            var userId = RequireUserId();
            var user = _currentUserService.GetValidUser();
            var startDate = user.StartDate.Date;
            var (weekCount, totalCount, _) = await _statisticsService.QueryLearnCountAsync(userId, startDate);

            return Ok(new { success = true, bzcount = weekCount, count = totalCount, username = user.Name });
        }

        /// <summary>按月份统计学习情况</summary>
        [HttpGet("StatisticsLearnCountTwo")]
        [Authorize]
        public async Task<IActionResult> StatisticsLearnCountTwo()
        {
            var userId = RequireUserId();
            var user = _currentUserService.GetValidUser();
            var startDate = user.StartDate.Date;
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
