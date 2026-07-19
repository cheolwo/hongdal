using Microsoft.Extensions.Logging.Abstractions;

namespace Ssalddel.Services.Community;

internal interface ICommunityGroupPurchaseDemandOutboxProcessor
{
    Task<bool> ProcessAsync(
        Guid voteId,
        string outboxId,
        CancellationToken cancellationToken);

    Task<bool> ProcessNextAsync(CancellationToken cancellationToken);
}

internal sealed class CommunityGroupPurchaseDemandOutboxProcessor : ICommunityGroupPurchaseDemandOutboxProcessor
{
    internal const int DefaultMaxAttempts = 8;
    internal static readonly TimeSpan LeaseTimeout = TimeSpan.FromMinutes(5);
    internal static readonly TimeSpan DefaultRetryBaseDelay = TimeSpan.FromSeconds(5);

    private readonly ICommunityVoteStore _store;
    private readonly ICommunityGroupPurchaseDemandHandoff _handoff;
    private readonly ILogger<CommunityGroupPurchaseDemandOutboxProcessor> _logger;
    private readonly int _maxAttempts;
    private readonly TimeSpan _retryBaseDelay;

    public CommunityGroupPurchaseDemandOutboxProcessor(
        ICommunityVoteStore store,
        ICommunityGroupPurchaseDemandHandoff handoff,
        ILogger<CommunityGroupPurchaseDemandOutboxProcessor>? logger = null,
        int maxAttempts = DefaultMaxAttempts,
        TimeSpan? retryBaseDelay = null)
    {
        _store = store;
        _handoff = handoff;
        _logger = logger ?? NullLogger<CommunityGroupPurchaseDemandOutboxProcessor>.Instance;
        _maxAttempts = Math.Max(1, maxAttempts);
        _retryBaseDelay = retryBaseDelay ?? DefaultRetryBaseDelay;
    }

    public async Task<bool> ProcessAsync(
        Guid voteId,
        string outboxId,
        CancellationToken cancellationToken)
    {
        var work = await _store.TryClaimDemandHandoffAsync(
            voteId,
            outboxId,
            LeaseTimeout,
            cancellationToken);
        return work is not null && await ProcessClaimedAsync(work, cancellationToken);
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var work = await _store.TryClaimNextDemandHandoffAsync(LeaseTimeout, cancellationToken);
        if (work is null)
        {
            return false;
        }

        await ProcessClaimedAsync(work, cancellationToken);
        return true;
    }

    private async Task<bool> ProcessClaimedAsync(
        CommunityVoteDemandHandoffWork work,
        CancellationToken cancellationToken)
    {
        try
        {
            await _handoff.SyncAsync(work.Request, cancellationToken);
            await _store.CompleteDemandHandoffAsync(work, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _store.FailDemandHandoffAsync(
                work,
                ex.Message,
                _maxAttempts,
                _retryBaseDelay,
                cancellationToken);
            _logger.LogWarning(
                ex,
                "공동구매 투표 수요 전달에 실패하여 재시도 대기열에 보관했습니다. VoteId={VoteId}, OutboxId={OutboxId}, Attempt={Attempt}",
                work.VoteId,
                work.OutboxId,
                work.AttemptCount);
            return false;
        }
    }
}

public sealed class CommunityGroupPurchaseDemandOutboxWorker : BackgroundService
{
    private const int BatchSize = 50;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CommunityGroupPurchaseDemandOutboxWorker> _logger;

    public CommunityGroupPurchaseDemandOutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CommunityGroupPurchaseDemandOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = 0;
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<ICommunityGroupPurchaseDemandOutboxProcessor>();
                for (; processed < BatchSize; processed++)
                {
                    if (!await processor.ProcessNextAsync(stoppingToken))
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "공동구매 투표 수요 재시도 대기열 처리 중 예외가 발생했습니다.");
            }

            if (processed < BatchSize)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
