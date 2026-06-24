using LearnEnglish.Infrastructure.MongoDB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearnEnglish.Infrastructure.BackgroundServices;

/// <summary>
/// 应用启动时执行一次的 MongoDB 索引初始化服务
/// </summary>
public class MongoIndexInitializer : IHostedService
{
    private readonly ILexiconDetailRepository _lexiconDetailRepository;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(
        ILexiconDetailRepository lexiconDetailRepository,
        ILogger<MongoIndexInitializer> logger)
    {
        _lexiconDetailRepository = lexiconDetailRepository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _lexiconDetailRepository.EnsureIndexesAsync(cancellationToken);
            _logger.LogInformation("MongoDB indexes ensured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure MongoDB indexes");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
