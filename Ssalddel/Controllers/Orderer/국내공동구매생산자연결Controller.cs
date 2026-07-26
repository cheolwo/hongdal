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
[SsalddelApiContractName("DomesticGroupPurchaseProducerConnectionsController")]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/producer-connections")]
public sealed class 국내공동구매생산자연결Controller : OrdererControllerBase
{
    private readonly IDomesticGroupPurchaseProducerConnectionService _생산자연결Service;

    public 국내공동구매생산자연결Controller(
        IDomesticGroupPurchaseProducerConnectionService 생산자연결Service)
    {
        _생산자연결Service = 생산자연결Service;
    }

    [HttpGet("candidates")]
    [SsalddelApiContractName("SearchCandidates")]
    public async Task<IActionResult> 생산자후보검색(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromQuery(Name = "search")] string? 검색어,
        [FromQuery(Name = "regionCode")] string? 지역코드,
        [FromQuery(Name = "product")] string? 상품,
        CancellationToken cancellationToken)
    {
        if (모집Id == Guid.Empty)
        {
            return BadRequest(new { message = "공동구매 캠페인 식별자가 필요합니다." });
        }

        return Ok(await _생산자연결Service.SearchCandidatesAsync(검색어, 지역코드, 상품, cancellationToken));
    }

    [HttpPost("contact-request-drafts")]
    [SsalddelApiContractName("CreateDraft")]
    public async Task<IActionResult> 연락요청초안생성(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticProducerContactRequestDraftRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.GroupPurchaseCampaignId = 모집Id;
            var 초안 = await _생산자연결Service.CreateDraftAsync(CurrentUserId(), 요청, cancellationToken);
            return CreatedAtAction(
                nameof(연락요청초안조회),
                new { campaignId = 모집Id, draftId = 초안.DraftId },
                초안);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("contact-request-drafts/{draftId:guid}")]
    [SsalddelApiContractName("GetDraft")]
    public async Task<IActionResult> 연락요청초안조회(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromRoute(Name = "draftId")] Guid 초안Id,
        CancellationToken cancellationToken)
    {
        var 초안 = await _생산자연결Service.GetDraftAsync(CurrentUserId(), 초안Id, cancellationToken);
        return 초안 is not null && 초안.GroupPurchaseCampaignId == 모집Id
            ? Ok(초안)
            : NotFound();
    }

    [HttpGet("representatives")]
    [SsalddelApiContractName("SearchRepresentatives")]
    public async Task<IActionResult> 대표후보검색(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromQuery(Name = "search")] string? 검색어,
        [FromQuery(Name = "operatingAreaCode")] string? 운영지역코드,
        [FromQuery(Name = "product")] string? 상품,
        CancellationToken cancellationToken)
    {
        if (모집Id == Guid.Empty)
        {
            return BadRequest(new { message = "공동구매 캠페인 식별자가 필요합니다." });
        }

        return Ok(await _생산자연결Service.SearchRepresentativesAsync(
            검색어,
            운영지역코드,
            상품,
            cancellationToken));
    }

    [HttpPost("supply-offer-drafts")]
    [SsalddelApiContractName("CreateSupplyOfferDraft")]
    public async Task<IActionResult> 공급제안초안생성(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticProducerSupplyOfferDraftRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.GroupPurchaseCampaignId = 모집Id;
            var 초안 = await _생산자연결Service.CreateSupplyOfferDraftAsync(CurrentUserId(), 요청, cancellationToken);
            return CreatedAtAction(
                nameof(공급제안초안조회),
                new { campaignId = 모집Id, draftId = 초안.DraftId },
                초안);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("supply-offer-drafts/{draftId:guid}")]
    [SsalddelApiContractName("GetSupplyOfferDraft")]
    public async Task<IActionResult> 공급제안초안조회(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromRoute(Name = "draftId")] Guid 초안Id,
        CancellationToken cancellationToken)
    {
        var 초안 = await _생산자연결Service.GetSupplyOfferDraftAsync(CurrentUserId(), 초안Id, cancellationToken);
        return 초안 is not null && 초안.GroupPurchaseCampaignId == 모집Id
            ? Ok(초안)
            : NotFound();
    }

    [HttpPost("compatibility-previews")]
    [SsalddelApiContractName("PreviewCompatibility")]
    public IActionResult 공급적합성미리보기(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseSupplyCompatibilityPreviewRequest 요청)
    {
        if (모집Id == Guid.Empty)
        {
            return BadRequest(new { message = "공동구매 캠페인 식별자가 필요합니다." });
        }

        return Ok(_생산자연결Service.PreviewCompatibility(요청));
    }

    [HttpPost("urgent-harvest-compatibility-previews")]
    [SsalddelApiContractName("PreviewUrgentHarvestConnection")]
    public IActionResult 긴급수확연결적합성미리보기(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticUrgentHarvestConnectionPreviewRequest 요청)
    {
        if (모집Id == Guid.Empty)
        {
            return BadRequest(new { message = "공동구매 캠페인 식별자가 필요합니다." });
        }

        return Ok(
            _생산자연결Service.PreviewUrgentHarvestConnection(요청));
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
