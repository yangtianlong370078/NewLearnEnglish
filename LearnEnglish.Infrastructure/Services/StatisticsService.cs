using LearnEnglish.Application.Dtos.Statistics;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;

using Microsoft.Extensions.Logging;

using MongoDB.Bson.IO;

using Newtonsoft.Json;

using SharpCompress.Common;

using static Mysqlx.Expect.Open.Types.Condition.Types;

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
        private readonly IStatisticsVersionService _statsVersionService;
        public StatisticsService(
            IMyLearnRepository myLearnRepository,
            ILearnTaskRepository learnTaskRepository,
            IRedisService redisService,
            ILogger<StatisticsService> logger,
            IStatisticsVersionService statsVersionService)
        {
            _myLearnRepository = myLearnRepository;
            _learnTaskRepository = learnTaskRepository;
            _redisService = redisService;
            _logger = logger;
            _statsVersionService = statsVersionService;
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

        public async Task<StudyStatisticsDto> GetStudyStatistics(int userId)
        {
            var studyKey = $"studystatistics:user_{userId}";
            var statsKey = $"categorys:user_{userId}";

            // 两个不同 Redis key，并行读取，减少一次网络等待
            // GetMinMaxCachedStatAsync 内部用 HKEYS+HMGET 替代 HGETALL，只取首尾两条记录
            var studyTask = GetStudyStatistics(studyKey);
            var minMaxTask = GetMinMaxCachedStatAsync(statsKey);
            await Task.WhenAll(studyTask, minMaxTask);

            var result = studyTask.Result;
            if (result == null)
            {
                result = await _myLearnRepository.GetStudyStatistics(userId);
                // 回填缓存，fire-and-forget，写失败不影响主流程
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                _ = _redisService.HashSetAsync(studyKey, "data", json);
            }

            // 将 min/max 统计信息下发给前端，由前端结合当前日期计算 TodayCount 与 GrowthRate
            var (minStat, maxStat) = minMaxTask.Result;
            if (minStat != null && maxStat != null)
            {
                result.MinDate = minStat.Date.Date;
                result.LastDate = maxStat.Date.Date;
                result.LastCount = maxStat.Count;
            }

            return result;
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
                queryDate = cachedStats.Max(a => a.Date).Date.AddDays(1).AddSeconds(-1);
            }

            var dbStatsList = new List<StatisticsLearnDto>();

            //缓存中没有或者缓存中数据小于今天，更新一次
            if (cachedStats == null || !cachedStats.Any() || queryDate < DateTime.Now.Date)
            {
                // 从数据库获取学习记录
                var dbStats = await _myLearnRepository.GetDailyCountByUserIdAsync(userId, queryDate);
                dbStatsList = dbStats.Select(x => new StatisticsLearnDto
                {
                    Date = x.Date,
                    Count = x.Count
                }).ToList();

                // 合并：数据库数据优先，缓存
                var nowDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
                if (dbStatsList.Count > 0)
                {
                    var toCache = dbStatsList.Where(a => a.Date <= nowDate).ToList();
                    if (toCache.Count > 0)
                    {
                        // 合并到缓存
                        //var merged = toCache.ToDictionary(t => t.Date)
                        //    .Union(cachedStats.ToDictionary(t => t.Date))
                        //    .Select(kv => kv.Value)
                        //    .ToList();


                        //只做增量更新
                        var merged = toCache.ToDictionary(t => t.Date)
                            .Select(kv => kv.Value)
                            .ToList();

                        // 批量写入 Redis（单次 HSET，N 条数据只需 1 次网络往返）
                        var batchFields = merged.ToDictionary(
                            item => item.Date.ToString("yyyy-MM-dd"),
                            item => Newtonsoft.Json.JsonConvert.SerializeObject(item));
                        _ = _redisService.HashSetBatchAsync(key, batchFields);
                    }
                }
            }

            // 合并所有数据
            var allStats = cachedStats ?? new List<StatisticsLearnDto>();
            if(dbStatsList!=null&& dbStatsList.Any())
            {
                allStats.AddRange(dbStatsList);
            }
           
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

            // 让统计 ETag 失效，触发前端拿到 200 + 新数据
            await _statsVersionService.BumpAsync(userId, "LearnCount");

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
        private async Task<StudyStatisticsDto?> GetStudyStatistics(string key)
        {
            var entries = await _redisService.HashGetAllAsync(key);
            if (entries != null && entries.TryGetValue("data", out var json))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<StudyStatisticsDto>(json);
            }

            return null;
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
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<StatisticsLearnDto>(entry.Value.ToString()!);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
            }

            return result.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// 仅读取缓存中日期最小和最大的两条记录
        /// 用 HKEYS（只取字段名）+ HMGET（只取2个值）代替 HGETALL，
        /// 数据量和反序列化开销均大幅降低
        /// </summary>
        private async Task<(StatisticsLearnDto? min, StatisticsLearnDto? max)> GetMinMaxCachedStatAsync(string key)
        {
            var fields = (await _redisService.HashKeysAsync(key)).ToList();
            if (fields.Count == 0) return (null, null);

            // "yyyy-MM-dd" 格式可直接按字符串排序，等价于日期排序
            fields.Sort(StringComparer.Ordinal);
            var minField = fields[0];
            var maxField = fields[fields.Count - 1];

            // 同一 key 的两个 field，HMGET 单次请求取回
            var values = await _redisService.HashMultiGetAsync(key, minField, maxField);

            StatisticsLearnDto? minStat = null, maxStat = null;
            if (values.TryGetValue(minField, out var minJson) && minJson != null)
                minStat = Newtonsoft.Json.JsonConvert.DeserializeObject<StatisticsLearnDto>(minJson);

            // 只有一条记录时 minField == maxField，复用 minStat 即可
            if (minField == maxField)
                return (minStat, minStat);

            if (values.TryGetValue(maxField, out var maxJson) && maxJson != null)
                maxStat = Newtonsoft.Json.JsonConvert.DeserializeObject<StatisticsLearnDto>(maxJson);

            return (minStat, maxStat);
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
