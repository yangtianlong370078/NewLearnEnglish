namespace LearnEnglish.Infrastructure.BackgroundServices;

/// <summary>
/// 后台任务队列接口
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 将任务加入队列
    /// </summary>
    /// <param name="workItem">要执行的异步任务</param>
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

    /// <summary>
    /// 从队列取出任务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>要执行的任务，如果队列为空则返回 null</returns>
    ValueTask<Func<CancellationToken, ValueTask>?> DequeueAsync(CancellationToken cancellationToken);
}
