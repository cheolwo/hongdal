using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V1_5)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.OrdererGroupCommerce)]
[SsalddelApiContractName("DomesticGroupPurchaseFulfillmentPlansController")]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/fulfillment-plans")]
public sealed class 국내공동구매이행계획Controller : OrdererControllerBase
{
    private readonly IDomesticGroupPurchaseFulfillmentPlanService _이행계획Service;

    public 국내공동구매이행계획Controller(
        IDomesticGroupPurchaseFulfillmentPlanService 이행계획Service)
    {
        _이행계획Service = 이행계획Service;
    }

    [HttpPost("preview")]
    [SsalddelApiContractName("Preview")]
    public IActionResult 미리보기(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseFulfillmentPlanRequest 요청)
    {
        요청.GroupPurchaseCampaignId = 모집Id;
        return Ok(_이행계획Service.Preview(요청));
    }

    [HttpPost("order-drafts")]
    [SsalddelApiContractName("CreateOrderDraft")]
    public async Task<IActionResult> 발주초안생성(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseFulfillmentPlanRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.GroupPurchaseCampaignId = 모집Id;
            var 초안 = await _이행계획Service.CreateOrderDraftAsync(CurrentUserId(), 요청, cancellationToken);
            return CreatedAtAction(
                nameof(발주초안조회),
                new { campaignId = 모집Id, draftId = 초안.DraftId },
                초안);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("order-drafts/{draftId:guid}")]
    [SsalddelApiContractName("GetOrderDraft")]
    public async Task<IActionResult> 발주초안조회(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromRoute(Name = "draftId")] Guid 초안Id,
        CancellationToken cancellationToken)
    {
        var 초안 = await _이행계획Service.GetOrderDraftAsync(CurrentUserId(), 초안Id, cancellationToken);
        return 초안 is not null && 초안.Plan.GroupPurchaseCampaignId == 모집Id
            ? Ok(초안)
            : NotFound();
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
