using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Route("api/v1/orderer/group-purchase-logistics-workflows")]
public sealed class 공동구매물류워크플로우Controller : ControllerBase
{
    private readonly I공동구매물류워크플로우저장소 _store;

    public 공동구매물류워크플로우Controller(I공동구매물류워크플로우저장소 store)
    {
        _store = store;
    }

    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve(
        [FromQuery] string productCategoryCode,
        [FromQuery] string temperatureCode,
        [FromQuery] string logisticsMode,
        [FromQuery] string sellerOriginType,
        [FromQuery] string ordererGroupScopeType,
        CancellationToken cancellationToken)
    {
        var item = await _store.ResolveAsync(new 공동구매물류워크플로우조회조건
        {
            품목분류코드 = productCategoryCode,
            온도코드 = temperatureCode,
            물류방식 = logisticsMode,
            판매자출처유형 = sellerOriginType,
            주문자집단배송권유형 = ordererGroupScopeType,
            활성만 = true
        }, cancellationToken);

        return item is null
            ? this.ToNotFoundProblem("조건에 맞는 공동주문 물류 흐름 정의를 찾을 수 없습니다.")
            : Ok(item);
    }
}
