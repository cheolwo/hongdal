using Hongdal.ApiMetadata;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/content/youtube/videos")]
public sealed class YouTube공개영상Controller : ControllerBase
{
    private readonly IYouTube채널감시Service _service;

    public YouTube공개영상Controller(IYouTube채널감시Service service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> 최신영상조회(
        [FromQuery] string? channelId,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _service.공개영상목록조회Async(channelId, take, cancellationToken));
}
