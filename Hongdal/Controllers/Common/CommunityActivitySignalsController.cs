using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/activity-signals")]
public sealed class CommunityActivitySignalsController : ControllerBase
{
    private readonly ICommunityActivitySignalService _signalService;

    public CommunityActivitySignalsController(ICommunityActivitySignalService signalService)
    {
        _signalService = signalService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityActivitySignalListResponse>> Get(
        [FromQuery] CommunityActivitySignalQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _signalService.GetSignalsAsync(query, cancellationToken));
    }
}
