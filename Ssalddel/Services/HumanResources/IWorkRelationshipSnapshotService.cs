using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Services.HumanResources;

public interface IWorkRelationshipSnapshotService
{
    Task RecordAsync(WorkRelationshipSnapshotRecordRequest request, CancellationToken cancellationToken);

    Task<WorkRelationshipSnapshotListResponse> GetMineAsync(int take, CancellationToken cancellationToken);
}
