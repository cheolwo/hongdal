using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Api,
    "기존 농사로 감자 자료를 단회 수집하고 최신 승인 자료의 보관 상태를 조회하는 관리자 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "기존 수집·저장 서비스를 재사용하며 자료 승인, 게임 규칙 승격, 원문·비밀값 공개와 자동 재시도는 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/nongsaro-potato")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("NongsaroPotatoCollectionController")]
public sealed class 농사로감자자료수집Controller(
    INongsaro감자ProfileArchiveService archiveService) : ControllerBase
{
    [HttpPost("collections")]
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<농사로감자자료상태Response>> 수집(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            // 클라이언트 입력으로 자료 승인이나 게임 소비 권한을 올리지 않는다.
            var archive = await archiveService.CollectAndArchiveAsync(false, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return Ok(상태(archive));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // 제공처 URL/인증값이 섞일 수 있는 예외는 응답이나 로그로 복사하지 않는다.
            // 저장 뒤 응답 실패도 가능하므로 저장되지 않았다고 단정하지 않는다.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 문제(
                StatusCodes.Status503ServiceUnavailable,
                "NongsaroPotatoCollectionUnavailable",
                "감자 자료 수집 결과를 확인하지 못했습니다.",
                "저장 여부는 별도로 확인해야 합니다. 자동으로 재시도하지 않았습니다."));
        }
    }

    [HttpGet("latest-approved")]
    [SsalddelApiContractName("GetLatestApproved")]
    public async Task<ActionResult<농사로감자자료상태Response>> 최신승인자료조회(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var archive = await archiveService.최신자료승인조회Async(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (archive is null || !archive.ApprovedForSimulationContext)
                return NotFound(문제(
                    StatusCodes.Status404NotFound,
                    "ApprovedNongsaroPotatoProfileUnavailable",
                    "조회 가능한 최신 승인 감자 자료가 없습니다.",
                    "자료 미확보 또는 최신 자료의 승인 보류 상태입니다."));
            return Ok(상태(archive));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 문제(
                StatusCodes.Status503ServiceUnavailable,
                "NongsaroPotatoReadUnavailable",
                "최신 승인 감자 자료를 조회하지 못했습니다.",
                "미확보로 확정하지 않았습니다. 자동으로 재시도하지 않았습니다."));
        }
    }

    private static 농사로감자자료상태Response 상태(Nongsaro감자ProfileArchive archive)
        => new(archive.Id, archive.Revision, archive.ApprovedForSimulationContext,
            archive.RetrievedAtUtc, archive.ArchivedAtUtc);

    private static ProblemDetails 문제(int status, string code, string title, string detail)
        => new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Extensions = { ["code"] = code }
        };
}

// 엔드포인트의 대상은 기존 감자 Profile 하나다. 원문·URL·hash·예외 문자열은 반환하지 않는다.
public sealed record 농사로감자자료상태Response(
    long ArchiveId,
    int Revision,
    bool ApprovedForSimulationContext,
    DateTime RetrievedAtUtc,
    DateTime ArchivedAtUtc);
