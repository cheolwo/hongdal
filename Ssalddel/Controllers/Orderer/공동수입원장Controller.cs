using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Filters;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/orderer/group-purchase-demand-votes/{campaignId:guid}/group-import-ledger")]
public sealed class 공동수입원장Controller : OrdererControllerBase
{
    private readonly I공동수입원장전환Service _원장전환Service;

    public 공동수입원장Controller(I공동수입원장전환Service 원장전환Service)
    {
        _원장전환Service = 원장전환Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        CancellationToken cancellationToken)
    {
        var result = await _원장전환Service.조회Async(모집Id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("preview")]
    [SsalddelApiContractName("Preview")]
    public IActionResult 미리보기(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] CommunityGroupImportLedgerConversionRequest 요청)
    {
        요청.GroupPurchaseCampaignId = 모집Id;
        return Ok(_원장전환Service.미리보기(요청));
    }

    [HttpPost]
    [SsalddelApiContractName("Convert")]
    public async Task<IActionResult> 원장전환(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] CommunityGroupImportLedgerConversionRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.GroupPurchaseCampaignId = 모집Id;
            var result = await _원장전환Service.전환Async(
                요청,
                CurrentUserId(),
                cancellationToken);
            return result.Created
                ? CreatedAtAction(nameof(조회), new { campaignId = 모집Id }, result)
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
