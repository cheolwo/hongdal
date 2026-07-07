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
[Route("api/v1/orderer/group-purchase-commerce-fulfillment-plans")]
public sealed class GroupPurchaseCommerceFulfillmentPlanController : ControllerBase
{
    private readonly IGroupPurchaseCommerceFulfillmentPlanStore _store;

    public GroupPurchaseCommerceFulfillmentPlanController(IGroupPurchaseCommerceFulfillmentPlanStore store)
    {
        _store = store;
    }

    [HttpGet("by-group-purchase/{groupPurchaseId}")]
    public async Task<ActionResult<IReadOnlyList<GroupPurchaseCommerceFulfillmentPlanPublicDto>>> ListByGroupPurchase(
        string groupPurchaseId,
        CancellationToken cancellationToken)
    {
        var items = await _store.ListAsync(new GroupPurchaseCommerceFulfillmentPlanQuery
        {
            GroupPurchaseId = groupPurchaseId
        }, cancellationToken);

        return Ok(items.Select(GroupPurchaseCommerceFulfillmentPlanProjection.ToPublicDto).ToArray());
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentManagementNumber))
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: "documentManagementNumber is required.", statusCode: StatusCodes.Status400BadRequest);
        }

        var items = await _store.ListAsync(new GroupPurchaseCommerceFulfillmentPlanQuery
        {
            DocumentManagementNumber = documentManagementNumber
        }, cancellationToken);

        return items.Count == 0
            ? this.ToNotFoundProblem("문서관리번호에 해당하는 공동주문 커머스 풀필먼트 플랜을 찾을 수 없습니다.")
            : Ok(items.Select(GroupPurchaseCommerceFulfillmentPlanProjection.ToPublicDto).ToArray());
    }
}
