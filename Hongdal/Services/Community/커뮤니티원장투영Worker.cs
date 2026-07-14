using Hongdal.Application.Community.Events;
using MediatR;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.Community;

public sealed class 커뮤니티원장투영Worker : BackgroundService
{
    private readonly I커뮤니티원장투영작업저장소 _작업저장소;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<CommunityLedgerProjectionOptions> _options;
    private readonly ILogger<커뮤니티원장투영Worker> _logger;

    public 커뮤니티원장투영Worker(
        I커뮤니티원장투영작업저장소 작업저장소,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<CommunityLedgerProjectionOptions> options,
        ILogger<커뮤니티원장투영Worker> logger)
    {
        _작업저장소 = 작업저장소;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            var processed = 0;
            try
            {
                if (options.Enabled)
                {
                    processed = await ProcessBatchAsync(options, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "커뮤니티 원장 투영 대기열 처리 중 예외가 발생했습니다.");
            }

            if (processed < Math.Clamp(options.BatchSize, 1, 100))
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(1, options.PollingIntervalSeconds)),
                    stoppingToken);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(
        CommunityLedgerProjectionOptions options,
        CancellationToken cancellationToken)
    {
        var batchSize = Math.Clamp(options.BatchSize, 1, 100);
        var processed = 0;
        for (; processed < batchSize; processed++)
        {
            var work = await _작업저장소.다음작업확보Async(
                TimeSpan.FromMinutes(Math.Max(1, options.LeaseTimeoutMinutes)),
                cancellationToken);
            if (work is null)
            {
                break;
            }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                await publisher.Publish(
                    new 커뮤니티원장변경됨Event(
                        work.원장,
                        work.변경유형,
                        work.변경자,
                        work.상태변경요청,
                        work.발생시각Utc,
                        work.EventId),
                    cancellationToken);

                await _작업저장소.완료Async(
                    work.원장.원장Id,
                    work.원장.Revision,
                    work.ProcessingToken,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _작업저장소.실패Async(
                    work.원장.원장Id,
                    work.원장.Revision,
                    work.ProcessingToken,
                    ex.Message,
                    options.MaxAttempts,
                    TimeSpan.FromSeconds(Math.Max(1, options.RetryBaseSeconds)),
                    cancellationToken);
                _logger.LogWarning(
                    ex,
                    "커뮤니티 원장 투영 재시도 작업에 실패했습니다. EventId={EventId}, 원장Id={원장Id}, Revision={Revision}, Attempt={Attempt}",
                    work.EventId,
                    work.원장.원장Id,
                    work.원장.Revision,
                    work.시도횟수);
            }
        }

        return processed;
    }
}
