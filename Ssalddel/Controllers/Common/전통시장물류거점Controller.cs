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
[SsalddelApiContractName("TraditionalMarketLogisticsHubsController")]
public sealed class 전통시장물류거점Controller : ControllerBase
{
    private readonly ITraditionalMarketLogisticsHubService _전통시장물류거점Service;

    public 전통시장물류거점Controller(ITraditionalMarketLogisticsHubService 전통시장물류거점Service)
    {
        _전통시장물류거점Service = 전통시장물류거점Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Search")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubListResponse>> 검색(
        [FromQuery] TraditionalMarketLogisticsHubSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _전통시장물류거점Service.SearchAsync(request, false, cancellationToken));
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
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> 조회(
        string marketCode,
        CancellationToken cancellationToken)
    {
        var hub = await _전통시장물류거점Service.GetAsync(marketCode, false, cancellationToken);
        return hub is null ? NotFound() : Ok(hub);
    }
}
