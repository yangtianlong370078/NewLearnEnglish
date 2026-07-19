using LearnEnglish.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace LearnEnglish.Infrastructure.BackgroundServices
{
    /// <summary>
    /// 在应用启动时创建音节服务，确保 CMU 词典不会在首个请求期间加载。
    /// </summary>
    public sealed class SyllableDictionaryInitializer : IHostedService
    {
        public SyllableDictionaryInitializer(ISyllableService syllableService)
        {
            _ = syllableService;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
