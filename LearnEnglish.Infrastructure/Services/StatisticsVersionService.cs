using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Redis;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 基于 Redis 的统计数据版本号服务（ETag 协商缓存方案 A）。
    /// 基础前缀：<c>stats:ver:user_{userId}</c>。
    /// 带后缀：<c>stats:ver:user_{userId}:{suffix}</c>，用于区分不同缓存类型。
    /// </summary>
    public class StatisticsVersionService : IStatisticsVersionService
    {
        private readonly IRedisService _redis;

        /// <summary>基础前缀 key，也用作无后缀时的独立 key。</summary>
        private static string BaseKey(int userId) => $"stats:ver:user_{userId}";

        /// <summary>带后缀的完整 key。</summary>
        private static string SuffixKey(int userId, string suffix) => $"stats:ver:user_{userId}:{suffix}";

        public StatisticsVersionService(IRedisService redis)
        {
            _redis = redis;
        }

        /// <inheritdoc/>
        public async Task<string> GetAsync(int userId, string? suffix = null)
        {
            var key = string.IsNullOrEmpty(suffix) ? BaseKey(userId) : SuffixKey(userId, suffix);
            var v = await _redis.GetAsync(key);
            if (!string.IsNullOrEmpty(v)) return v;

            // 懒初始化，避免首次读到 null
            var init = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            await _redis.SetAsync(key, init);
            return init;
        }

        /// <inheritdoc/>
        public async Task BumpAsync(int userId, string? suffix = null)
        {
            if (!string.IsNullOrEmpty(suffix))
            {
                // 指定后缀：只更新对应的单个 key
                var v = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                await _redis.SetAsync(SuffixKey(userId, suffix), v);
            }
            else
            {
                // 未指定后缀：删除该用户所有统计版本缓存（前缀匹配）
                await _redis.RemoveByPatternAsync($"{BaseKey(userId)}*");
            }
        }
    }
}
