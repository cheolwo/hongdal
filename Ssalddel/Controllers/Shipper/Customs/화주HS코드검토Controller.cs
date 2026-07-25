using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Customs;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Shipper.Customs;

[SsalddelApiOperation(SsalddelOperation.Browse)]
[ApiController]
[Authorize(Policy = "화주또는판매자전용")]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.CustomsAndTradeData)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Customs)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/shipper/customs/hs-reviews")]
public sealed class 화주HS코드검토Controller(I화주HS코드검토조회UseCase useCase) : ShipperControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] string? query,
        [FromQuery] int? businessCategory,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await useCase.목록Async(
            query,
            businessCategory,
            page,
            pageSize,
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{reviewId:long}")]
    public async Task<IActionResult> 상세(
        long reviewId,
        CancellationToken cancellationToken = default)
    {
        var result = await useCase.상세Async(reviewId, cancellationToken);
        return this.ToActionResult(result);
    }
}
