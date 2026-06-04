using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearnEnglish.Infrastructure.BackgroundServices;

/// <summary>
/// 后台任务处理服务
/// </summary>
public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task queue service started");

        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                if (workItem != null)
                {
                    try
                    {
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing background work item");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in background task queue service");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task queue service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
