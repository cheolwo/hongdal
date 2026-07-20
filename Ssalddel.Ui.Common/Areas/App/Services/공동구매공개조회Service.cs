using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Ui.Common.Areas.App.Services;

/// <summary>
/// 로그인 여부와 무관하게 공개할 수 있는 공동구매 모집 조회만 제공합니다.
/// 제안, 참여, 마감과 서명 명령은 <see cref="I공동구매업무Service"/>의 보호 API 경계를 사용합니다.
/// </summary>
[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.ClientFeature,
    "공개 공동구매 모집 목록·상세·연결 의견 조회 계약",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.DomesticGroupPurchasePilot,
    Boundary = "공개 조회만 허용하며 참여 저장, 모집 마감, 계약, 결제와 물류 실행은 포함하지 않습니다.")]
public interface I공동구매공개조회Service
{
    Task<CommunityVoteListResponse> 목록조회Async(
        string? communityScope = null,
        string? hsCode = null,
        CancellationToken cancellationToken = default);

    Task<CommunityVoteResponse?> 상세조회Async(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> 의견조회Async(
        long postId,
        CancellationToken cancellationToken = default);
}

public sealed class PlatformCommunity공동구매공개조회Service(
    PlatformCommunityService communityService) : I공동구매공개조회Service
{
    public Task<CommunityVoteListResponse> 목록조회Async(
        string? communityScope = null,
        string? hsCode = null,
        CancellationToken cancellationToken = default)
        => communityService.GetPublicGroupPurchaseVotesAsync(
            communityScope,
            hsCode,
            cancellationToken);

    public Task<CommunityVoteResponse?> 상세조회Async(
        Guid campaignId,
        CancellationToken cancellationToken = default)
        => communityService.GetPublicGroupPurchaseVoteAsync(campaignId, cancellationToken);

    public Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> 의견조회Async(
        long postId,
        CancellationToken cancellationToken = default)
        => communityService.GetCommentsAsync(postId, cancellationToken);
}
