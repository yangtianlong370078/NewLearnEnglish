using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Redis;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 基于 Redis 的统计数据版本号服务（ETag 协商缓存方案 A）。
    /// 每个用户一个 key：<c>stats:ver:user_{userId}</c>，值为 Unix 毫秒时间戳字符串。
    /// </summary>
    public class StatisticsVersionService : IStatisticsVersionService
    {
        private readonly IRedisService _redis;

        private static string Key(int userId) => $"stats:ver:user_{userId}";

        public StatisticsVersionService(IRedisService redis)
        {
            _redis = redis;
        }

        /// <inheritdoc/>
        public async Task<string> GetAsync(int userId)
        {
            var key = Key(userId);
            var v = await _redis.GetAsync(key);
            if (!string.IsNullOrEmpty(v)) return v;

            // 懒初始化，避免首次读到 null
            var init = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            await _redis.SetAsync(key, init);
            return init;
        }

        /// <inheritdoc/>
        public async Task BumpAsync(int userId)
        {
            var v = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            await _redis.SetAsync(Key(userId), v);
        }
    }
}
