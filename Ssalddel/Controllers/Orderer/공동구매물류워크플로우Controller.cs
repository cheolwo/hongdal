using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V1_5, FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/orderer/group-purchase-logistics-workflows")]
public sealed class 공동구매물류워크플로우Controller : OrdererControllerBase
{
    private readonly I공동구매물류워크플로우저장소 _물류워크플로우Store;

    public 공동구매물류워크플로우Controller(I공동구매물류워크플로우저장소 물류워크플로우Store)
    {
        _물류워크플로우Store = 물류워크플로우Store;
    }

    [HttpGet("resolve")]
    [SsalddelApiContractName("Resolve")]
    public async Task<IActionResult> 물류흐름결정(
        [FromQuery(Name = "productCategoryCode")] string 품목분류코드,
        [FromQuery(Name = "temperatureCode")] string 온도코드,
        [FromQuery(Name = "logisticsMode")] string 물류방식,
        [FromQuery(Name = "sellerOriginType")] string 판매자출처유형,
        [FromQuery(Name = "ordererGroupScopeType")] string 주문자집단배송권유형,
        CancellationToken cancellationToken)
    {
        var 항목 = await _물류워크플로우Store.ResolveAsync(new 공동구매물류워크플로우조회조건
        {
            품목분류코드 = 품목분류코드,
            온도코드 = 온도코드,
            물류방식 = 물류방식,
            판매자출처유형 = 판매자출처유형,
            주문자집단배송권유형 = 주문자집단배송권유형,
            활성만 = true
        }, cancellationToken);

        return 항목 is null
            ? this.ToNotFoundProblem("조건에 맞는 공동주문 물류 흐름 정의를 찾을 수 없습니다.")
            : Ok(항목);
    }
}
