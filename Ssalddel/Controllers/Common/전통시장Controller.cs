using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.TraditionalMarkets;
using Ssalddel.Services.TraditionalMarkets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/traditional-markets")]
[SsalddelApiContractName("TraditionalMarketsController")]
public sealed class 전통시장Controller : CommunityControllerBase
{
    private readonly ITraditionalMarketCatalogService _catalogService;

    public 전통시장Controller(ITraditionalMarketCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("Search")]
    public async Task<ActionResult<TraditionalMarketListResponse>> 검색(
        [FromQuery] TraditionalMarketSearchRequest request,
        CancellationToken cancellationToken)
        => Ok(await _catalogService.SearchAsync(request, cancellationToken));

    [HttpGet("{marketCode}")]
    [AllowAnonymous]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<TraditionalMarketResponse>> 조회(
        string marketCode,
        CancellationToken cancellationToken)
    {
        var market = await _catalogService.GetAsync(marketCode, cancellationToken);
        return market is null ? NotFound() : Ok(market);
    }

    [HttpPost("sync")]
    [Authorize(Policy = "서버관리자전용")]
    [SsalddelApiContractName("Sync")]
    public async Task<ActionResult<TraditionalMarketSyncResponse>> 동기화(
        CancellationToken cancellationToken)
    {
        var result = await _catalogService.SyncAsync(cancellationToken);
        return result.Status == TraditionalMarketSyncStatuses.Completed
            ? Ok(result)
            : Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "전통시장 공공데이터 동기화에 실패했습니다.",
                detail: result.ErrorMessage);
    }
}
