using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
// 기존 운영 도구와 client의 URL 호환을 위해 route는 유지한다.
[Route("api/v1/admin/orderer/group-purchase-demand-os")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Api,
    "관리자가 공동구매 모집 프로세스 상태를 조회하고 마감 재조율 또는 1.5 준비 인계를 명시적으로 승인하게 합니다.",
    ContractType = typeof(I공동구매수요모집ProcessManager),
    FlowOrder = 15,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "서버 관리자만 접근하며 승인 API도 주문·결제·계약이나 1.5 원장을 자동 생성하지 않습니다.")]
public sealed class 공동구매수요모집ProcessManagerAdminController : ControllerBase
{
    private readonly I공동구매수요모집ProcessManager _processManager;

    public 공동구매수요모집ProcessManagerAdminController(
        I공동구매수요모집ProcessManager processManager)
    {
        _processManager = processManager;
    }

    [HttpGet("groups/{autoGroupId}/operating-status")]
    [ProducesResponseType(typeof(공동구매수요모집Os상태응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> 운영상태조회(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        CancellationToken cancellationToken)
    {
        var 상태 = await _processManager.운영상태조회Async(자동집단Id, cancellationToken);
        return 상태 is null
            ? this.ToNotFoundProblem("공동구매 모집 프로세스 원장을 찾을 수 없습니다.")
            : Ok(상태);
    }

    [HttpPost("groups/{autoGroupId}/reconcile")]
    [ProducesResponseType(typeof(공동구매수요모집Os조율응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 수동재조율(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        CancellationToken cancellationToken)
    {
        var 결과 = await _processManager.집단조율Async(
            자동집단Id,
            공동구매수요모집Os트리거코드.수동재조율,
            cancellationToken: cancellationToken);
        return Ok(결과);
    }

    [HttpPost("groups/{autoGroupId}/handoff-approval")]
    [ProducesResponseType(typeof(공동구매수요모집인계승인응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> 인계승인(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        [FromBody] 공동구매수요모집인계승인요청 요청,
        [FromHeader(Name = "Idempotency-Key")] string? 요청멱등키,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(요청멱등키))
        {
            요청.요청멱등키 = 요청멱등키.Trim();
        }

        var 결과 = await _processManager.인계승인Async(
            자동집단Id,
            요청,
            현재관리자키(),
            cancellationToken);
        return Ok(결과);
    }

    [HttpPost("deadline-scan")]
    [ProducesResponseType(typeof(공동구매수요모집마감스캔응답), StatusCodes.Status200OK)]
    public async Task<IActionResult> 모집마감스캔(
        [FromQuery(Name = "maxItems")] int? 최대건수,
        CancellationToken cancellationToken)
    {
        var 결과 = await _processManager.모집마감스캔Async(
            최대건수: 최대건수,
            cancellationToken: cancellationToken);
        return Ok(결과);
    }

    private string 현재관리자키()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name
           ?? throw new UnauthorizedAccessException("관리자 식별자를 찾을 수 없습니다.");
}
