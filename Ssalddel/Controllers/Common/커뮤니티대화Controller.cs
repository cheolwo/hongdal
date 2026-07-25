using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.Api,
    "원장·다이어그램 대화방과 메시지 조회 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.ClosedLoop,
    Boundary = "참여자와 공개 범위를 확인하지 않은 비공개 대화 노출을 허용하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/diagram-conversations")]
public sealed class 커뮤니티대화Controller : CommunityControllerBase
{
    private readonly I커뮤니티대화UseCase _useCase;

    public 커뮤니티대화Controller(I커뮤니티대화UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("ListDiagramConversations")]
    public async Task<IActionResult> 다이어그램대화목록조회(
        [FromQuery] string? communityId,
        [FromQuery] string? ledgerId,
        [FromQuery] string? diagramId,
        [FromQuery] string? participantUserId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.다이어그램대화방목록Async(
            communityId,
            ledgerId,
            diagramId,
            participantUserId,
            limit,
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{roomId}/messages")]
    [AllowAnonymous]
    [SsalddelApiContractName("ListDiagramMessages")]
    public async Task<IActionResult> 다이어그램메시지목록조회(
        [FromRoute] string roomId,
        [FromQuery] int limit = 80,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.다이어그램메시지목록Async(roomId, limit, cancellationToken);
        return this.ToActionResult(result);
    }
}
