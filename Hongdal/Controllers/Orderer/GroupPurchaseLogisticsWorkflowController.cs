using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Route("api/v1/orderer/group-purchase-logistics-workflows")]
public sealed class GroupPurchaseLogisticsWorkflowController : ControllerBase
{
    private readonly IGroupPurchaseLogisticsWorkflowStore _store;

    public GroupPurchaseLogisticsWorkflowController(IGroupPurchaseLogisticsWorkflowStore store)
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
        var item = await _store.ResolveAsync(new GroupPurchaseLogisticsWorkflowQuery
        {
            ProductCategoryCode = productCategoryCode,
            TemperatureCode = temperatureCode,
            LogisticsMode = logisticsMode,
            SellerOriginType = sellerOriginType,
            OrdererGroupScopeType = ordererGroupScopeType,
            ActiveOnly = true
        }, cancellationToken);

        return item is null
            ? this.ToNotFoundProblem("조건에 맞는 공동주문 물류 흐름 정의를 찾을 수 없습니다.")
            : Ok(item);
    }
}
