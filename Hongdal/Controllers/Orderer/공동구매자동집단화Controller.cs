using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Authorize]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Route("api/v1/orderer/group-purchase-auto-groups")]
public sealed class 공동구매자동집단화Controller : ControllerBase
{
    private readonly I공동구매자동집단화UseCase _useCase;

    public 공동구매자동집단화Controller(I공동구매자동집단화UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery(Name = "productKey")] string? 상품키,
        [FromQuery(Name = "deliveryScopeKey")] string? 배송권키,
        [FromQuery(Name = "currentStatus")] string? 현재상태,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록조회Async(new 공동구매자동집단조회조건
        {
            상품키 = 상품키,
            배송권키 = 배송권키,
            현재상태 = 현재상태
        }, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("demands")]
    public async Task<IActionResult> 수요등록(
        [FromBody] 공동구매자동수요등록Command command,
        CancellationToken cancellationToken)
    {
        command.주문자키 = CurrentUserId();
        if (string.IsNullOrWhiteSpace(command.주문자표시명))
        {
            command.주문자표시명 = User.Identity?.Name ?? command.주문자키;
        }

        var result = await _useCase.수요등록Async(command, cancellationToken);
        return this.ToActionResult(result);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
