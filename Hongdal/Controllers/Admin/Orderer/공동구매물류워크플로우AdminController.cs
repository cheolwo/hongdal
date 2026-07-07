using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/group-purchase-logistics-workflows")]
public sealed class 공동구매물류워크플로우AdminController : ControllerBase
{
    private readonly I공동구매물류워크플로우저장소 _store;

    public 공동구매물류워크플로우AdminController(I공동구매물류워크플로우저장소 store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<공동구매물류워크플로우정의Dto>>> List(
        [FromQuery] string? productCategoryCode,
        [FromQuery] string? temperatureCode,
        [FromQuery] string? logisticsMode,
        [FromQuery] string? sellerOriginType,
        [FromQuery] string? ordererGroupScopeType,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(new 공동구매물류워크플로우조회조건
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
    public async Task<IActionResult> Get(
        string workflowId,
        [FromQuery] string? version,
        CancellationToken cancellationToken)
    {
        var item = await _store.GetAsync(workflowId, version, cancellationToken);
        return item is null
            ? this.ToNotFoundProblem("공동주문 물류 흐름 정의를 찾을 수 없습니다.")
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] 공동구매물류워크플로우저장요청 request,
        CancellationToken cancellationToken)
    {
        var item = await _store.UpsertAsync(request, ResolveUserId(), cancellationToken);
        return Ok(item);
    }

    [HttpPost("seed-defaults")]
    public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
    {
        await _store.SeedDefaultsAsync(cancellationToken);
        return NoContent();
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
