using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed class 커뮤니티게시글음성Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly I커뮤니티게시글음성작업신호 _작업신호;
    private readonly IOptionsMonitor<CommunityPostAudioOptions> _options;
    private readonly ILogger<커뮤니티게시글음성Worker> _logger;

    public 커뮤니티게시글음성Worker(
        IServiceScopeFactory scopeFactory,
        I커뮤니티게시글음성작업신호 작업신호,
        IOptionsMonitor<CommunityPostAudioOptions> options,
        ILogger<커뮤니티게시글음성Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _작업신호 = 작업신호;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            if (options.Enabled)
            {
                try
                {
                    await ProcessBatchAsync(options, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "커뮤니티 게시글 음성 작업 처리 중 예외가 발생했습니다.");
                }
            }

            await _작업신호.대기Async(
                TimeSpan.FromSeconds(Math.Max(5, options.PollingIntervalSeconds)),
                stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(
        CommunityPostAudioOptions options,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < Math.Clamp(options.BatchSize, 1, 100); index++)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<I커뮤니티게시글음성작업Processor>();
            if (!await processor.다음작업처리Async(cancellationToken))
            {
                return;
            }
        }
    }
}
