using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Services.HumanResources;

namespace Ssalddel.Application.HumanResources;

public interface I인연스냅샷조회UseCase
{
    Task<WorkRelationshipSnapshotListResponse> 내목록Async(int take, CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("인연 스냅샷 조회", Summary = "현재 사용자의 업무 인연 스냅샷을 조회해 커뮤니티 신뢰 신호와 참여 이력을 연결합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelUseCaseActor(SsalddelActor.Worker, SsalddelUseCaseActorRole.Supporting)]
public sealed class 인연스냅샷조회UseCase : I인연스냅샷조회UseCase
{
    private readonly IWorkRelationshipSnapshotService _snapshotService;

    public 인연스냅샷조회UseCase(IWorkRelationshipSnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    public Task<WorkRelationshipSnapshotListResponse> 내목록Async(int take, CancellationToken cancellationToken)
    {
        return _snapshotService.GetMineAsync(take, cancellationToken);
    }
}
