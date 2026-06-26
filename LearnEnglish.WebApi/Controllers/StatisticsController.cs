using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
        private readonly IStatisticsVersionService _statsVersionService;

        public StatisticsController(
            IStatisticsService statisticsService,
            ICurrentUserService currentUserService,
            IStatisticsVersionService statsVersionService)
        {
            _statisticsService = statisticsService;
            _currentUserService = currentUserService;
            _statsVersionService = statsVersionService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>
        /// 从 JWT Claims 中读取当前登录用户的上下文（userId / userName / startDate）。
        /// StartDate 在登录时已写入 Token，避免每次请求都查询数据库；
        /// 若 Token 中缺失或无效，则回退到一年前，避免 SQL Server datetime 越界。
        /// </summary>
        private (int userId, string userName, DateTime startDate) LoadUserContext()
        {
            var userId = RequireUserId();
            var userName = _currentUserService.UserName ?? string.Empty;
            var startDate = _currentUserService.StartDate is { } sd && sd != default
                ? sd.Date
                : DateTime.Today.AddYears(-1);

            return (userId, userName, startDate);
        }

        /// <summary>本周及总学习单词数量</summary>
        [HttpGet("QueryLearnCount")]
        [Authorize]
        public async Task<IActionResult> QueryLearnCount()
        {
            var (userId, userName, startDate) = LoadUserContext();
            var (weekCount, totalCount, _) = await _statisticsService.QueryLearnCountAsync(userId, startDate);

            return Ok(new { success = true, bzcount = weekCount, count = totalCount, username = userName });
        }

        /// <summary>按月份统计学习情况</summary>
        /// <remarks>
        /// 响应通过 ETag + Cache-Control: private, no-cache 实现协商缓存。
        /// 用户的统计数据未变化时浏览器会自动带 If-None-Match 重新校验，
        /// 服务端返回 304（无 body），浏览器从本地复用上一次的响应体。
        /// </remarks>
        [HttpGet("StatisticsLearnCountTwo")]
        [Authorize]
        public async Task<IActionResult> StatisticsLearnCountTwo()
        {
            var (userId, _, startDate) = LoadUserContext();

            // 1. 拿到当前用户的统计版本号，构造 ETag
            var version = await _statsVersionService.GetAsync(userId, "LearnCount");
            var etag = new EntityTagHeaderValue($"\"u{userId}-v{version}\"");

            // 2. 校验 If-None-Match：命中则返回 304，无需查询/序列化数据
            var ifNoneMatch = Request.Headers.IfNoneMatch;
            if (ifNoneMatch.Count > 0)
            {
                foreach (var raw in ifNoneMatch)
                {
                    if (raw is null) continue;
                    if (EntityTagHeaderValue.TryParse(raw, out var incoming)
                        && incoming.Compare(etag, useStrongComparison: false))
                    {
                        Response.Headers.ETag = etag.ToString();
                        Response.Headers.CacheControl = "private, no-cache";
                        return StatusCode(StatusCodes.Status304NotModified);
                    }
                }
            }

            // 3. 未命中缓存，查询并返回精简后的数据
            var result = await _statisticsService.GetMonthlyStatisticsAsync(userId, startDate);

            // 紧凑编码：月份用 yyyyMM，每日明细用 [day, count]，任务字段平铺。
            var data2 = result.Select(g => new
            {
                ym = g.Date.Year * 100 + g.Date.Month,
                total = g.TotalCount,
                taskCnt = g.Task?.Count ?? 0,
                taskWeekend = g.Task?.Weekend ?? 0,
                items = g.StatisticsLearns
                    .OrderBy(s => s.Date)
                    .Select(s => new[] { s.Date.Day, s.Count })
                    .ToList()
            }).ToList();

            Response.Headers.ETag = etag.ToString();
            Response.Headers.CacheControl = "private, no-cache";

            List<object> data = new List<object>();
            for (int i = 0; i < 120; i++)
            {
                var bb = new List<int[]>();

                for (int j = 0; j < 30; j++)
                {
                    bb.Add(new[] { j, 5 });
                }

                data.Add( new
                {
                    ym =202602,
                    total = 120,
                    taskCnt = 150,
                    taskWeekend =3,
                    items = bb
                });
            }


            return Ok(new { success = true, data });
        }


        /// <summary>
        /// 获取学习统计 KPI 数据（掌握数/未熟练/强化中/今日学习/增长率）。
        /// </summary>
        /// <remarks>
        /// 响应通过 ETag + Cache-Control: private, no-cache 实现协商缓存。
        /// ETag 使用 "kpi-u{userId}-v{version}" 前缀，与 StatisticsLearnCountTwo 的
        /// "u{userId}-v{version}" 相互独立，不会产生命中冲突。
        /// </remarks>
        [HttpGet("GetStudyStatistics")]
        [Authorize]
        public async Task<IActionResult> GetStudyStatistics()
        {
            var userId = RequireUserId();

            // 1. 构造 KPI 专用 ETag（加 "kpi-" 前缀与月统计接口区分）
            var version = await _statsVersionService.GetAsync(userId, "StudyStatistics");
            var etag = new EntityTagHeaderValue($"\"kpi-u{userId}-v{version}\"");

            // 2. 校验 If-None-Match：命中则返回 304
            var ifNoneMatch = Request.Headers.IfNoneMatch;
            if (ifNoneMatch.Count > 0)
            {
                foreach (var raw in ifNoneMatch)
                {
                    if (raw is null) continue;
                    if (EntityTagHeaderValue.TryParse(raw, out var incoming)
                        && incoming.Compare(etag, useStrongComparison: false))
                    {
                        Response.Headers.ETag = etag.ToString();
                        Response.Headers.CacheControl = "private, no-cache";
                        return StatusCode(StatusCodes.Status304NotModified);
                    }
                }
            }

            // 3. 未命中缓存，查询并返回
            var studyStatistics = await _statisticsService.GetStudyStatistics(userId);

            Response.Headers.ETag = etag.ToString();
            Response.Headers.CacheControl = "private, no-cache";
            return Ok(new { success = true, studyStatistics });
        }



            /// <summary>保存/更新月度学习任务</summary>
            ///weekend:0不休息，1周六休息2周日休息3，周六周日都休息
            [HttpPost("SaveLearntask")]
        [Authorize]
        public async Task<IActionResult> SaveLearntask(int id, int count, DateTime date, int weekend)
        {
            var userId = RequireUserId();
            await _statisticsService.SaveTaskAsync(userId, count, weekend, date);
            return Ok(new { msg = "操作成功", success = true });
        }
    }
}
