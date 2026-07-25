using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Application.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiIntroducedIn(SsalddelProductVersion.V0_0)]
[SsalddelApiCapability(SsalddelCapability.WorkActivitySignal)]
[SsalddelApiCapability(SsalddelCapability.RelationshipFormation)]
[SsalddelApiAudience(SsalddelActor.CommunityMember)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[ApiController]
[Authorize]
[Route("api/v1/work-relationship-snapshots")]
[SsalddelApiContractName("WorkRelationshipSnapshotsController")]
public sealed class 업무관계SnapshotController : CommunityControllerBase
{
    private readonly I업무관계스냅샷조회UseCase _업무관계Snapshot조회UseCase;

    public 업무관계SnapshotController(I업무관계스냅샷조회UseCase 업무관계Snapshot조회UseCase)
    {
        _업무관계Snapshot조회UseCase = 업무관계Snapshot조회UseCase;
    }

    [HttpGet("me")]
    [SsalddelApiContractName("GetMine")]
    public async Task<ActionResult<WorkRelationshipSnapshotListResponse>> 내업무관계조회(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _업무관계Snapshot조회UseCase.내목록Async(take, cancellationToken));
    }
}
