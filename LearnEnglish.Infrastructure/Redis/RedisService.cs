using System.Text.Json;
using LearnEnglish.Redis;

namespace LearnEnglish.Infrastructure.Redis
{
    /// <summary>
    /// Redis 服务实现，封装 RedisConfig 并提供全异步接口
    /// </summary>
    public class RedisService : IRedisService
    {
        private readonly RedisConfig _redisConfig;

        public RedisService(RedisConfig redisConfig)
        {
            _redisConfig = redisConfig;
        }

        public Task<bool> SetAsync(string key, string value)
        {
            var result = _redisConfig.Set(key, value);
            return Task.FromResult(result);
        }

        public Task<bool> SetAsync(string key, string value, TimeSpan expireTime)
        {
            var result = _redisConfig.Set(key, value, expireTime);
            return Task.FromResult(result);
        }

        public Task<string?> GetAsync(string key)
        {
            var result = _redisConfig.Get(key);
            return Task.FromResult(result.IsNullOrEmpty ? null : (string?)result.ToString());
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            var result = _redisConfig.Get(key);
            if (result.IsNullOrEmpty)
                return Task.FromResult<T?>(null);

            try
            {
                var obj = JsonSerializer.Deserialize<T>(result.ToString());
                return Task.FromResult(obj);
            }
            catch
            {
                return Task.FromResult<T?>(null);
            }
        }

        public Task<bool> RemoveAsync(string key)
        {
            var result = _redisConfig.Remove(key);
            return Task.FromResult(result);
        }

        public Task<bool> ExpireAsync(string key, TimeSpan time)
        {
            var result = _redisConfig.Expire(key, time);
            return Task.FromResult(result);
        }

        public Task HashSetAsync(string key, string field, string value)
        {
            _redisConfig.SaveHashSet(key, field, value);
            return Task.CompletedTask;
        }

        public Task<string?> HashGetAsync(string key, string field)
        {
            var result = _redisConfig.GetHash(key, field);
            return Task.FromResult(result.IsNullOrEmpty ? null : (string?)result.ToString());
        }

        public Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            var entries = _redisConfig.GetAllHash(key);
            var dict = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                dict[entry.Name.ToString()] = entry.Value.ToString();
            }
            return Task.FromResult(dict);
        }

        public Task<bool> HashDeleteAsync(string key, string field)
        {
            var result = _redisConfig.DeleteHash(key, field);
            return Task.FromResult(result);
        }

        /// <summary>
        /// 判断哈希字段是否存在 —— 修复原始 RedisConfig 中未实现的 Bug
        /// 使用 HashExists（底层 HEXISTS 命令）而非 HashGet
        /// </summary>
        public Task<bool> HashExistsAsync(string key, string field)
        {
            var db = _redisConfig.GetDatabase();
            var exists = db.HashExists(key, field);
            return Task.FromResult(exists);
        }

        public Task<long> IncrementAsync(string key, long value = 1)
        {
            var result = _redisConfig.Incr(key, value);
            return Task.FromResult(result);
        }

        public Task<long> DecrementAsync(string key, long value = 1)
        {
            var db = _redisConfig.GetDatabase();
            var result = db.StringDecrement(key, value);
            return Task.FromResult(result);
        }

        public Task<bool> TryGetLockAsync(string key, string value, TimeSpan expire)
        {
            var result = _redisConfig.TryGetLock(key, value, expire);
            return Task.FromResult(result);
        }

        public Task<bool> LockReleaseAsync(string key, string value)
        {
            var result = _redisConfig.LockRelease(key, value);
            return Task.FromResult(result);
        }
    }
}
