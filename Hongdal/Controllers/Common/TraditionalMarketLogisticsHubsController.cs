using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.TraditionalMarkets;
using Hongdal.Filters;
using Hongdal.Services.TraditionalMarkets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Common;

[ApiController]
[AllowAnonymous]
[HongdalApiVersion(
    HongdalProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
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
