using Microsoft.Extensions.Options;
using Ssalddel.Infrastructure.BackgroundJobs.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;

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
