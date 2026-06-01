using System.Globalization;
using System.Text.Json;

using LearnEnglish.Application.Dtos.Statistics;
using LearnEnglish.Application.Dtos.Word;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Common;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.MongoDB;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 单词学习服务实现
    /// </summary>
    public class WordService : IWordService
    {
        private readonly IWordQueryRepository _wordQueryRepository;
        private readonly ILexiconRepository _lexiconRepository;
        private readonly IMyLexiconRepository _myLexiconRepository;
        private readonly IMyLearnRepository _myLearnRepository;
        private readonly ILearnRepository _learnRepository;
        private readonly ICourseContentRepository _courseContentRepository;
        private readonly ILexiconDetailRepository _lexiconDetailRepository;
        private readonly IRedisService _redisService;
        private readonly ITranslateService _translateService;
        private readonly IStatisticsVersionService _statsVersionService;
        private readonly ILogger<WordService> _logger;

        // Redis 中单词详情的 Hash Key 前缀
        private const string WordDetailRedisKey = "word:";

        public WordService(
            IWordQueryRepository wordQueryRepository,
            ILexiconRepository lexiconRepository,
            IMyLexiconRepository myLexiconRepository,
            IMyLearnRepository myLearnRepository,
            ILearnRepository learnRepository,
            ICourseContentRepository courseContentRepository,
            ILexiconDetailRepository lexiconDetailRepository,
            IRedisService redisService,
            ITranslateService translateService,
            IStatisticsVersionService statsVersionService,
            ILogger<WordService> logger)
        {
            _wordQueryRepository = wordQueryRepository;
            _lexiconRepository = lexiconRepository;
            _myLexiconRepository = myLexiconRepository;
            _myLearnRepository = myLearnRepository;
            _learnRepository = learnRepository;
            _courseContentRepository = courseContentRepository;
            _lexiconDetailRepository = lexiconDetailRepository;
            _redisService = redisService;
            _translateService = translateService;
            _statsVersionService = statsVersionService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<(bool exists, int isEnAudio, int isUsAudio)> WordExistAsync(int courseId, string en)
        {
            return await _wordQueryRepository.CheckWordExistsAsync(courseId, en);
        }

        /// <inheritdoc/>
        public async Task<(PagedList<ShowTranslateDto> pagedList, int newCount, int learningCount, int masteredCount)> GetWordListAsync(
            int userId, int courseId, int status, int displayType, string? name, int pageIndex, int pageSize)
        {
            var orderBy = status > 1 ? "t4.updatetime desc" : "t2.frequency desc, t1.id desc";

            var (items, total, newCount, learningCount, masteredCount) =
                await _wordQueryRepository.GetCourseWordsPagedAsync(userId, courseId, status, name, pageIndex, pageSize, orderBy);

            var list = items.ToList();
            var textInfo = CultureInfo.CurrentCulture.TextInfo;

            foreach (var item in list)
            {
                var en = textInfo.ToTitleCase(item.En.ToLower());
                item.En = en;
                item.Name = displayType == 1 ? item.Cn : en;
                item.Value = displayType == 1 ? en : (displayType == 3 ? en : item.Cn);
                item.IsUpdate = item.UserId == userId;
            }

            return (new PagedList<ShowTranslateDto>(list, pageIndex, pageSize, total), newCount, learningCount, masteredCount);
        }

        /// <inheritdoc/>
        public async Task<PagedList<ShowTranslateDto>> GetFavoriteListAsync(
            int userId, int displayType, string? name, int pageIndex, int pageSize)
        {
            var (items, total) = await _wordQueryRepository.GetFavoriteWordsPagedAsync(userId, name, pageIndex, pageSize);

            var list = items.ToList();
            var textInfo = CultureInfo.CurrentCulture.TextInfo;

            foreach (var item in list)
            {
                var en = textInfo.ToTitleCase(item.En.ToLower());
                item.En = en;
                item.Name = displayType == 1 ? item.Cn : en;
                item.Value = displayType == 1 ? en : (displayType == 3 ? en : item.Cn);
                item.IsUpdate = item.UserId == userId;
            }

            return new PagedList<ShowTranslateDto>(list, pageIndex, pageSize, total);
        }

        /// <inheritdoc/>
        public async Task<(int changed, int added, int removed)> CalibrateNewAsync(int userId)
        {
            // 获取需要变更状态的数据
            var changes = (await _myLexiconRepository.GetCalibrationChangesAsync(userId)).ToList();
            if (changes.Count == 0)
                return (0, 0, 0);

            // 批量更新 mylexicon 状态
            await _myLexiconRepository.ApplyCalibrationAsync(userId,
                changes.Select(c => (c.LexiconId, c.NewStatus)));

            // 处理新掌握的（newStatus=3）→ 插入 mylearn
            var mastered = changes.Where(c => c.NewStatus == 3).ToList();
            if (mastered.Count > 0)
            {
                await _myLearnRepository.BulkInsertIgnoreAsync(mastered.Select(c => new MyLearn
                {
                    LexiconId = c.LexiconId,
                    UserId = userId,
                    UpdateTime = DateTime.Now
                }));

                // 让统计 ETag 失效
                await _statsVersionService.BumpAsync(userId);
            }

            // 处理被降级的（oldStatus=3）→ 删除 mylearn + 更新 Redis 缓存
            var demoted = changes.Where(c => c.OldStatus == 3 && c.NewStatus != 3).ToList();
            if (demoted.Count > 0)
            {
                await _myLearnRepository.BulkDeleteByUserAndLexiconIdsAsync(userId,
                    demoted.Select(c => c.LexiconId));

                // 更新 Redis 统计缓存
                await UpdateStatisticsCacheAsync(userId, demoted.Select(c => c.LexiconId));
            }

            return (changes.Count, mastered.Count, demoted.Count);
        }

        /// <inheritdoc/>
        public async Task SetWordStatusAsync(int userId, int lexiconId, int status)
        {
            await _myLexiconRepository.UpsertOrInsertNumberAsync(userId, lexiconId, status);

            var keys = new List<(DateTime Date, int Count)>();

            // 处理 mylearn 记录
            if (status == 3)
            {
                await _myLearnRepository.InsertIgnoreAsync(new MyLearn
                {
                    LexiconId = lexiconId,
                    UserId = userId,
                    UpdateTime = DateTime.Now
                });

                keys.Add((DateTime.Now.Date, 0));
            }
            else
            {
                var update = await _myLearnRepository.QueryByUserAndLexiconAsync(userId, lexiconId);

                if (update != null)
                {
                    keys.Add((update.Value.Date, 0));
                }

                await _myLearnRepository.DeleteByUserAndLexiconAsync(userId, lexiconId);
            }

            if (keys.Count > 0)
            {
                var result = (await _myLearnRepository.GetDailyCountByUserIdAndDatesAsync(userId, keys.Select(a => a.Date))).ToList();

                if (result.Count > 0)
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        var match = result.FirstOrDefault(a => a.Date == keys[i].Date);
                        keys[i] = (keys[i].Date, match.Count);
                    }
                }

                // 更新 Redis 统计缓存
                await UpdateStatisticsCacheAsync(userId, keys);
            }
        }

        /// <inheritdoc/>
        public async Task ModifyNumberAsync(int userId, string jsonData)
        {
            var items = JsonConvert.DeserializeObject<List<NumberUpdateItem>>(jsonData);
            if (items == null || items.Count == 0) return;

            // 按 type 分组批量处理
            var grouped = items.GroupBy(i => i.Type);
            foreach (var group in grouped)
            {
                var fieldName = group.Key switch
                {
                    1 => "zynumber",
                    2 => "yznumber",
                    3 => "txnumber",
                    4 => "fynumber",
                    _ => "zynumber"
                };

                var dict = group.ToDictionary(i => i.Id, i => i.No);
                await _myLexiconRepository.BatchUpsertNumbersAsync(userId, dict, fieldName);
            }
        }

        /// <inheritdoc/>
        public async Task EditWordAsync(int userId, int lexiconId, string en, string cn)
        {
            // 更新 lexicon 表
            await _lexiconRepository.UpdateEnCnAsync(lexiconId, userId, en, cn);

            // 更新或插入 mylexicon.cn
            var myLexicon = await _myLexiconRepository.GetByUserAndLexiconAsync(userId, lexiconId);
            if (myLexicon == null)
            {
                await _myLexiconRepository.CreateOrUpdateAsync(new MyLexicon
                {
                    UserId = userId,
                    LexiconId = lexiconId,
                    Cn = cn
                });
            }
            else if (myLexicon.Cn != cn)
            {
                await _myLexiconRepository.UpdateCnAsync(userId, lexiconId, cn);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteWordAsync(int userId, int courseContentId)
        {
            await _learnRepository.DeleteByCourseContentIdAsync(courseContentId);
            await _courseContentRepository.DeleteAsync(courseContentId);
        }

        /// <inheritdoc/>
        public async Task<object?> GetWordDetailAsync(int userId, string word, int courseId)
        {
            // 1. 尝试从 Redis 获取
            //var redisResult = await _redisService.HashGetAsync(WordDetailRedisKey, word);
            var key = $"word:{word.Trim().ToLower()}";
            var redisResult = await _redisService.GetAsync(key);

            if (!string.IsNullOrEmpty(redisResult))
            {
                return ParseWordDetail(redisResult);
            }

            // 2. 尝试从 MongoDB 获取
            var mongoResult = await _lexiconDetailRepository.GetByWordAsync(word);
            if (mongoResult != null)
            {
                // 存入 Redis
                var json = JsonConvert.SerializeObject(mongoResult);
                await _redisService.HashSetAsync(WordDetailRedisKey, word, json);
                return mongoResult;
            }

            // 3. 从外部 API 获取
            var apiResult = await _translateService.QueryWordAsync(word);
            if (apiResult != null)
            {
                // 存入 Redis
                var json = JsonConvert.SerializeObject(apiResult);
                await _redisService.HashSetAsync(WordDetailRedisKey, word, json);

                // 确保单词存在于 lexicon 表中
                var lexicon = await _lexiconRepository.GetByEnAsync(word);
                if (lexicon == null)
                {
                    await _lexiconRepository.CreateAsync(new Lexicon
                    {
                        En = word,
                        Cn = string.Empty,
                        UserId = userId
                    });
                }
            }

            return apiResult;
        }

        /// <inheritdoc/>
        public async Task SetCollectAsync(int userId, int lexiconId, int isCollect)
        {
            await _myLexiconRepository.SetCollectAsync(userId, lexiconId, isCollect);
        }

        /// <summary>
        /// 更新 Redis 统计缓存 真实更新
        /// </summary>
        private async Task UpdateStatisticsCacheAsync(int userId, IEnumerable<(DateTime Date, int Count)> values)
        {
            // 统计缓存更新逻辑：移除今天的缓存，让下次查询时重新计算
            var key = $"categorys:user_{userId}";
            // 让统计 ETag 失效，触发前端拿到 200 + 新数据
            await _statsVersionService.BumpAsync(userId);

            foreach (var item in values)
            {
                var field = item.Date.ToString("yyyy-MM-dd");
                var data = new StatisticsLearnDto
                {
                    Date = item.Date,
                    Count = item.Count
                };
                var json = JsonConvert.SerializeObject(data);
                await _redisService.HashSetAsync(key, field, json);
            }

        }

        /// <summary>
        /// 更新 Redis 统计缓存
        /// </summary>
        private async Task UpdateStatisticsCacheAsync(int userId, IEnumerable<int> lexiconIds)
        {
            // 统计缓存更新逻辑：移除今天的缓存，让下次查询时重新计算
            var key = $"categorys:user_{userId}";

            // 让统计 ETag 失效，触发前端拿到 200 + 新数据
            await _statsVersionService.BumpAsync(userId);

            var today = DateTime.Now.Date.ToString("yyyy-MM-dd");

            await _redisService.HashDeleteAsync(key, today);
        }

        /// <summary>
        /// 解析单词详情 JSON
        /// </summary>
        private static object? ParseWordDetail(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<object>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 批量更新的数据项
        /// </summary>
        private class NumberUpdateItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("no")]
            public int No { get; set; }

            [JsonProperty("type")]
            public int Type { get; set; }
        }

        /// <inheritdoc/>
        public async Task<bool> BatchUpdateNumberAsync(int userId, string jsonData)
        {
            var items = JsonConvert.DeserializeObject<List<UpdcnoItem>>(jsonData);
            if (items == null || !items.Any()) return false;

            // 按类型分组批量 upsert
            foreach (var group in items.GroupBy(a => a.Type))
            {
                var numberField = group.Key;
                // 限制值范围 0-15
                var dict = group.ToDictionary(
                    i => i.Id,
                    i => Math.Max(0, Math.Min(15, i.No))
                );
                await _myLexiconRepository.BatchUpsertNumbersAsync(userId, dict, numberField);
            }

            return true;
        }

        /// <summary>
        /// updcno 批量更新数据项
        /// </summary>
        private class UpdcnoItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("no")]
            public int No { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; } = string.Empty;
        }
    }
}
