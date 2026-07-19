using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Application.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route("api/v1/work-relationship-snapshots")]
public sealed class WorkRelationshipSnapshotsController : ControllerBase
{
    private readonly I인연스냅샷조회UseCase _useCase;

    public WorkRelationshipSnapshotsController(I인연스냅샷조회UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WorkRelationshipSnapshotListResponse>> GetMine(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _useCase.내목록Async(take, cancellationToken));
    }
}
