using LearnEnglish.Application.Dtos.Statistics;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 学习统计服务实现
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IMyLearnRepository _myLearnRepository;
        private readonly ILearnTaskRepository _learnTaskRepository;
        private readonly IRedisService _redisService;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IMyLearnRepository myLearnRepository,
            ILearnTaskRepository learnTaskRepository,
            IRedisService redisService,
            ILogger<StatisticsService> logger)
        {
            _myLearnRepository = myLearnRepository;
            _learnTaskRepository = learnTaskRepository;
            _redisService = redisService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<(int weekCount, int totalCount, string userName)> QueryLearnCountAsync(int userId, DateTime startDate)
        {
            var key = $"categorys:user_{userId}";
            var cachedStats = await GetCachedStatisticsAsync(key);

            var weekStart = GetStartOfThisWeek();
            var weekCount = await _myLearnRepository.GetCountByUserIdSinceDateAsync(userId, weekStart);

            // 获取最新的缓存日期
            var queryDate = startDate.Date;
            if (cachedStats.Count > 0)
            {
                queryDate = cachedStats.Max(a => a.Date);
            }

            var dbCount = await _myLearnRepository.GetCountByUserIdSinceDateAsync(userId, queryDate);
            var totalCount = dbCount + cachedStats.Sum(a => a.Count);

            return (weekCount, totalCount, string.Empty);
        }

        /// <inheritdoc/>
        public async Task<List<StatisticsLearnGroupDto>> GetMonthlyStatisticsAsync(int userId, DateTime startDate)
        {
            var key = $"categorys:user_{userId}";
            var cachedStats = await GetCachedStatisticsAsync(key);

            // 确定从数据库中查询的起始日期
            var queryDate = startDate.Date;
            if (cachedStats.Count > 0)
            {
                queryDate = cachedStats.Max(a => a.Date);
            }

            // 从数据库获取学习记录
            var dbStats = await _myLearnRepository.GetDailyCountByUserIdAsync(userId, queryDate);
            var dbStatsList = dbStats.Select(x => new StatisticsLearnDto
            {
                Date = x.Date,
                Count = x.Count
            }).ToList();

            // 合并：数据库数据优先，缓存非当天数据
            var nowDate = DateTime.Now.Date;
            if (dbStatsList.Count > 0)
            {
                var toCache = dbStatsList.Where(a => a.Date < nowDate).ToList();
                if (toCache.Count > 0)
                {
                    // 合并到缓存
                    var merged = toCache.ToDictionary(t => t.Date)
                        .Union(cachedStats.ToDictionary(t => t.Date))
                        .Select(kv => kv.Value)
                        .ToList();

                    // 异步写入 Redis
                    _ = Task.Run(async () =>
                    {
                        foreach (var item in merged)
                        {
                            var field = item.Date.ToString("yyyy-MM-dd");
                            var json = JsonConvert.SerializeObject(item);
                            await _redisService.HashSetAsync(key, field, json);
                        }
                    });
                }
            }

            // 合并所有数据
            var allStats = new List<StatisticsLearnDto>(cachedStats);
            allStats.AddRange(dbStatsList);
            // 去重：以日期为Key，取最新值
            allStats = allStats.GroupBy(x => x.Date).Select(g => g.Last()).ToList();

            // 获取学习任务
            var tasks = await _learnTaskRepository.GetByUserIdSinceDateAsync(userId, startDate);

            // 按月分组
            var months1 = allStats.Select(a => a.Date.ToString("yyyy-MM")).ToList();
            var months2 = tasks.Select(a => a.StartDate.ToString("yyyy-MM")).ToList();
            var allMonths = months1.Union(months2).Distinct().OrderBy(m => m).ToList();

            return allMonths.Select(month =>
            {
                var monthDate = DateTime.Parse(month + "-01");
                var monthStats = allStats.Where(a => a.Date.ToString("yyyy-MM") == month).ToList();
                var task = tasks.FirstOrDefault(a => a.StartDate.ToString("yyyy-MM") == month);

                return new StatisticsLearnGroupDto
                {
                    Date = monthDate,
                    StatisticsLearns = monthStats,
                    TotalCount = monthStats.Sum(a => a.Count),
                    Task = task != null ? new LearnTaskDto
                    {
                        Id = task.Id,
                        UserId = task.UserId,
                        StartDate = task.StartDate,
                        Count = task.Count,
                        Weekend = task.Weekend
                    } : new LearnTaskDto()
                };
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task SaveTaskAsync(int userId, int count, int weekend, DateTime date)
        {
            var existing = await _learnTaskRepository.GetByUserIdAndMonthAsync(userId, date.Year, date.Month);

            if (existing != null)
            {
                existing.Count = count;
                existing.Weekend = weekend;
                await _learnTaskRepository.UpdateAsync(existing);
            }
            else
            {
                var firstDay = new DateTime(date.Year, date.Month, 1);
                var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                await _learnTaskRepository.CreateAsync(new LearnTask
                {
                    UserId = userId,
                    Count = count,
                    StartDate = firstDay,
                    EndDate = lastDay,
                    Weekend = weekend
                });
            }
        }

        /// <summary>
        /// 从 Redis 获取缓存的学习统计
        /// </summary>
        private async Task<List<StatisticsLearnDto>> GetCachedStatisticsAsync(string key)
        {
            var result = new List<StatisticsLearnDto>();
            var entries = await _redisService.HashGetAllAsync(key);

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var data = JsonConvert.DeserializeObject<StatisticsLearnDto>(entry.Value.ToString()!);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
            }

            return result.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// 获取本周一日期
        /// </summary>
        private static DateTime GetStartOfThisWeek()
        {
            var today = DateTime.Today;
            var daysDiff = (int)today.DayOfWeek;
            if (daysDiff == 0) daysDiff = 7;
            return today.AddDays(-daysDiff + 1);
        }
    }
}
