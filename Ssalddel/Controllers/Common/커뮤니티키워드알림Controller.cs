using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route("api/v1/community/keyword-notifications")]
[SsalddelApiContractName("CommunityKeywordNotificationsController")]
public sealed class 커뮤니티키워드알림Controller : CommunityControllerBase
{
    private readonly ICommunityKeywordInboxService _키워드알림Service;

    public 커뮤니티키워드알림Controller(ICommunityKeywordInboxService 키워드알림Service)
    {
        _키워드알림Service = 키워드알림Service;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? appKey,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _키워드알림Service.ListAsync(
                CurrentUserId(),
                appKey,
                unreadOnly,
                page,
                pageSize,
                cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    [SsalddelApiContractName("GetUnreadCount")]
    public async Task<IActionResult> 읽지않은개수조회(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await _키워드알림Service.GetUnreadCountAsync(CurrentUserId(), appKey, cancellationToken);
            return Ok(new { count });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}/read")]
    [SsalddelApiContractName("MarkRead")]
    public async Task<IActionResult> 읽음처리(long id, CancellationToken cancellationToken)
        => await _키워드알림Service.MarkReadAsync(CurrentUserId(), id, cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPut("read-all")]
    [SsalddelApiContractName("MarkAllRead")]
    public async Task<IActionResult> 전체읽음처리(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedCount = await _키워드알림Service.MarkAllReadAsync(CurrentUserId(), appKey, cancellationToken);
            return Ok(new { updatedCount });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
