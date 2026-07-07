using Hongdal.Contracts.Common.Warehouse;

namespace Hongdal.Services.LogisticsProcessing.Warehouse;

public interface IOutboundBatchEngine
{
    Task<OutboundBatchPlanResult> PlanAsync(OutboundBatchPlanRequest request, CancellationToken cancellationToken);
}
