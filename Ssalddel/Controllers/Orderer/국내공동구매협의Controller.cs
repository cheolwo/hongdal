using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.OrdererGroupCommerce)]
[SsalddelApiContractName("DomesticGroupPurchaseNegotiationsController")]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/negotiation")]
public sealed class 국내공동구매협의Controller : OrdererControllerBase
{
    private readonly IDomesticGroupPurchaseNegotiationService _협의Service;

    public 국내공동구매협의Controller(IDomesticGroupPurchaseNegotiationService 협의Service)
    {
        _협의Service = 협의Service;
    }

    [HttpGet]
    [SsalddelApiContractName("GetTimeline")]
    public async Task<IActionResult> 협의이력조회(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        CancellationToken cancellationToken)
        => Ok(await _협의Service.GetTimelineAsync(모집Id, cancellationToken));

    [HttpPost("events")]
    [SsalddelApiContractName("AppendEvent")]
    public Task<IActionResult> 협의기록추가(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseNegotiationEventRequest 요청,
        CancellationToken cancellationToken)
        => 실행Async(() => _협의Service.AppendEventAsync(모집Id, CurrentUserId(), 요청, cancellationToken));

    [HttpPost("issues")]
    [SsalddelApiContractName("OpenIssue")]
    public Task<IActionResult> 쟁점등록(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseNegotiationIssueRequest 요청,
        CancellationToken cancellationToken)
        => 실행Async(() => _협의Service.OpenIssueAsync(모집Id, CurrentUserId(), 요청, cancellationToken));

    [HttpPost("issues/{issueId:guid}/positions")]
    [SsalddelApiContractName("AddPosition")]
    public Task<IActionResult> 숙고의견추가(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromRoute(Name = "issueId")] Guid 쟁점Id,
        [FromBody] DomesticGroupPurchaseDeliberationPositionRequest 요청,
        CancellationToken cancellationToken)
        => 실행Async(() => _협의Service.AddPositionAsync(모집Id, 쟁점Id, CurrentUserId(), 요청, cancellationToken));

    [HttpPost("issues/{issueId:guid}/resolution")]
    [SsalddelApiContractName("ResolveIssue")]
    public Task<IActionResult> 쟁점합의(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromRoute(Name = "issueId")] Guid 쟁점Id,
        [FromBody] DomesticGroupPurchaseNegotiationResolutionRequest 요청,
        CancellationToken cancellationToken)
        => 실행Async(() => _협의Service.ResolveIssueAsync(모집Id, 쟁점Id, CurrentUserId(), 요청, cancellationToken));

    private async Task<IActionResult> 실행Async<T>(Func<Task<T>> 동작)
    {
        try
        {
            return Ok(await 동작());
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
