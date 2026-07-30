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
    "USDA AMS Local Food Directories의 자발적 공개 사업체를 최소 필드로 수집·보관하는 관리자 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "상세 주소·좌표·담당자·연락처를 저장하지 않고, directory 등재를 인증·허가·거래 권한으로 해석하거나 자동 초대·선정하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/us-operator-profiles")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("UsdaAmsPublicBusinessCollectionController")]
public sealed class UsdaAms공개사업체수집Controller(
    IUsdaAms공개사업체ArchiveService archiveService) : ControllerBase
{
    [HttpPost("collections")]
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<UsdaAms공개사업체수집응답>> 수집(
        [FromBody] UsdaAms공개사업체수집요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await archiveService.CollectAsync(
                request,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "USDA AMS 공개 사업체 수집 범위를 확인해 주세요.",
                Detail = exception.Message
            });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or InvalidOperationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "USDA AMS 공개 사업체 자료를 현재 수집하지 못했습니다.",
                    Detail = exception.Message
                });
        }
    }
}
