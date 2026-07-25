using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Services.HumanResources;

namespace Ssalddel.Application.HumanResources;

public interface I업무관계스냅샷조회UseCase
{
    Task<WorkRelationshipSnapshotListResponse> 내목록Async(int take, CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("업무 관계 스냅샷 조회", Summary = "현재 사용자의 업무 관계 기록을 조회해 명시적인 친구 요청 후보와 참여 이력을 연결합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelUseCaseActor(SsalddelActor.Worker, SsalddelUseCaseActorRole.Supporting)]
public sealed class 업무관계스냅샷조회UseCase : I업무관계스냅샷조회UseCase
{
    private readonly IWorkRelationshipSnapshotService _snapshotService;

    public 업무관계스냅샷조회UseCase(IWorkRelationshipSnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    public Task<WorkRelationshipSnapshotListResponse> 내목록Async(int take, CancellationToken cancellationToken)
    {
        return _snapshotService.GetMineAsync(take, cancellationToken);
    }
}
