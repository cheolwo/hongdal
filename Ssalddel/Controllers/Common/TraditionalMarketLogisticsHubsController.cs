using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Filters;
using Ssalddel.Services.TraditionalMarkets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(
    SsalddelProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[Route("api/v1/traditional-market-logistics-hubs")]
public sealed class TraditionalMarketLogisticsHubsController : ControllerBase
{
    private readonly ITraditionalMarketLogisticsHubService _service;

    public TraditionalMarketLogisticsHubsController(ITraditionalMarketLogisticsHubService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<TraditionalMarketLogisticsHubListResponse>> Search(
        [FromQuery] TraditionalMarketLogisticsHubSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.SearchAsync(request, false, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "물류 거점 검색 조건이 올바르지 않습니다.",
                detail: ex.Message);
        }
    }

    [HttpGet("{marketCode}")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> Get(
        string marketCode,
        CancellationToken cancellationToken)
    {
        var hub = await _service.GetAsync(marketCode, false, cancellationToken);
        return hub is null ? NotFound() : Ok(hub);
    }
}
