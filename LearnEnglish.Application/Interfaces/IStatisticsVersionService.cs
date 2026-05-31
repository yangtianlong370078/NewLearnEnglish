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
        /// <summary>获取当前用户的统计版本号；若不存在则懒初始化一个。</summary>
        Task<string> GetAsync(int userId);

        /// <summary>提升用户统计版本号（任意写入路径调用）。</summary>
        Task BumpAsync(int userId);
    }
}
