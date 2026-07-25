using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Infrastructure.BackgroundJobs.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.BackgroundProcessing,
    "KAMIS·USDA 공공데이터 수집 성공 뒤 canonical 주기 데이터 게시판의 요약 글 작성을 인계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "수집과 자동 발행이 모두 명시적으로 활성화된 경우에만 인계하며 원자료 조회 실패를 sample 글로 대체하지 않습니다.")]
public sealed class AgriculturalFisheriesCommunityPipelineRunner
{
    private readonly AgriculturalFisheriesBatchRunner _collectionRunner;
    private readonly CommunityEditorialBatchRunner _editorialRunner;
    private readonly AgriculturalFisheriesBatchOptions _batchOptions;
    private readonly CommunityEditorialBatchOptions _editorialOptions;

    public AgriculturalFisheriesCommunityPipelineRunner(
        AgriculturalFisheriesBatchRunner collectionRunner,
        CommunityEditorialBatchRunner editorialRunner,
        IOptions<AgriculturalFisheriesBatchOptions> batchOptions,
        IOptions<CommunityEditorialBatchOptions> editorialOptions)
    {
        _collectionRunner = collectionRunner;
        _editorialRunner = editorialRunner;
        _batchOptions = batchOptions.Value;
        _editorialOptions = editorialOptions.Value;
    }

    public async Task RunKamisDailyAsync(CancellationToken cancellationToken)
    {
        await _collectionRunner.RunKamisDailyAsync(cancellationToken);
        if (_batchOptions.PublishCommunityPriceBriefs
            && _editorialOptions.KamisPriceBriefEnabled)
        {
            await _editorialRunner.RunKamisPriceBriefAsync(cancellationToken);
        }
    }

    public Task RunKamisMonthlyAsync(CancellationToken cancellationToken)
        => _collectionRunner.RunKamisMonthlyAsync(cancellationToken);

    public async Task RunUsdaMonthlyAsync(CancellationToken cancellationToken)
    {
        await _collectionRunner.RunUsdaMonthlyAsync(cancellationToken);
        if (_batchOptions.PublishCommunityPriceBriefs
            && _editorialOptions.UsdaNassPriceBriefEnabled)
        {
            await _editorialRunner.RunUsdaNassPriceBriefAsync(cancellationToken);
        }
    }
}
