using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed class CommunityKeywordNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICommunityKeywordNotificationSignal _signal;
    private readonly IOptionsMonitor<CommunityKeywordNotificationOptions> _options;
    private readonly ILogger<CommunityKeywordNotificationWorker> _logger;

    public CommunityKeywordNotificationWorker(
        IServiceScopeFactory scopeFactory,
        ICommunityKeywordNotificationSignal signal,
        IOptionsMonitor<CommunityKeywordNotificationOptions> options,
        ILogger<CommunityKeywordNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _signal = signal;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            var reachedBatchLimit = false;
            if (options.Enabled)
            {
                try
                {
                    reachedBatchLimit = await ProcessBatchAsync(options, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "커뮤니티 키워드 알림 작업 처리 중 예외가 발생했습니다.");
                }
            }

            if (reachedBatchLimit)
            {
                await Task.Yield();
                continue;
            }

            await _signal.WaitAsync(
                TimeSpan.FromSeconds(Math.Max(5, options.PollingIntervalSeconds)),
                stoppingToken);
        }
    }

    private async Task<bool> ProcessBatchAsync(
        CommunityKeywordNotificationOptions options,
        CancellationToken cancellationToken)
    {
        var batchSize = Math.Clamp(options.BatchSize, 1, 200);
        var scans = 0;
        for (; scans < batchSize; scans++)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<ICommunityKeywordNotificationProcessor>();
            if (!await processor.ProcessNextScanAsync(cancellationToken))
            {
                break;
            }
        }

        var deliveries = 0;
        for (; deliveries < batchSize; deliveries++)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<ICommunityKeywordNotificationProcessor>();
            if (!await processor.ProcessNextDeliveryAsync(cancellationToken))
            {
                break;
            }
        }

        return scans == batchSize || deliveries == batchSize;
    }
}
