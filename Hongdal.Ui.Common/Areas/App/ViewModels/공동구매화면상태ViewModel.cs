using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public static class 공동구매절차코드
{
    public const string 제안 = "proposal";
    public const string 거래경로 = "trade-route";
    public const string 수요모집 = "recruitment";
    public const string 거래상대연결 = "counterparty";
    public const string 공급조건협상 = "supply-negotiation";
    public const string 이의검토 = "objection";
    public const string 확정안 = "resolution";
    public const string 전자서명 = "signature";
    public const string 이행계획 = "fulfillment-plan";
    public const string 실행 = "execution";
    public const string 커머스 = "commerce";
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
        new(2, 공동구매절차코드.거래경로, "거래 경로", "국내 공동구매 또는 공동수입 분기"),
        new(3, 공동구매절차코드.수요모집, "수요 모집", "지역·픽업별 참여와 자동집단 수요 구성"),
        new(4, 공동구매절차코드.거래상대연결, "거래 상대 연결", "생산자·공동구매 대표 또는 해외 판매자 연결"),
        new(5, 공동구매절차코드.공급조건협상, "공급 조건 협상", "가격·수량·품질·통관·물류 조건 합의"),
        new(6, 공동구매절차코드.이의검토, "이의 검토", "협상 결과 공개와 최종 조정"),
        new(7, 공동구매절차코드.확정안, "확정안", "최종 조건과 결의문 작성"),
        new(8, 공동구매절차코드.전자서명, "전자서명", "당사자와 필수 구성원 동의"),
        new(9, 공동구매절차코드.이행계획, "이행 계획", "국내 물류 또는 공동수입 원장 계획"),
        new(10, 공동구매절차코드.실행, "주문 실행", "자동집단·주문원장과 후속 물류 실행"),
        new(11, 공동구매절차코드.커머스, "커머스 이행", "입고·재고·출품·출고 상태 확인")
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
    private string _진행단계코드 = 공동구매절차코드.제안;

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

    /// <summary>
    /// 현재 화면에서 선택한 단계와 별개로 실제 업무가 도달한 가장 앞선 단계입니다.
    /// </summary>
    public string 진행단계코드
    {
        get => _진행단계코드;
        private set => SetProperty(ref _진행단계코드, value);
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
        var inferredStageCode = 진행단계추론(campaign);
        진행단계코드 = inferredStageCode;
        현재단계코드 = inferredStageCode;
        OnPropertyChanged(nameof(선택된공동구매Id));
        OnPropertyChanged(nameof(공동구매선택됨));
    }

    public void 선택해제()
    {
        선택된공동구매 = null;
        의견목록 = [];
        현재단계코드 = 공동구매절차코드.제안;
        진행단계코드 = 공동구매절차코드.제안;
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
        var initialStageCode = 진행단계추론(campaign);
        현재단계코드 = initialStageCode;
        진행단계코드 = initialStageCode;
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

    public void 단계진행(string stageCode)
    {
        var target = 공동구매절차카탈로그.찾기(stageCode)
            ?? throw new ArgumentException(
                $"알 수 없는 공동구매 절차 코드입니다: {stageCode}",
                nameof(stageCode));
        var current = 공동구매절차카탈로그.찾기(진행단계코드);
        if (current is not null && target.순서 < current.순서)
        {
            throw new InvalidOperationException(
                $"공동구매 진행 단계는 이전 단계로 되돌릴 수 없습니다: {진행단계코드} -> {stageCode}");
        }

        진행단계코드 = stageCode;
        현재단계코드 = stageCode;
    }

    public bool 단계도달(string stageCode)
    {
        var target = 공동구매절차카탈로그.찾기(stageCode)
            ?? throw new ArgumentException(
                $"알 수 없는 공동구매 절차 코드입니다: {stageCode}",
                nameof(stageCode));
        var current = 공동구매절차카탈로그.찾기(진행단계코드);
        if (current is not null && target.순서 <= current.순서)
        {
            return false;
        }

        진행단계코드 = stageCode;
        현재단계코드 = stageCode;
        return true;
    }

    private static string 진행단계추론(CommunityVoteResponse campaign)
    {
        var explicitRouteCode = campaign.GroupPurchase?.TradeRouteCode;
        if (string.Equals(
                explicitRouteCode,
                CommunityGroupPurchaseTradeRouteCodes.ReviewRequired,
                StringComparison.OrdinalIgnoreCase))
        {
            return 공동구매절차코드.거래경로;
        }

        if (campaign.ResolutionDocument?.Status == CommunityVoteResolutionStatusCodes.Signed)
        {
            return 공동구매절차코드.이행계획;
        }

        if (campaign.ResolutionDocument?.Status is CommunityVoteResolutionStatusCodes.ReadyToSign
            or CommunityVoteResolutionStatusCodes.PartiallySigned)
        {
            return 공동구매절차코드.전자서명;
        }

        if (campaign.ResolutionDocument is not null)
        {
            return 공동구매절차코드.확정안;
        }

        return campaign.Status == CommunityVoteStatusCodes.Open
            ? 공동구매절차코드.수요모집
            : 공동구매절차코드.거래상대연결;
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
