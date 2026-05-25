using Microsoft.Extensions.DependencyInjection;

namespace LearnEnglish.Application
{
    /// <summary>
    /// Application 层依赖注入扩展方法
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册 Application 层所有服务
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Service 实现注册将在阶段4实现后添加

            return services;
        }
    }
}
