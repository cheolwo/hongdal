using Hongdal.Services.HumanResources;
using Microsoft.Extensions.Logging;
using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public sealed class Command인연스냅샷Processor : ICommand후처리Processor
{
    private readonly IWorkRelationshipSnapshotService _snapshotService;
    private readonly IWorkRelationshipSnapshotCollector _snapshotCollector;
    private readonly ILogger<Command인연스냅샷Processor> _logger;

    public Command인연스냅샷Processor(
        IWorkRelationshipSnapshotService snapshotService,
        IWorkRelationshipSnapshotCollector snapshotCollector,
        ILogger<Command인연스냅샷Processor> logger)
    {
        _snapshotService = snapshotService;
        _snapshotCollector = snapshotCollector;
        _logger = logger;
    }

    public string Name => "WorkRelationshipSnapshot";

    public bool CanProcess(CommandProcessingRule rule) => rule.WorkRelationshipSnapshotEnabled.GetValueOrDefault();

    public async Task ProcessAsync(Command후처리Context context, CancellationToken cancellationToken)
    {
        if (context.Request is not IWorkRelationshipSnapshotCommand)
        {
            return;
        }

        var snapshots = _snapshotCollector.Drain()
            .Where(x => !string.IsNullOrWhiteSpace(x.WorkDomain)
                        && !string.IsNullOrWhiteSpace(x.WorkProcess)
                        && !string.IsNullOrWhiteSpace(x.ActionCode)
                        && !string.IsNullOrWhiteSpace(x.RelatedEntityType)
                        && !string.IsNullOrWhiteSpace(x.RelatedEntityId))
            .ToArray();

        foreach (var snapshot in snapshots)
        {
            await _snapshotService.RecordAsync(snapshot, cancellationToken);
        }

        if (snapshots.Length > 0)
        {
            _logger.LogInformation(
                "Command relationship snapshots recorded. CommandName={CommandName} Count={Count} TraceId={TraceId}",
                context.CommandName,
                snapshots.Length,
                context.TraceId);
        }
    }
}
