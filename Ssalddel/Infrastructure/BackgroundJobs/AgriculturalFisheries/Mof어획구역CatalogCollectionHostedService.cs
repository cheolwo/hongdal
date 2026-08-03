using Microsoft.Extensions.Options;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;

public sealed class Mof어획구역CatalogCollectionHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<PublicDataOptions> options,
    TimeProvider timeProvider,
    ILogger<Mof어획구역CatalogCollectionHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var configured = options.Value.MofFishingAreas;
        if (!configured.PersistenceEnabled || !configured.CollectAtStartup)
        {
            logger.LogInformation(
                "MOF fishing-area persistence is disabled. PersistenceEnabled={PersistenceEnabled}, CollectAtStartup={CollectAtStartup}",
                configured.PersistenceEnabled,
                configured.CollectAtStartup);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var archive = scope.ServiceProvider.GetRequiredService<IMof어획구역CatalogArchiveService>();
        var day = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var runKey = $"mof-fishing-area:{configured.DatasetVersion}:{day:yyyyMMdd}";
        var result = await archive.CollectAsync(runKey, cancellationToken);
        if (result.StatusCode == Mof어획구역Catalog수집상태Codes.실패)
        {
            logger.LogWarning(
                "MOF fishing-area snapshot collection failed. RunKey={RunKey}, Error={Error}",
                result.RunKey,
                result.ErrorMessage);
            return;
        }

        logger.LogInformation(
            "MOF fishing-area snapshot collection finished. RunKey={RunKey}, Status={Status}, SnapshotId={SnapshotId}, Created={Created}, Rows={Rows}, Hash={Hash}",
            result.RunKey,
            result.StatusCode,
            result.SnapshotId,
            result.SnapshotCreated,
            result.SourceRowCount,
            result.ContentSha256);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
