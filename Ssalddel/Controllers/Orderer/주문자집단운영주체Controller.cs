using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[Route("api/v1/orderer/orderer-group-operating-entities")]
public sealed class 주문자집단운영주체Controller : OrdererControllerBase
{
    private readonly I주문자집단운영주체저장소 _운영주체Store;

    public 주문자집단운영주체Controller(I주문자집단운영주체저장소 운영주체Store)
    {
        _운영주체Store = 운영주체Store;
    }

    [HttpGet("{ordererGroupScopeKey}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(
        [FromRoute(Name = "ordererGroupScopeKey")] string 주문자집단배송권키,
        CancellationToken cancellationToken)
    {
        try
        {
            var 항목 = await _운영주체Store.배송권키로조회Async(주문자집단배송권키, cancellationToken);
            return 항목 is null
                ? this.ToNotFoundProblem("주문자 집단 운영 주체 프로필을 찾을 수 없습니다.")
                : Ok(주문자집단운영주체공개변환기.공개Dto로(항목));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
