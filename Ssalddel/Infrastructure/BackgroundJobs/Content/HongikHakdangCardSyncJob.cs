using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;
using Quartz;

namespace Ssalddel.Infrastructure.BackgroundJobs.Content;

[DisallowConcurrentExecution]
public sealed class HongikHakdangCardSyncJob : IJob
{
    private readonly IHongikHakdangCardService _service;
    private readonly IHongikHakdangCardVariantService _variantService;
    private readonly Microsoft.Extensions.Options.IOptions<살뜰.Services.Options.HongikHakdangCardOptions> _options;
    private readonly ILogger<HongikHakdangCardSyncJob> _logger;

    public HongikHakdangCardSyncJob(
        IHongikHakdangCardService service,
        IHongikHakdangCardVariantService variantService,
        Microsoft.Extensions.Options.IOptions<살뜰.Services.Options.HongikHakdangCardOptions> options,
        ILogger<HongikHakdangCardSyncJob> logger)
    {
        _service = service;
        _variantService = variantService;
        _options = options;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await _service.SyncAsync(context.CancellationToken);
        HongikHakdangCardVariantPreparationResultDto? variants = null;
        if (result.Executed && _options.Value.PrepareVariantsOnSync)
        {
            variants = await _variantService.EnsureActiveVariantsAsync(context.CancellationToken);
        }
        _logger.LogInformation(
            "Action={Action} Executed={Executed} CollectionCount={CollectionCount} UniqueCardCount={UniqueCardCount} DownloadedImageCount={DownloadedImageCount} FailedImageCount={FailedImageCount} GeneratedVariantCount={GeneratedVariantCount} ReusedVariantCount={ReusedVariantCount} FailedVariantCount={FailedVariantCount}",
            "HongikHakdangCardsSynced",
            result.Executed,
            result.CollectionCount,
            result.UniqueCardCount,
            result.DownloadedImageCount,
            result.FailedImageCount,
            variants?.GeneratedVariantCount ?? 0,
            variants?.ReusedVariantCount ?? 0,
            variants?.FailedVariantCount ?? 0);
    }
}
