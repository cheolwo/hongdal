using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/votes")]
public sealed class CommunityVotesController : ControllerBase
{
    private readonly ICommunityVoteService _voteService;

    public CommunityVotesController(ICommunityVoteService voteService)
    {
        _voteService = voteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityVoteListResponse>> List(
        [FromQuery] string? appKey,
        [FromQuery] string? communityScope,
        CancellationToken cancellationToken)
    {
        return Ok(await _voteService.ListAsync(appKey, communityScope, cancellationToken));
    }

    [HttpGet("{voteId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid voteId, CancellationToken cancellationToken)
    {
        var vote = await _voteService.GetAsync(voteId, cancellationToken);
        return vote is null ? this.ToNotFoundProblem("커뮤니티 투표를 찾을 수 없습니다.") : Ok(vote);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] CommunityVoteCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vote = await _voteService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { voteId = vote.Id }, vote);
        }
        catch (InvalidOperationException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("{voteId:guid}/votes")]
    [AllowAnonymous]
    public async Task<IActionResult> CastVote(
        Guid voteId,
        [FromBody] CommunityVoteCastRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vote = await _voteService.CastVoteAsync(voteId, request, cancellationToken);
            return vote is null ? this.ToNotFoundProblem("커뮤니티 투표를 찾을 수 없습니다.") : Ok(vote);
        }
        catch (InvalidOperationException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("{voteId:guid}/close")]
    [AllowAnonymous]
    public async Task<IActionResult> Close(
        Guid voteId,
        [FromBody] CommunityVoteCloseRequest request,
        CancellationToken cancellationToken)
    {
        var vote = await _voteService.CloseAsync(voteId, request, cancellationToken);
        return vote is null ? this.ToNotFoundProblem("커뮤니티 투표를 찾을 수 없습니다.") : Ok(vote);
    }

    [HttpPost("{voteId:guid}/resolution-documents")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateResolutionDraft(
        Guid voteId,
        [FromBody] CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _voteService.CreateResolutionDraftAsync(voteId, request, cancellationToken);
            return document is null ? this.ToNotFoundProblem("커뮤니티 투표를 찾을 수 없습니다.") : Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("{voteId:guid}/resolution-documents/signatures")]
    [AllowAnonymous]
    public async Task<IActionResult> SignResolution(
        Guid voteId,
        [FromBody] CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _voteService.SignResolutionAsync(voteId, request, cancellationToken);
            return document is null ? this.ToNotFoundProblem("서명 가능한 결의문을 찾을 수 없습니다.") : Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("{voteId:guid}/resolution-documents/ready-to-sign")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkResolutionReadyToSign(
        Guid voteId,
        [FromBody] CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _voteService.MarkResolutionReadyToSignAsync(voteId, request, cancellationToken);
        return document is null ? this.ToNotFoundProblem("결의문을 찾을 수 없습니다.") : Ok(document);
    }
}
