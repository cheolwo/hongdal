using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Content;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Content07;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/content/youtube")]
[Authorize(Policy = "서버관리자전용")]
public sealed class YouTube채널감시Controller : ControllerBase
{
    private readonly IYouTube채널감시Service _service;

    public YouTube채널감시Controller(IYouTube채널감시Service service)
    {
        _service = service;
    }

    [HttpGet("channels")]
    public async Task<IActionResult> 채널목록조회(CancellationToken cancellationToken)
        => Ok(await _service.채널목록조회Async(cancellationToken));

    [HttpPost("channels")]
    public async Task<IActionResult> 채널등록(
        [FromBody] YouTube감시채널등록요청Dto 요청,
        CancellationToken cancellationToken)
    {
        var created = await _service.채널등록Async(요청, cancellationToken);
        return CreatedAtAction(nameof(채널목록조회), created);
    }

    [HttpGet("videos")]
    public async Task<IActionResult> 영상목록조회(
        [FromQuery] string? channelId,
        [FromQuery] bool newOnly = false,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _service.영상목록조회Async(channelId, newOnly, take, cancellationToken));

    [HttpGet("playlists")]
    public async Task<IActionResult> 재생목록조회(
        [FromQuery] string channelId,
        CancellationToken cancellationToken = default)
        => Ok(await _service.재생목록목록조회Async(channelId, cancellationToken));

    [HttpGet("playlists/{playlistId}/videos")]
    public async Task<IActionResult> 재생목록영상조회(
        [FromRoute] string playlistId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await _service.재생목록영상목록조회Async(playlistId, take, cancellationToken));

    [HttpPut("videos/{videoId}/publication")]
    public async Task<IActionResult> 영상공개설정(
        [FromRoute] string videoId,
        [FromBody] YouTube영상공개설정요청Dto 요청,
        CancellationToken cancellationToken)
        => Ok(await _service.영상공개설정Async(videoId, 요청.공개여부, cancellationToken));

    [HttpPost("sync")]
    public async Task<IActionResult> 동기화(
        [FromQuery] string? channelId,
        CancellationToken cancellationToken)
        => Ok(await _service.동기화Async(channelId, cancellationToken));
}
