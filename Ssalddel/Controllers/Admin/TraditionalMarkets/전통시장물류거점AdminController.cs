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
[SsalddelApiContractName("TraditionalMarketLogisticsHubsAdminController")]
public sealed class 전통시장물류거점AdminController : ControllerBase
{
    private readonly ITraditionalMarketLogisticsHubService _전통시장물류거점Service;

    public 전통시장물류거점AdminController(ITraditionalMarketLogisticsHubService 전통시장물류거점Service)
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
            return Ok(await _전통시장물류거점Service.SearchAsync(request, true, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpGet("{marketCode}")]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> 상세조회(
        string marketCode,
        CancellationToken cancellationToken)
    {
        var hub = await _전통시장물류거점Service.GetAsync(marketCode, true, cancellationToken);
        return hub is null ? NotFound() : Ok(hub);
    }

    [HttpPut("{marketCode}")]
    [SsalddelApiContractName("Upsert")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> 등록또는수정(
        string marketCode,
        [FromBody] TraditionalMarketLogisticsHubUpsertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _전통시장물류거점Service.UpsertAsync(
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
    [SsalddelApiContractName("ChangeStatus")]
    public async Task<ActionResult<TraditionalMarketLogisticsHubResponse>> 상태변경(
        string marketCode,
        [FromBody] TraditionalMarketLogisticsHubStatusChangeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _전통시장물류거점Service.ChangeStatusAsync(
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
