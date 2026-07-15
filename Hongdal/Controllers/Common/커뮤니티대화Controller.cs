using Hongdal.ApiMetadata;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V0_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/diagram-conversations")]
public sealed class 커뮤니티대화Controller : ControllerBase
{
    private readonly I커뮤니티대화UseCase _useCase;

    public 커뮤니티대화Controller(I커뮤니티대화UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListDiagramConversations(
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
    public async Task<IActionResult> ListDiagramMessages(
        [FromRoute] string roomId,
        [FromQuery] int limit = 80,
        CancellationToken cancellationToken = default)
    {
        var result = await _useCase.다이어그램메시지목록Async(roomId, limit, cancellationToken);
        return this.ToActionResult(result);
    }
}
