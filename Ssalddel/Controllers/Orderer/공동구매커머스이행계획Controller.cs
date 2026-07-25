using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow, WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[Route("api/v1/orderer/group-purchase-commerce-fulfillment-plans")]
public sealed class 공동구매커머스이행계획Controller : OrdererControllerBase
{
    private readonly I공동구매커머스이행계획저장소 _이행계획Store;

    public 공동구매커머스이행계획Controller(I공동구매커머스이행계획저장소 이행계획Store)
    {
        _이행계획Store = 이행계획Store;
    }

    [HttpGet("by-group-purchase/{groupPurchaseId}")]
    [SsalddelApiContractName("ListByGroupPurchase")]
    public async Task<ActionResult<IReadOnlyList<공동구매커머스이행계획공개Dto>>> 공동구매별목록조회(
        [FromRoute(Name = "groupPurchaseId")] string 공동구매Id,
        CancellationToken cancellationToken)
    {
        var 항목목록 = await _이행계획Store.ListAsync(new 공동구매커머스이행계획조회조건
        {
            공동구매Id = 공동구매Id
        }, cancellationToken);

        return Ok(항목목록.Select(공동구매커머스이행계획공개변환기.ToPublicDto).ToArray());
    }

    [HttpGet("lookup")]
    [SsalddelApiContractName("Lookup")]
    public async Task<IActionResult> 문서관리번호조회(
        [FromQuery(Name = "documentManagementNumber")] string 문서관리번호,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(문서관리번호))
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: "documentManagementNumber is required.", statusCode: StatusCodes.Status400BadRequest);
        }

        var 항목목록 = await _이행계획Store.ListAsync(new 공동구매커머스이행계획조회조건
        {
            문서관리번호 = 문서관리번호
        }, cancellationToken);

        return 항목목록.Count == 0
            ? this.ToNotFoundProblem("문서관리번호에 해당하는 공동주문 커머스 풀필먼트 플랜을 찾을 수 없습니다.")
            : Ok(항목목록.Select(공동구매커머스이행계획공개변환기.ToPublicDto).ToArray());
    }
}
