using Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Microsoft.Extensions.Options;
using Quartz;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.Community;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.BackgroundProcessing,
    "게시판별 공공·공식 데이터 source를 정해진 주기로 글로 작성하고 같은 source·기간의 중복 발행을 방지",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "카탈로그에 등록되고 운영 설정에서 명시적으로 활성화된 source만 실행하며, 검증된 데이터가 없으면 글을 만들지 않습니다.")]
public sealed class CommunityEditorialBatchRunner
{
    private readonly IReadOnlyDictionary<string, ICommunityAutomatedPostSource> _sources;
    private readonly ICommunityAutomatedPostPublisher _publisher;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CommunityEditorialBatchRunner> _logger;

    public CommunityEditorialBatchRunner(
        IEnumerable<ICommunityAutomatedPostSource> sources,
        ICommunityAutomatedPostPublisher publisher,
        IOptions<CommunityEditorialBatchOptions> options,
        TimeProvider timeProvider,
        ILogger<CommunityEditorialBatchRunner> logger)
    {
        _sources = sources.ToDictionary(source => source.SourceKey, StringComparer.OrdinalIgnoreCase);
        _publisher = publisher;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public Task RunKamisPriceBriefAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.KamisPriceBrief, cancellationToken);

    public Task RunUsdaNassPriceBriefAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief, cancellationToken);

    public Task RunReflectionAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.Reflection, cancellationToken);

    public Task RunActivityDigestAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.ActivityDigest, cancellationToken);

    public Task RunCultureTransportAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.CultureTransport, cancellationToken);

    public Task RunPrajnaPublicationAsync(CancellationToken cancellationToken)
        => RunSourceAsync(CommunityAutomatedPostSourceKeys.Prajna, cancellationToken);

    private async Task RunSourceAsync(string sourceKey, CancellationToken cancellationToken)
    {
        if (!_sources.TryGetValue(sourceKey, out var source))
        {
            throw new InvalidOperationException($"자동 게시 원천을 찾을 수 없습니다. SourceKey={sourceKey}");
        }

        var timeZone = AgriculturalFisheriesBatchSchedule.ResolveTimeZone(_options.TimeZoneId);
        var publicationDate = AgriculturalFisheriesBatchSchedule.GetLocalDate(
            _timeProvider,
            _options.TimeZoneId);
        var draft = await source.BuildAsync(publicationDate, timeZone, cancellationToken);
        if (draft is null)
        {
            _logger.LogInformation(
                "Action={Action} SourceKey={SourceKey} PublicationDate={PublicationDate} Result={Result}",
                "CommunityEditorialPostSkipped",
                sourceKey,
                publicationDate,
                "NoVerifiedSourceData");
            return;
        }

        var result = await _publisher.PublishIfMissingAsync(draft, cancellationToken);
        _logger.LogInformation(
            "Action={Action} SourceKey={SourceKey} PeriodKey={PeriodKey} PostId={PostId} Created={Created}",
            "CommunityEditorialPostPublished",
            draft.SourceKey,
            draft.PeriodKey,
            result.PostId,
            result.Created);
    }
}

[DisallowConcurrentExecution]
public sealed class CommunityKamisPriceBriefJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityKamisPriceBriefJob> _logger;

    public CommunityKamisPriceBriefJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityKamisPriceBriefJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityKamisPriceBrief",
            _runner.RunKamisPriceBriefAsync,
            context,
            _options,
            _logger);
}

[DisallowConcurrentExecution]
public sealed class CommunityUsdaNassPriceBriefJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityUsdaNassPriceBriefJob> _logger;

    public CommunityUsdaNassPriceBriefJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityUsdaNassPriceBriefJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityUsdaNassPriceBrief",
            _runner.RunUsdaNassPriceBriefAsync,
            context,
            _options,
            _logger);
}

[DisallowConcurrentExecution]
public sealed class CommunityReflectionJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityReflectionJob> _logger;

    public CommunityReflectionJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityReflectionJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityReflection",
            _runner.RunReflectionAsync,
            context,
            _options,
            _logger);
}

[DisallowConcurrentExecution]
public sealed class CommunityActivityDigestJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityActivityDigestJob> _logger;

    public CommunityActivityDigestJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityActivityDigestJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityActivityDigest",
            _runner.RunActivityDigestAsync,
            context,
            _options,
            _logger);
}

[DisallowConcurrentExecution]
public sealed class CommunityPrajnaPublicationJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityPrajnaPublicationJob> _logger;

    public CommunityPrajnaPublicationJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityPrajnaPublicationJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityPrajnaPublication",
            _runner.RunPrajnaPublicationAsync,
            context,
            _options,
            _logger);
}

internal static class CommunityEditorialJobExecution
{
    internal static async Task RunAsync(
        string jobName,
        Func<CancellationToken, Task> action,
        IJobExecutionContext context,
        CommunityEditorialBatchOptions options,
        ILogger logger)
    {
        try
        {
            await action(context.CancellationToken);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var retryLimit = Math.Clamp(options.ImmediateRetryCount, 0, 3);
            var shouldRetry = context.RefireCount < retryLimit;
            logger.LogError(
                exception,
                "Action={Action} RefireCount={RefireCount} RetryLimit={RetryLimit} WillRetry={WillRetry}",
                jobName,
                context.RefireCount,
                retryLimit,
                shouldRetry);
            throw new JobExecutionException(exception, shouldRetry);
        }
    }
}
