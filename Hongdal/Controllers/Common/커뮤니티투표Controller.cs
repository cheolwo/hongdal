using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/votes")]
public sealed class 커뮤니티투표Controller : ControllerBase
{
    private readonly I커뮤니티투표UseCase _useCase;

    public 커뮤니티투표Controller(I커뮤니티투표UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? appKey,
        [FromQuery] string? communityScope,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(appKey, communityScope, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{voteId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid voteId, CancellationToken cancellationToken)
    {
        var result = await _useCase.상세Async(voteId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] CommunityVoteCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (request.VoteKind == CommunityVoteKindCodes.GroupPurchaseDemand)
        {
            return BadRequest("공동구매 수요 투표는 /api/v1/orderer/group-purchase-demand-votes API를 사용해야 합니다.");
        }

        var result = await _useCase.생성Async(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { voteId = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/votes")]
    [AllowAnonymous]
    public async Task<IActionResult> CastVote(
        Guid voteId,
        [FromBody] CommunityVoteCastRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _useCase.상세Async(voteId, cancellationToken);
        if (existing.IsSuccess && existing.Value.VoteKind == CommunityVoteKindCodes.GroupPurchaseDemand)
        {
            return BadRequest("공동구매 수요 참여는 주문자 공동구매 투표 API를 사용해야 합니다.");
        }

        var result = await _useCase.투표Async(voteId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/close")]
    [AllowAnonymous]
    public async Task<IActionResult> Close(
        Guid voteId,
        [FromBody] CommunityVoteCloseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.마감Async(voteId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/resolution-documents")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateResolutionDraft(
        Guid voteId,
        [FromBody] CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.결의문초안생성Async(voteId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/resolution-documents/signatures")]
    [AllowAnonymous]
    public async Task<IActionResult> SignResolution(
        Guid voteId,
        [FromBody] CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.결의문서명Async(voteId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/resolution-documents/ready-to-sign")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkResolutionReadyToSign(
        Guid voteId,
        [FromBody] CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.결의문서명가능전환Async(voteId, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
