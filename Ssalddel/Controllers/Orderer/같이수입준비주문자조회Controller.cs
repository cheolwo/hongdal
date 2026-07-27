using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
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
[Route("api/v1/orderer/group-imports/{groupImportLedgerId}/readiness")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Api,
    "주문자 App이 본인이 참여한 같이 수입 원장의 1.5 준비 자료를 원장 식별자로 조회합니다.",
    ContractType = typeof(I같이수입준비주문자조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "인증과 원천 자동집단 참여 검증을 요구하며 조회 외 상태 변경, OS 실행, 외부 전송 또는 거래 실행 API를 노출하지 않습니다.")]
public sealed class 같이수입준비주문자조회Controller : OrdererControllerBase
{
    private readonly I같이수입준비주문자조회UseCase _같이수입준비조회UseCase;

    public 같이수입준비주문자조회Controller(I같이수입준비주문자조회UseCase 같이수입준비조회UseCase)
    {
        _같이수입준비조회UseCase = 같이수입준비조회UseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(같이수입준비주문자조회응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> 조회(
        [FromRoute(Name = "groupImportLedgerId")] string 같이수입원장Id,
        [FromQuery(Name = "autoGroupId")] string? 자동집단Id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(자동집단Id))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "원천 자동집단 식별자가 필요합니다."
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var result = await _같이수입준비조회UseCase.조회Async(
            같이수입원장Id,
            자동집단Id,
            currentUserId,
            cancellationToken);
        return result is null
            ? NotFound()
            : Ok(result);
    }
}
