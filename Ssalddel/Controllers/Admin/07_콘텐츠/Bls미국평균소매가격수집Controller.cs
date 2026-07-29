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
    "BLS CPI Average Price Data의 미국 식품 월평균 소매가격을 키 없이 수집·보관하는 관리자 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 수집을 실행하며 자동 커뮤니티 게시, 환율·중량 환산, 주문·견적·계약 실행은 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/us-retail-average-prices")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("BlsUsRetailAveragePriceCollectionController")]
public sealed class Bls미국평균소매가격수집Controller(
    IBls평균소매가격ArchiveService 평균소매가격ArchiveService) : ControllerBase
{
    [HttpPost("collections")]
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<Bls평균소매가격수집응답>> 수집(
        [FromBody] Bls평균소매가격수집요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await 평균소매가격ArchiveService.CollectAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "BLS 수집 범위를 확인해 주세요.",
                exception.Message));
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidOperationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                CreateProblem(
                    StatusCodes.Status503ServiceUnavailable,
                    "BLS 공식 자료를 현재 수집하지 못했습니다.",
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
