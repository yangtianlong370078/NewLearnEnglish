using LearnEnglish.Application.Interfaces;
using LearnEnglish.Models.Dtos;
using LearnEnglish.Models.ErrorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnEnglish.Controllers
{
    /// <summary>
    /// 学习统计控制器
    /// </summary>
    public class StatisticsController : BaseController
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

        /// <summary>
        /// 查询本周及总学习单词数量
        /// </summary>
        [Authorize]
        public async Task<JsonResult> QueryLearnCount()
        {
            var userId = RequireUserId();
            var user = _currentUserService.GetValidUser();
            var startDate = user.StartDate.Date;
            var (weekCount, totalCount, _) = await _statisticsService.QueryLearnCountAsync(userId, startDate);

            return Json(new { success = true, bzcount = weekCount, count = totalCount, username = user.Name });
        }

        /// <summary>
        /// 按月份统计学习情况
        /// </summary>
        [Authorize]
        public async Task<JsonResult> StatisticsLearnCountTwo()
        {
            var userId = RequireUserId();
            var user = _currentUserService.GetValidUser();
            var startDate = user.StartDate.Date;
            var result = await _statisticsService.GetMonthlyStatisticsAsync(userId, startDate);

            // 映射为旧模型（视图兼容：属性名小写）
            var categorys = result.Select(g => new StatisticsLearnGroup
            {
                date = g.Date,
                totalcount = g.TotalCount,
                statisticsLearns = g.StatisticsLearns.Select(s => new StatisticsLearn
                {
                    date = s.Date,
                    count = s.Count
                }).ToList(),
                task = new LearnTask
                {
                    id = g.Task.Id,
                    userid = g.Task.UserId,
                    startdate = g.Task.StartDate,
                    count = g.Task.Count,
                    weekend = g.Task.Weekend
                }
            }).ToList();

            return Json(new { success = true, categorys });
        }

        /// <summary>
        /// 保存/更新月度学习任务
        /// </summary>
        [Authorize]
        public async Task<JsonResult> SaveLearntask(int id, int count, DateTime date, int type, int weekend)
        {
            var userId = RequireUserId();
            await _statisticsService.SaveTaskAsync(userId, count, weekend, date);
            return Json(new { msg = "操作成功", success = true });
        }
    }
}
