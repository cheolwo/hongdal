using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I업무인연커뮤니티Service
{
    Task<WorkRelationshipSnapshotListResponse> 내업무인연조회Async(
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<long> 연결요청Async(
        Guid snapshotId,
        WorkRelationshipConnectionRequestCreateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class 업무인연커뮤니티Client(ISsalddelJsonApiClient apiClient)
    : I업무인연커뮤니티Service
{
    public async Task<WorkRelationshipSnapshotListResponse> 내업무인연조회Async(
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var safeTake = Math.Clamp(take, 1, 100);
        return await apiClient.GetAsync<WorkRelationshipSnapshotListResponse>(
                   $"api/v1/work-relationship-snapshots/me?take={safeTake}",
                   "내 업무 인연 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? new WorkRelationshipSnapshotListResponse();
    }

    public async Task<long> 연결요청Async(
        Guid snapshotId,
        WorkRelationshipConnectionRequestCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (snapshotId == Guid.Empty)
        {
            throw new ArgumentException("업무 인연 스냅샷 ID가 필요합니다.", nameof(snapshotId));
        }

        return await apiClient.SendAsync<WorkRelationshipConnectionRequestCreateRequest, long>(
            HttpMethod.Post,
            $"api/v1/connections/requests/from-work-relationship/{snapshotId:D}",
            request,
            "업무 인연 연결 요청",
            allowNotFound: false,
            cancellationToken);
    }
}
