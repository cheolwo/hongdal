using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[SsalddelApiContractName("SupplierRelationshipMembershipsController")]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/orderer/supplier-relationships/{supplierKey}")]
public sealed class 공급자MembershipController : OrdererControllerBase
{
    private readonly I공급자Membership혜택계산Service _혜택계산Service;
    private readonly I공급자관심구독Service _관심구독Service;

    public 공급자MembershipController(
        I공급자Membership혜택계산Service 혜택계산Service,
        I공급자관심구독Service 관심구독Service)
    {
        _혜택계산Service = 혜택계산Service;
        _관심구독Service = 관심구독Service;
    }

    [HttpPost("interest-subscription-drafts")]
    [SsalddelApiContractName("CreateInterestSubscriptionDraft")]
    public async Task<IActionResult> 관심구독초안생성(
        [FromRoute(Name = "supplierKey")] string 공급자Key,
        [FromBody] SupplierInterestSubscriptionDraftRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.SupplierKey = 공급자Key;
            var draft = await _관심구독Service.초안생성Async(
                CurrentUserId(),
                요청,
                cancellationToken);
            return CreatedAtAction(
                nameof(관심구독초안조회),
                new { supplierKey = 공급자Key, draftId = draft.DraftId },
                draft);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("interest-subscription-drafts/{draftId:guid}")]
    [SsalddelApiContractName("GetInterestSubscriptionDraft")]
    public async Task<IActionResult> 관심구독초안조회(
        [FromRoute(Name = "supplierKey")] string 공급자Key,
        [FromRoute(Name = "draftId")] Guid 초안Id,
        CancellationToken cancellationToken)
    {
        var draft = await _관심구독Service.초안조회Async(
            CurrentUserId(),
            초안Id,
            cancellationToken);
        return draft is not null &&
               string.Equals(draft.SupplierKey, 공급자Key, StringComparison.Ordinal)
            ? Ok(draft)
            : NotFound();
    }

    [HttpPost("membership-benefit-previews")]
    [SsalddelApiContractName("PreviewBenefit")]
    public IActionResult 혜택미리보기(
        [FromRoute(Name = "supplierKey")] string 공급자Key,
        [FromBody] SupplierMembershipBenefitPreviewRequest 요청)
    {
        try
        {
            요청.SupplierKey = 공급자Key;
            return Ok(_혜택계산Service.미리보기(요청));
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
