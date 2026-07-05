using Hongdal.Contracts.Common.Hr;

namespace Hongdal.Services.HumanResources;

public interface IWorkRelationshipSnapshotService
{
    Task RecordAsync(WorkRelationshipSnapshotRecordRequest request, CancellationToken cancellationToken);

    Task<WorkRelationshipSnapshotListResponse> GetMineAsync(int take, CancellationToken cancellationToken);
}
