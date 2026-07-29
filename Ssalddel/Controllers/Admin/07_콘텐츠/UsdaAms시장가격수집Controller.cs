using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Api,
    "USDA AMS My Market News의 산지·도매·소매 광고 가격을 보고서 단위로 수집·보관하는 관리자 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 수집을 실행하며 시장 단계 혼합, 자동 환율·포장 환산, 주문·견적·계약 실행은 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/us-market-news-prices")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("UsdaAmsMarketPriceCollectionController")]
public sealed class UsdaAms시장가격수집Controller(
    IUsdaAms시장가격ArchiveService archiveService) : ControllerBase
{
    [HttpPost("collections")]
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<UsdaAms시장가격수집응답>> 수집(
        [FromBody] UsdaAms시장가격수집요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await archiveService.CollectAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "USDA AMS 수집 범위를 확인해 주세요.",
                exception.Message));
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidOperationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                CreateProblem(
                    StatusCodes.Status503ServiceUnavailable,
                    "USDA AMS 공식 자료를 현재 수집하지 못했습니다.",
                    exception.Message));
        }
    }

    private static ProblemDetails CreateProblem(
        int status,
        string title,
        string detail)
        => new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
}
