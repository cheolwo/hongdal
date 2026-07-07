using Hongdal.Contracts.Common.Hr;
using Hongdal.Services.HumanResources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route("api/v1/work-relationship-snapshots")]
public sealed class WorkRelationshipSnapshotsController : ControllerBase
{
    private readonly IWorkRelationshipSnapshotService _snapshotService;

    public WorkRelationshipSnapshotsController(IWorkRelationshipSnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WorkRelationshipSnapshotListResponse>> GetMine(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _snapshotService.GetMineAsync(take, cancellationToken));
    }
}
