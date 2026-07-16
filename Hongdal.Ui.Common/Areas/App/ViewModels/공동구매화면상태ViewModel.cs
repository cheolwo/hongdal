using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public static class 공동구매절차코드
{
    public const string 제안 = "proposal";
    public const string 수요모집 = "recruitment";
    public const string 이의검토 = "objection";
    public const string 확정안 = "resolution";
    public const string 전자서명 = "signature";
    public const string 실행 = "execution";
}

public enum 공동구매절차단계상태
{
    대기,
    진행중,
    완료
}

public sealed record 공동구매절차단계정의(
    int 순서,
    string 코드,
    string 제목,
    string 설명);

public sealed record 공동구매절차단계표시(
    int 순서,
    string 코드,
    string 제목,
    string 설명,
    공동구매절차단계상태 상태,
    bool 선택됨)
{
    public string 상태문구 => 상태 switch
    {
        공동구매절차단계상태.완료 => $"완료 · {설명}",
        공동구매절차단계상태.진행중 => $"진행 중 · {설명}",
        _ => $"대기 · {설명}"
    };
}

public static class 공동구매절차카탈로그
{
    public static IReadOnlyList<공동구매절차단계정의> 전체 { get; } =
    [
        new(1, 공동구매절차코드.제안, "제안 글", "상품과 운영 조건 공개"),
        new(2, 공동구매절차코드.수요모집, "수요 모집", "지역·픽업별 참여"),
        new(3, 공동구매절차코드.이의검토, "이의 검토", "단계별 의견과 조정"),
        new(4, 공동구매절차코드.확정안, "확정안", "모집 마감과 결의문"),
        new(5, 공동구매절차코드.전자서명, "전자서명", "구성원 전원 동의"),
        new(6, 공동구매절차코드.실행, "실행", "구매·물류 업무 전달")
    ];

    public static 공동구매절차단계정의? 찾기(string? code)
        => 전체.FirstOrDefault(stage =>
            string.Equals(stage.코드, code, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// 여러 공동구매 하위 ViewModel이 공유하는 목록, 선택 항목, 의견과 현재 단계입니다.
/// 페이지마다 별도 복사본을 만들지 않아 사용자 흐름 중 선택 상태가 어긋나는 것을 막습니다.
/// </summary>
public sealed class 공동구매화면상태ViewModel : ObservableObject
{
    private IReadOnlyList<CommunityVoteResponse> _공동구매목록 = [];
    private CommunityVoteResponse? _선택된공동구매;
    private IReadOnlyList<PlatformCommunityPostCommentResponse> _의견목록 = [];
    private string _현재단계코드 = 공동구매절차코드.제안;

    public IReadOnlyList<CommunityVoteResponse> 공동구매목록
    {
        get => _공동구매목록;
        private set => SetProperty(ref _공동구매목록, value);
    }

    public CommunityVoteResponse? 선택된공동구매
    {
        get => _선택된공동구매;
        private set => SetProperty(ref _선택된공동구매, value);
    }

    public IReadOnlyList<PlatformCommunityPostCommentResponse> 의견목록
    {
        get => _의견목록;
        private set => SetProperty(ref _의견목록, value);
    }

    public string 현재단계코드
    {
        get => _현재단계코드;
        private set => SetProperty(ref _현재단계코드, value);
    }

    public Guid? 선택된공동구매Id => 선택된공동구매?.Id;
    public bool 공동구매선택됨 => 선택된공동구매 is not null;

    public void 목록적용(IEnumerable<CommunityVoteResponse> campaigns)
    {
        ArgumentNullException.ThrowIfNull(campaigns);
        공동구매목록 = campaigns.ToArray();
    }

    public void 선택적용(
        CommunityVoteResponse campaign,
        IEnumerable<PlatformCommunityPostCommentResponse>? comments = null)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        ReplaceCampaign(campaign);
        선택된공동구매 = campaign;
        의견목록 = comments?.ToArray() ?? [];
        OnPropertyChanged(nameof(선택된공동구매Id));
        OnPropertyChanged(nameof(공동구매선택됨));
    }

    public void 선택해제()
    {
        선택된공동구매 = null;
        의견목록 = [];
        현재단계코드 = 공동구매절차코드.제안;
        OnPropertyChanged(nameof(선택된공동구매Id));
        OnPropertyChanged(nameof(공동구매선택됨));
    }

    public void 새공동구매적용(CommunityVoteResponse campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        공동구매목록 = 공동구매목록
            .Where(item => item.Id != campaign.Id)
            .Prepend(campaign)
            .ToArray();
        선택된공동구매 = campaign;
        의견목록 = [];
        현재단계코드 = 공동구매절차코드.수요모집;
        OnPropertyChanged(nameof(선택된공동구매Id));
        OnPropertyChanged(nameof(공동구매선택됨));
    }

    public void 공동구매갱신(CommunityVoteResponse campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        ReplaceCampaign(campaign);

        if (선택된공동구매?.Id == campaign.Id)
        {
            _선택된공동구매 = campaign;
            OnPropertyChanged(nameof(선택된공동구매));
            OnPropertyChanged(nameof(선택된공동구매Id));
            OnPropertyChanged(nameof(공동구매선택됨));
        }
    }

    public void 의견추가(PlatformCommunityPostCommentResponse comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        의견목록 = 의견목록.Append(comment).ToArray();
    }

    public void 단계선택(string stageCode)
    {
        if (공동구매절차카탈로그.찾기(stageCode) is null)
        {
            throw new ArgumentException($"알 수 없는 공동구매 절차 코드입니다: {stageCode}", nameof(stageCode));
        }

        현재단계코드 = stageCode;
    }

    private void ReplaceCampaign(CommunityVoteResponse campaign)
    {
        var replaced = false;
        var items = 공동구매목록
            .Select(item =>
            {
                if (item.Id != campaign.Id)
                {
                    return item;
                }

                replaced = true;
                return campaign;
            })
            .ToList();

        if (!replaced)
        {
            items.Insert(0, campaign);
        }

        공동구매목록 = items;
    }
}
