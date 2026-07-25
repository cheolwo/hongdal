using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[Route("api/v1/orderer/group-purchase-demand-votes")]
public sealed class 공동구매수요투표Controller : OrdererControllerBase
{
    private readonly I커뮤니티투표UseCase _투표UseCase;
    private readonly I공동구매원장절차Service _원장절차Service;

    public 공동구매수요투표Controller(
        I커뮤니티투표UseCase 투표UseCase,
        I공동구매원장절차Service 원장절차Service)
    {
        _투표UseCase = 투표UseCase;
        _원장절차Service = 원장절차Service;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery(Name = "communityScope")] string? 커뮤니티범위,
        [FromQuery(Name = "hsCode")] string? HS코드,
        CancellationToken cancellationToken)
    {
        var result = await _투표UseCase.목록Async(
            "OrdererApp",
            커뮤니티범위,
            HS코드,
            cancellationToken);
        if (result.IsSuccess)
        {
            result.Value.Items = result.Value.Items
                .Where(x => x.VoteKind == CommunityVoteKindCodes.GroupPurchaseDemand)
                .ToArray();
        }

        return this.ToActionResult(result);
    }

    [HttpGet("{voteId:guid}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(
        [FromRoute(Name = "voteId")] Guid 투표Id,
        CancellationToken cancellationToken)
    {
        var result = await _투표UseCase.상세Async(투표Id, cancellationToken);
        if (result.IsSuccess && result.Value.VoteKind != CommunityVoteKindCodes.GroupPurchaseDemand)
        {
            return NotFound();
        }

        return this.ToActionResult(result);
    }

    [HttpGet("{voteId:guid}/ledger-progress")]
    [SsalddelApiContractName("GetLedgerProgress")]
    public async Task<IActionResult> 원장진행조회(
        [FromRoute(Name = "voteId")] Guid 투표Id,
        CancellationToken cancellationToken)
    {
        var 진행 = await _원장절차Service.조회Async(투표Id, cancellationToken);
        return 진행 is null ? NotFound() : Ok(진행);
    }

    [HttpPost("{voteId:guid}/ledger-progress")]
    [SsalddelApiContractName("AdvanceLedgerProgress")]
    public async Task<IActionResult> 원장절차진행(
        [FromRoute(Name = "voteId")] Guid 투표Id,
        [FromBody] CommunityGroupPurchaseLedgerProgressRequest 요청,
        CancellationToken cancellationToken)
    {
        var 사용자Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(사용자Id))
        {
            return Unauthorized();
        }

        try
        {
            var 진행 = await _원장절차Service.진행Async(
                투표Id,
                요청,
                사용자Id,
                cancellationToken);
            return 진행 is null ? NotFound() : Ok(진행);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost]
    [SsalddelApiContractName("Create")]
    public async Task<IActionResult> 생성(
        [FromBody] CommunityVoteCreateRequest 요청,
        CancellationToken cancellationToken)
    {
        var 배송권키 = User.FindFirstValue(주문자집단배송권ClaimTypes.ScopeKey);
        요청.AppKey = "OrdererApp";
        요청.VoteKind = CommunityVoteKindCodes.GroupPurchaseDemand;
        요청.CreatedByDisplayName = string.IsNullOrWhiteSpace(요청.CreatedByDisplayName)
            ? User.Identity?.Name ?? "공동구매 참여자"
            : 요청.CreatedByDisplayName;
        if (!string.IsNullOrWhiteSpace(배송권키))
        {
            if (string.IsNullOrWhiteSpace(요청.CommunityScope) || 요청.CommunityScope == "platform")
            {
                요청.CommunityScope = 배송권키;
            }

            if (요청.GroupPurchase is not null
                && string.IsNullOrWhiteSpace(요청.GroupPurchase.ServiceAreaKey)
                && 요청.GroupPurchase.ParticipationPolicyCode is CommunityVoteParticipationPolicyCodes.ServiceAreaOnly
                    or CommunityVoteParticipationPolicyCodes.Hybrid)
            {
                요청.GroupPurchase.ServiceAreaKey = 배송권키;
                요청.GroupPurchase.ServiceAreaLabel = User.FindFirstValue(주문자집단배송권ClaimTypes.DisplayName)
                    ?? 배송권키;
            }
        }

        var result = await _투표UseCase.생성Async(요청, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(상세조회), new { voteId = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{voteId:guid}/votes")]
    [SsalddelApiContractName("CastVote")]
    public async Task<IActionResult> 투표(
        [FromRoute(Name = "voteId")] Guid 투표Id,
        [FromBody] CommunityVoteCastRequest 요청,
        CancellationToken cancellationToken)
    {
        var 기존투표 = await _투표UseCase.상세Async(투표Id, cancellationToken);
        if (!기존투표.IsSuccess)
        {
            return this.ToActionResult(기존투표);
        }

        if (기존투표.Value.VoteKind != CommunityVoteKindCodes.GroupPurchaseDemand)
        {
            return NotFound();
        }

        var 사용자Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(사용자Id))
        {
            return Unauthorized();
        }

        var 배송권키 = User.FindFirstValue(주문자집단배송권ClaimTypes.ScopeKey);
        요청.VoterKey = 사용자Id;
        요청.VoterDisplayName = string.IsNullOrWhiteSpace(요청.VoterDisplayName)
            ? User.Identity?.Name ?? "공동구매 참여자"
            : 요청.VoterDisplayName;
        요청.CommunityMembershipReference = 배송권키;
        요청.ServiceAreaReference = 배송권키;

        var result = await _투표UseCase.투표Async(투표Id, 요청, cancellationToken);
        return this.ToActionResult(result);
    }
}
