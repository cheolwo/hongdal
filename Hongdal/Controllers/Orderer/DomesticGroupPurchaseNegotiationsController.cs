using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Authorize]
[HongdalApiVersion(HongdalProductVersion.V0_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/negotiation")]
public sealed class DomesticGroupPurchaseNegotiationsController : ControllerBase
{
    private readonly IDomesticGroupPurchaseNegotiationService service;

    public DomesticGroupPurchaseNegotiationsController(IDomesticGroupPurchaseNegotiationService service)
    {
        this.service = service;
    }

    [HttpGet]
    public IActionResult GetTimeline(Guid campaignId)
        => Ok(service.GetTimeline(campaignId));

    [HttpPost("events")]
    public IActionResult AppendEvent(Guid campaignId, [FromBody] DomesticGroupPurchaseNegotiationEventRequest request)
        => Execute(() => service.AppendEvent(campaignId, CurrentUserId(), request));

    [HttpPost("issues")]
    public IActionResult OpenIssue(Guid campaignId, [FromBody] DomesticGroupPurchaseNegotiationIssueRequest request)
        => Execute(() => service.OpenIssue(campaignId, CurrentUserId(), request));

    [HttpPost("issues/{issueId:guid}/positions")]
    public IActionResult AddPosition(
        Guid campaignId,
        Guid issueId,
        [FromBody] DomesticGroupPurchaseDeliberationPositionRequest request)
        => Execute(() => service.AddPosition(campaignId, issueId, CurrentUserId(), request));

    [HttpPost("issues/{issueId:guid}/resolution")]
    public IActionResult ResolveIssue(
        Guid campaignId,
        Guid issueId,
        [FromBody] DomesticGroupPurchaseNegotiationResolutionRequest request)
        => Execute(() => service.ResolveIssue(campaignId, issueId, CurrentUserId(), request));

    private IActionResult Execute<T>(Func<T> action)
    {
        try
        {
            return Ok(action());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
