using System.Security.Claims;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V1_5, FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-logistics-workflows")]
public sealed class 공동구매물류워크플로우AdminController : ControllerBase
{
    private readonly I공동구매물류워크플로우저장소 _공동구매물류워크플로우Store;

    public 공동구매물류워크플로우AdminController(I공동구매물류워크플로우저장소 공동구매물류워크플로우Store)
    {
        _공동구매물류워크플로우Store = 공동구매물류워크플로우Store;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<IReadOnlyList<공동구매물류워크플로우정의Dto>>> 목록조회(
        [FromQuery] string? productCategoryCode,
        [FromQuery] string? temperatureCode,
        [FromQuery] string? logisticsMode,
        [FromQuery] string? sellerOriginType,
        [FromQuery] string? ordererGroupScopeType,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var items = await _공동구매물류워크플로우Store.ListAsync(new 공동구매물류워크플로우조회조건
        {
            품목분류코드 = productCategoryCode,
            온도코드 = temperatureCode,
            물류방식 = logisticsMode,
            판매자출처유형 = sellerOriginType,
            주문자집단배송권유형 = ordererGroupScopeType,
            활성만 = activeOnly
        }, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{workflowId}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(
        string workflowId,
        [FromQuery] string? version,
        CancellationToken cancellationToken)
    {
        var item = await _공동구매물류워크플로우Store.GetAsync(workflowId, version, cancellationToken);
        return item is null
            ? this.ToNotFoundProblem("공동주문 물류 흐름 정의를 찾을 수 없습니다.")
            : Ok(item);
    }

    [HttpPost]
    [SsalddelApiContractName("Upsert")]
    public async Task<IActionResult> 등록또는수정(
        [FromBody] 공동구매물류워크플로우저장요청 request,
        CancellationToken cancellationToken)
    {
        var item = await _공동구매물류워크플로우Store.UpsertAsync(request, ResolveUserId(), cancellationToken);
        return Ok(item);
    }

    [HttpPost("seed-defaults")]
    [SsalddelApiContractName("SeedDefaults")]
    public async Task<IActionResult> 기본값시드(CancellationToken cancellationToken)
    {
        await _공동구매물류워크플로우Store.SeedDefaultsAsync(cancellationToken);
        return NoContent();
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
