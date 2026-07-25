using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[Route("api/v1/orderer/group-purchase-wishes/me")]
public sealed class 공동구매내원함Controller : OrdererControllerBase
{
    private readonly I공동구매내원함조회UseCase _내원함조회UseCase;

    public 공동구매내원함Controller(I공동구매내원함조회UseCase 내원함조회UseCase)
    {
        _내원함조회UseCase = 내원함조회UseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(공동구매내원함목록응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> 목록(CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var response = await _내원함조회UseCase.조회Async(currentUserId, cancellationToken);
        return Ok(response);
    }
}
