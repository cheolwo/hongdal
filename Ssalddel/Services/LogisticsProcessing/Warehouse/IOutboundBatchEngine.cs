using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Services.LogisticsProcessing.Warehouse;

public interface IOutboundBatchEngine
{
    Task<OutboundBatchPlanResult> PlanAsync(OutboundBatchPlanRequest request, CancellationToken cancellationToken);
}
