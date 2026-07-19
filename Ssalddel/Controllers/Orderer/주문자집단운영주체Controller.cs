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
[Route("api/v1/orderer/orderer-group-operating-entities")]
public sealed class 주문자집단운영주체Controller : ControllerBase
{
    private readonly I주문자집단운영주체저장소 _store;

    public 주문자집단운영주체Controller(I주문자집단운영주체저장소 store)
    {
        _store = store;
    }

    [HttpGet("{ordererGroupScopeKey}")]
    public async Task<IActionResult> Get(string ordererGroupScopeKey, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.배송권키로조회Async(ordererGroupScopeKey, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("주문자 집단 운영 주체 프로필을 찾을 수 없습니다.")
                : Ok(주문자집단운영주체공개변환기.공개Dto로(item));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
