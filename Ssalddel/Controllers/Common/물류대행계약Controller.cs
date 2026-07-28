using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.WarehouseBilling;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V3_5)]
[SsalddelApiCapability(SsalddelCapability.WarehouseFulfillment)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[SsalddelApiAudience(SsalddelActor.CommunityMember)]
[SsalddelApiAudience(SsalddelActor.Orderer)]
[SsalddelApiAudience(SsalddelActor.OrdererGroupLeader)]
[SsalddelApiAudience(SsalddelActor.Seller)]
[SsalddelApiAudience(SsalddelActor.Shipper)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiOperation(SsalddelOperation.Record)]
[Route("api/v1/logistics-service-contracts")]
[SsalddelApiContractName("LogisticsServiceContractsController")]
public sealed class 물류대행계약Controller(
    I물류대행계약계획UseCase useCase) : ControllerBase
{
    [HttpPost("cost-preview")]
    [SsalddelApiContractName("CreateCostPreview")]
    public async Task<IActionResult> 비용미리보기(
        [FromBody] 물류대행비용미리보기요청 request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return this.ToAuthenticationProblem("로그인 사용자 정보를 확인할 수 없습니다.");
        }

        var displayName = User.FindFirstValue(ClaimTypes.Name)
                          ?? User.Identity?.Name
                          ?? "로그인 이용자";
        var result = await useCase.비용미리보기Async(
            request,
            userId,
            displayName,
            cancellationToken);
        return this.ToActionResult(result);
    }
}
