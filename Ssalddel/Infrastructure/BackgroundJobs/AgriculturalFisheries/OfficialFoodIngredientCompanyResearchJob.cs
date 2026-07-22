using Microsoft.Extensions.Options;
using Quartz;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;

public sealed class OfficialFoodIngredientCompanyBatchRunner(
    IOfficialFoodIngredientCompanyArchiveService archiveService,
    IOptions<AgriculturalFisheriesBatchOptions> options,
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
