using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Filters;
using Ssalddel.Services.TraditionalMarkets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.TraditionalMarkets;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiVersion(
    SsalddelProductVersion.V2_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[Route("api/v1/admin/traditional-market-logistics-hubs")]
public sealed class TraditionalMarketLogisticsHubsAdminController : ControllerBase
{
    private readonly ITraditionalMarketLogisticsHubService _service;

    public TraditionalMarketLogisticsHubsAdminController(ITraditionalMarketLogisticsHubService service)
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
            return Ok(await _service.SearchAsync(request, true, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpGet("{marketCode}")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> Get(
        string marketCode,
        CancellationToken cancellationToken)
    {
        var hub = await _service.GetAsync(marketCode, true, cancellationToken);
        return hub is null ? NotFound() : Ok(hub);
    }

    [HttpPut("{marketCode}")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> Upsert(
        string marketCode,
        [FromBody] TraditionalMarketLogisticsHubUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.UpsertAsync(
                marketCode,
                request,
                CurrentUserId(),
                cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (TraditionalMarketLogisticsHubConcurrencyException ex)
        {
            return ConflictProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpPost("{marketCode}/status")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> ChangeStatus(
        string marketCode,
        [FromBody] TraditionalMarketLogisticsHubStatusChangeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.ChangeStatusAsync(
                marketCode,
                request,
                CurrentUserId(),
                cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (TraditionalMarketLogisticsHubConcurrencyException ex)
        {
            return ConflictProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "system";

    private ObjectResult BadRequestProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "전통시장 물류 거점 요청이 올바르지 않습니다.",
            detail: detail);

    private ObjectResult NotFoundProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "전통시장 물류 거점을 찾을 수 없습니다.",
            detail: detail);

    private ObjectResult ConflictProblem(string detail)
        => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "전통시장 물류 거점 정보가 이미 변경되었습니다.",
            detail: detail);
}
