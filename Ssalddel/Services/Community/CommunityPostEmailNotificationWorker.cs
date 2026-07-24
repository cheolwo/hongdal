using Microsoft.Extensions.Options;
using Ssalddel.Domain.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed class CommunityPostEmailNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<CommunityPostEmailNotificationOptions> _options;
    private readonly ILogger<CommunityPostEmailNotificationWorker> _logger;

    public CommunityPostEmailNotificationWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<CommunityPostEmailNotificationOptions> options,
        ILogger<CommunityPostEmailNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = _options.CurrentValue.Enabled
                && await ProcessNextAsync(stoppingToken);
            if (processed)
            {
                continue;
            }

            var pollSeconds = Math.Clamp(_options.CurrentValue.PollIntervalSeconds, 1, 60);
            await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
        }
    }

    internal async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICommunityPostEmailNotificationOutboxStore>();
        var processor = scope.ServiceProvider.GetRequiredService<ICommunityPostEmailNotificationProcessor>();
        var work = await store.ClaimNextAsync(TimeSpan.FromMinutes(2), cancellationToken);
        if (work is null)
        {
            return false;
        }

        try
        {
            var result = await processor.ProcessAsync(work.PostId, cancellationToken);
            switch (result.Status)
            {
                case CommunityPostEmailNotificationProcessStatus.Sent:
                    await store.CompleteAsync(
                        work,
                        CommunityPostEmailNotificationOutboxStatuses.Sent,
                        null,
                        cancellationToken);
                    _logger.LogInformation(
                        "게시글 {PostId} Gmail 알림을 발송했습니다. OutboxId={OutboxId}",
                        work.PostId,
                        work.OutboxId);
                    break;
                case CommunityPostEmailNotificationProcessStatus.Skipped:
                    await store.CompleteAsync(
                        work,
                        CommunityPostEmailNotificationOutboxStatuses.Skipped,
                        result.Detail,
                        cancellationToken);
                    break;
                case CommunityPostEmailNotificationProcessStatus.ConfigurationRequired:
                    await store.CompleteAsync(
                        work,
                        CommunityPostEmailNotificationOutboxStatuses.ConfigurationRequired,
                        result.Detail,
                        cancellationToken);
                    break;
                default:
                    await RetryOrFailAsync(store, work, result.Detail, null, cancellationToken);
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await RetryOrFailAsync(store, work, exception.Message, exception, cancellationToken);
        }

        return true;
    }

    private async Task RetryOrFailAsync(
        ICommunityPostEmailNotificationOutboxStore store,
        CommunityPostEmailNotificationOutboxWork work,
        string? detail,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        var maxAttempts = Math.Clamp(options.MaxAttempts, 1, 10);
        if (work.Attempt >= maxAttempts)
        {
            await store.CompleteAsync(
                work,
                CommunityPostEmailNotificationOutboxStatuses.Failed,
                detail,
                cancellationToken);
            _logger.LogError(
                exception,
                "게시글 {PostId} Gmail 알림이 {AttemptCount}회 실패했습니다. OutboxId={OutboxId}",
                work.PostId,
                work.Attempt,
                work.OutboxId);
            return;
        }

        var retryDelaySeconds = Math.Clamp(options.RetryDelaySeconds, 1, 3600);
        await store.RetryAsync(
            work,
            DateTime.UtcNow.AddSeconds(retryDelaySeconds),
            detail,
            cancellationToken);
        _logger.LogWarning(
            exception,
            "게시글 {PostId} Gmail 알림을 DB outbox에서 재시도합니다. Attempt={Attempt}/{MaxAttempts}",
            work.PostId,
            work.Attempt,
            maxAttempts);
    }
}
