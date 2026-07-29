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
    "키가 필요 없는 해외 공식 농수산 가격을 원통화·원단위로 수집·보관하는 관리자 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 수집을 실행하며 자동 환율·중량 환산, 국가 간 순위, 주문·견적·계약 실행은 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/international-agricultural-prices")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("InternationalAgriculturalPriceCollectionController")]
public sealed class 국제농수산가격수집Controller(
    I국제농수산가격ArchiveService 국제가격ArchiveService) : ControllerBase
{
    [HttpPost("collections")]
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<국제농수산가격수집응답>> 수집(
        [FromBody] 국제농수산가격수집요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await 국제가격ArchiveService.CollectAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "국제 가격 수집 범위를 확인해 주세요.",
                exception.Message));
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidOperationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                CreateProblem(
                    StatusCodes.Status503ServiceUnavailable,
                    "해외 공식 가격 자료를 현재 수집하지 못했습니다.",
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
