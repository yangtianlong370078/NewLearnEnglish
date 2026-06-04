namespace LearnEnglish.Application.Interfaces
{
    /// <summary>
    /// 学习统计数据版本号服务。
    /// 当用户的统计数据（MyLearn / LearnTask / 缓存回填修正）发生任何变化时，
    /// 调用 <see cref="BumpAsync"/> 让版本号变更；
    /// 读接口通过 <see cref="GetAsync"/> 构造 ETag 实现 HTTP 304 协商缓存。
    /// </summary>
    public interface IStatisticsVersionService
    {
        /// <summary>
        /// 获取当前用户指定缓存类型的版本号；若不存在则懒初始化一个。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <param name="suffix">可选缓存类型后缀，如 "mylearn"、"learnTask"；
        /// 为 null 时读取基础 key。</param>
        Task<string> GetAsync(int userId, string? suffix = null);

        /// <summary>
        /// 提升用户统计版本号。
        /// 若指定 <paramref name="suffix"/>，仅更新 key+suffix 对应的缓存版本；
        /// 若不指定，则删除该用户所有统计版本缓存（前缀匹配）。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <param name="suffix">可选缓存类型后缀；为 null 时清除全部。</param>
        Task BumpAsync(int userId, string? suffix = null);
    }
}
