using Microsoft.Extensions.Options;
using Quartz;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Community;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.BackgroundProcessing,
    "식약처 수입식품 근거 수집 뒤 중국 권역·미국 주별 월간 요약을 MFDS 주기 데이터 게시판에 작성",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "식약처의 현재 근거가 있고 국가별 발행 설정을 명시적으로 켠 경우에만 작성하며 제조업소 소재지를 원산지나 공급 가능성으로 확정하지 않습니다.")]
public sealed class OfficialFoodIngredientCompanyBatchRunner(
    IOfficialFoodIngredientCompanyArchiveService archiveService,
    IChinaImportedFoodRegionCommunityPostSource chinaCommunityPostSource,
    IUnitedStatesImportedFoodStateCommunityPostSource
        unitedStatesCommunityPostSource,
    ICommunityAutomatedPostPublisher communityPostPublisher,
    IOptions<AgriculturalFisheriesBatchOptions> options,
    TimeProvider timeProvider,
    ILogger<OfficialFoodIngredientCompanyBatchRunner> logger)
{
    private readonly AgriculturalFisheriesBatchOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var result = await archiveService.CollectCatalogAsync(
            new OfficialFoodIngredientCompanyCollectionRequest(
                _options.IngredientCompanyResearchMaxIngredients,
                _options.IngredientCompanyResearchCandidatesPerIngredient,
                Force: false,
                RefreshAfterDays: _options.IngredientCompanyResearchRefreshAfterDays,
                RequestDelayMilliseconds:
                _options.IngredientCompanyResearchRequestDelayMilliseconds),
            OfficialFoodIngredientCompanyResearchTriggerCodes.Scheduled,
            cancellationToken);
        logger.LogInformation(
            "Action={Action} RunKey={RunKey} Status={Status} Requested={Requested} Processed={Processed} Failed={Failed} Evidence={Evidence}",
            "OfficialFoodIngredientCompanyResearchCompleted",
            result.RunKey,
            result.StatusCode,
            result.RequestedIngredientCount,
            result.ProcessedIngredientCount,
            result.FailedIngredientCount,
            result.ObservedEvidenceCount);

        if (!_options.PublishChinaImportedFoodRegionBriefs
            && !_options.PublishUnitedStatesImportedFoodStateBriefs)
        {
            return;
        }

        var timeZone = AgriculturalFisheriesBatchSchedule.ResolveTimeZone(_options.TimeZoneId);
        var publicationDate = AgriculturalFisheriesBatchSchedule.GetLocalDate(
            timeProvider,
            _options.TimeZoneId);
        if (_options.PublishChinaImportedFoodRegionBriefs)
        {
            await PublishAsync(
                chinaCommunityPostSource.BuildAsync,
                "ChinaImportedFoodRegion",
                result.RunKey,
                publicationDate,
                timeZone,
                cancellationToken);
        }

        if (_options.PublishUnitedStatesImportedFoodStateBriefs)
        {
            await PublishAsync(
                unitedStatesCommunityPostSource.BuildAsync,
                "UnitedStatesImportedFoodState",
                result.RunKey,
                publicationDate,
                timeZone,
                cancellationToken);
        }
    }

    private async Task PublishAsync(
        Func<DateOnly, TimeZoneInfo, CancellationToken,
            Task<CommunityAutomatedPostDraft?>> buildDraft,
        string actionPrefix,
        string runKey,
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken)
    {
        var draft = await buildDraft(
            publicationDate,
            timeZone,
            cancellationToken);
        if (draft is null)
        {
            logger.LogInformation(
                "Action={Action} RunKey={RunKey} Result={Result}",
                $"{actionPrefix}CommunityPostSkipped",
                runKey,
                "NoCurrentRegionEvidence");
            return;
        }

        var publication = await communityPostPublisher.PublishIfMissingAsync(
            draft,
            cancellationToken);
        logger.LogInformation(
            "Action={Action} RunKey={RunKey} PeriodKey={PeriodKey} PostId={PostId} Created={Created}",
            $"{actionPrefix}CommunityPostPublished",
            runKey,
            draft.PeriodKey,
            publication.PostId,
            publication.Created);
    }
}

[DisallowConcurrentExecution]
public sealed class OfficialFoodIngredientCompanyResearchJob(
    OfficialFoodIngredientCompanyBatchRunner runner,
    IOptions<AgriculturalFisheriesBatchOptions> options,
    ILogger<OfficialFoodIngredientCompanyResearchJob> logger) : IJob
{
    private readonly AgriculturalFisheriesBatchOptions _options = options.Value;

    public Task Execute(IJobExecutionContext context)
        => AgriculturalFisheriesBatchJobExecution.RunAsync(
            "OfficialFoodIngredientCompanyResearch",
            runner.RunAsync,
            context,
            _options,
            logger);
}
