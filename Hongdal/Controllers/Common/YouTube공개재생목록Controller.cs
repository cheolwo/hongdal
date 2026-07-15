using Hongdal.ApiMetadata;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V0_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/content/youtube/playlists")]
public sealed class YouTube공개재생목록Controller : ControllerBase
{
    private readonly IYouTube채널감시Service _service;

    public YouTube공개재생목록Controller(IYouTube채널감시Service service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> 재생목록조회(
        [FromQuery] string channelId,
        CancellationToken cancellationToken = default)
        => Ok(await _service.재생목록목록조회Async(channelId, cancellationToken));

    [HttpGet("{playlistId}/videos")]
    [AllowAnonymous]
    public async Task<IActionResult> 재생목록영상조회(
        [FromRoute] string playlistId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _service.재생목록영상목록조회Async(playlistId, take, cancellationToken));
}
