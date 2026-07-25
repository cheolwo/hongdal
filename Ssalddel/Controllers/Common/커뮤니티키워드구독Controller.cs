using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route("api/v1/community/keyword-subscriptions")]
[SsalddelApiContractName("CommunityKeywordSubscriptionsController")]
public sealed class 커뮤니티키워드구독Controller : CommunityControllerBase
{
    private readonly ICommunityKeywordSubscriptionService _키워드구독Service;

    public 커뮤니티키워드구독Controller(ICommunityKeywordSubscriptionService 키워드구독Service)
    {
        _키워드구독Service = 키워드구독Service;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? appKey,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _키워드구독Service.ListAsync(CurrentUserId(), appKey, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [SsalddelApiContractName("Subscribe")]
    public async Task<IActionResult> 구독(
        [FromBody] CommunityKeywordSubscriptionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _키워드구독Service.SubscribeAsync(CurrentUserId(), request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    [SsalddelApiContractName("Unsubscribe")]
    public async Task<IActionResult> 구독해제(long id, CancellationToken cancellationToken)
        => await _키워드구독Service.UnsubscribeAsync(CurrentUserId(), id, cancellationToken)
            ? NoContent()
            : NotFound();

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
