namespace LearnEnglish.Infrastructure.Redis
{
    /// <summary>
    /// Redis 服务接口
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// 设置字符串值
        /// </summary>
        Task<bool> SetAsync(string key, string value);

        /// <summary>
        /// 设置字符串值（带过期时间）
        /// </summary>
        Task<bool> SetAsync(string key, string value, TimeSpan expireTime);

        /// <summary>
        /// 获取字符串值
        /// </summary>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// 获取泛型值
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// 删除键
        /// </summary>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 设置过期时间
        /// </summary>
        Task<bool> ExpireAsync(string key, TimeSpan time);

        /// <summary>
        /// 设置哈希值
        /// </summary>
        Task HashSetAsync(string key, string field, string value);

        /// <summary>
        /// 获取哈希值
        /// </summary>
        Task<string?> HashGetAsync(string key, string field);

        /// <summary>
        /// 获取所有哈希值
        /// </summary>
        Task<Dictionary<string, string>> HashGetAllAsync(string key);

        /// <summary>
        /// 删除哈希字段
        /// </summary>
        Task<bool> HashDeleteAsync(string key, string field);

        /// <summary>
        /// 判断哈希字段是否存在
        /// </summary>
        Task<bool> HashExistsAsync(string key, string field);

        /// <summary>
        /// 自增
        /// </summary>
        Task<long> IncrementAsync(string key, long value = 1);

        /// <summary>
        /// 自减
        /// </summary>
        Task<long> DecrementAsync(string key, long value = 1);

        /// <summary>
        /// 尝试获取分布式锁
        /// </summary>
        Task<bool> TryGetLockAsync(string key, string value, TimeSpan expire);

        /// <summary>
        /// 释放分布式锁
        /// </summary>
        Task<bool> LockReleaseAsync(string key, string value);
    }
}
