using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.ClientFeature,
    "공개 국내 공동구매 모집 목록 조회 상태",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.DomesticGroupPurchasePilot,
    Boundary = "목록 조회와 목록 항목 갱신만 담당하며 선택 상세나 참여 명령을 수행하지 않습니다.")]
public sealed partial class 공동구매공개목록ViewModel(
    I공동구매공개조회Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial IReadOnlyList<CommunityVoteResponse> 모집목록 { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial bool 초기화됨 { get; private set; }

    public bool 비어있음 => 초기화됨 && 모집목록.Count == 0;

    public Task<bool> 조회Async(
        string? communityScope = null,
        string? hsCode = null,
        CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var response = await service.목록조회Async(
                    communityScope,
                    hsCode,
                    token);
                모집목록 = response.Items
                    .Where(IsDomesticGroupPurchase)
                    .OrderByDescending(campaign => campaign.CreatedAtUtc)
                    .ToArray();
                초기화됨 = true;
            },
            "공개 공동구매 모집 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"공동구매 목록을 불러오지 못했습니다. 잠시 뒤 다시 시도해 주세요. {ex.Message}");

    public void 공동구매갱신(CommunityVoteResponse campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        if (!IsDomesticGroupPurchase(campaign))
        {
            return;
        }

        모집목록 = 모집목록
            .Where(item => item.Id != campaign.Id)
            .Append(campaign)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToArray();
        초기화됨 = true;
    }

    private static bool IsDomesticGroupPurchase(CommunityVoteResponse campaign)
        => string.Equals(
               campaign.VoteKind,
               CommunityVoteKindCodes.GroupPurchaseDemand,
               StringComparison.Ordinal)
           && !CommunityVoteWorkflowClassifier.IsGroupImport(campaign);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.ClientFeature,
    "campaignId 기준 공개 국내 공동구매 상세와 연결 의견 조회 상태",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.DomesticGroupPurchasePilot,
    Boundary = "선택한 한 모집의 공개 상세와 의견만 담당하며 원장 상태 전이나 참여 저장을 수행하지 않습니다.")]
public sealed partial class 공동구매공개상세ViewModel(
    I공동구매공개조회Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial Guid? 요청CampaignId { get; private set; }

    [ObservableProperty]
    public partial CommunityVoteResponse? 공동구매 { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<PlatformCommunityPostCommentResponse> 의견목록 { get; private set; } = [];

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        if (campaignId == Guid.Empty)
        {
            return Task.FromResult(유효성실패("조회할 공동구매 ID를 확인해 주세요."));
        }

        요청CampaignId = campaignId;
        찾을수없음 = false;
        공동구매 = null;
        의견목록 = [];

        return 작업실행Async(
            async token =>
            {
                var campaign = await service.상세조회Async(campaignId, token);
                if (campaign is null || CommunityVoteWorkflowClassifier.IsGroupImport(campaign))
                {
                    찾을수없음 = true;
                    return;
                }

                공동구매 = campaign;
                의견목록 = campaign.SourcePostId is long postId
                    ? await service.의견조회Async(postId, token)
                    : [];
            },
            "공동구매 상세 정보를 불러왔습니다.",
            cancellationToken,
            ex => $"공동구매 상세 정보를 불러오지 못했습니다. 잠시 뒤 다시 시도해 주세요. {ex.Message}");
    }

    public void 선택해제()
    {
        요청CampaignId = null;
        공동구매 = null;
        의견목록 = [];
        찾을수없음 = false;
        작업상태초기화();
    }

    public void 의견추가(PlatformCommunityPostCommentResponse comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        의견목록 = 의견목록.Append(comment).ToArray();
    }
}
