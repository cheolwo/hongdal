using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공동구매 수요를 상품·배송권 기준 자동집단으로 묶는 API를 담당합니다.
/// </summary>
public sealed partial class 공동구매자동집단ViewModel : 공동구매작업ViewModelBase, IDisposable
{
    private readonly I공동구매실행Service _service;
    private readonly 공동구매화면상태ViewModel _화면상태;
    private readonly 공동구매실행상태ViewModel _실행상태;
    private Guid? _대상공동구매Id;

    public 공동구매자동집단ViewModel(
        I공동구매실행Service service,
        공동구매화면상태ViewModel 화면상태,
        공동구매실행상태ViewModel 실행상태)
    {
        _service = service;
        _화면상태 = 화면상태;
        _실행상태 = 실행상태;
        _화면상태.PropertyChanged += 화면상태변경;
        _실행상태.PropertyChanged += 실행상태변경;
        공동구매변경동기화();
    }

    [ObservableProperty]
    public partial 공동구매자동집단조회조건 조회조건 { get; private set; } = new();

    [ObservableProperty]
    public partial IReadOnlyList<공동구매자동집단응답> 자동집단목록 { get; private set; } = [];

    [ObservableProperty]
    public partial 공동구매자동수요등록Command 수요초안 { get; private set; } = new();

    public 공동구매자동집단응답? 선택된자동집단 => _실행상태.선택된자동집단;
    public string? 실행공동구매Id => _실행상태.실행공동구매Id;

    public async Task<bool> 목록조회Async(CancellationToken cancellationToken = default)
        => await 작업실행Async(
            async token =>
            {
                자동집단목록 = await _service.자동집단목록조회Async(조회조건, token);
                var selectedId = _실행상태.선택된자동집단?.자동집단Id;
                if (!string.IsNullOrWhiteSpace(selectedId))
                {
                    var refreshed = 자동집단목록.FirstOrDefault(item =>
                        string.Equals(item.자동집단Id, selectedId, StringComparison.Ordinal));
                    if (refreshed is not null)
                    {
                        _실행상태.자동집단적용(refreshed);
                    }
                }
            },
            "자동집단 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"자동집단 목록을 불러오지 못했습니다. {ex.Message}");

    public bool 자동집단선택(string automaticGroupId)
    {
        var group = 자동집단목록.FirstOrDefault(item =>
            string.Equals(item.자동집단Id, automaticGroupId, StringComparison.Ordinal));
        if (group is null)
        {
            return 유효성실패("선택할 자동집단을 목록에서 찾아 주세요.");
        }

        _실행상태.자동집단적용(group);
        return true;
    }

    public async Task<bool> 수요등록Async(CancellationToken cancellationToken = default)
    {
        var campaign = _화면상태.선택된공동구매;
        if (campaign is null)
        {
            return 유효성실패("자동집단에 연결할 공동구매를 선택해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(수요초안.주문자키)
            || string.IsNullOrWhiteSpace(수요초안.주문자표시명))
        {
            return 유효성실패("주문자 식별키와 표시명을 입력해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(수요초안.상품키)
            || string.IsNullOrWhiteSpace(수요초안.상품명)
            || string.IsNullOrWhiteSpace(수요초안.배송권키))
        {
            return 유효성실패("상품과 배송권 정보를 확인해 주세요.");
        }

        if (수요초안.희망수량 <= 0 || string.IsNullOrWhiteSpace(수요초안.수량단위))
        {
            return 유효성실패("0보다 큰 희망 수량과 수량 단위를 입력해 주세요.");
        }

        if (string.IsNullOrWhiteSpace(수요초안.수요출처키))
        {
            수요초안.수요출처키 = $"community-vote:{campaign.Id:N}:{수요초안.주문자키.Trim()}";
            OnPropertyChanged(nameof(수요초안));
        }

        return await 작업실행Async(
            async token =>
            {
                var group = await _service.자동수요등록Async(수요초안, token)
                    ?? throw new InvalidOperationException("자동집단 수요 등록 응답이 비어 있습니다.");
                자동집단목록 = 자동집단목록
                    .Where(item => !string.Equals(item.자동집단Id, group.자동집단Id, StringComparison.Ordinal))
                    .Prepend(group)
                    .ToArray();
                _실행상태.자동집단적용(group);
            },
            "수요를 자동집단에 등록하고 실행 공동구매를 선택했습니다.",
            cancellationToken);
    }

    public void 입력변경알림()
        => OnPropertyChanged(nameof(수요초안));

    public void Dispose()
    {
        _화면상태.PropertyChanged -= 화면상태변경;
        _실행상태.PropertyChanged -= 실행상태변경;
        GC.SuppressFinalize(this);
    }

    private void 화면상태변경(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName is nameof(공동구매화면상태ViewModel.선택된공동구매)
                or nameof(공동구매화면상태ViewModel.선택된공동구매Id))
        {
            공동구매변경동기화();
        }
    }

    private void 실행상태변경(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName is nameof(공동구매실행상태ViewModel.선택된자동집단)
                or nameof(공동구매실행상태ViewModel.실행공동구매Id))
        {
            OnPropertyChanged(nameof(선택된자동집단));
            OnPropertyChanged(nameof(실행공동구매Id));
        }
    }

    private void 공동구매변경동기화()
    {
        var campaign = _화면상태.선택된공동구매;
        if (_대상공동구매Id == campaign?.Id)
        {
            return;
        }

        _대상공동구매Id = campaign?.Id;
        자동집단목록 = [];
        수요초안 = campaign is null ? new() : 수요초안생성(campaign);
        조회조건 = new 공동구매자동집단조회조건
        {
            상품키 = string.IsNullOrWhiteSpace(수요초안.상품키) ? null : 수요초안.상품키,
            배송권키 = string.IsNullOrWhiteSpace(수요초안.배송권키) ? null : 수요초안.배송권키
        };
        작업상태초기화();
    }

    private static 공동구매자동수요등록Command 수요초안생성(CommunityVoteResponse campaign)
    {
        var option = campaign.Options.FirstOrDefault(item => item.IsWinningOption)
            ?? campaign.Options.FirstOrDefault();
        var groupPurchase = campaign.GroupPurchase;
        // 자동집단 키는 여러 수요가 공유하는 카탈로그 키여야 하므로 옵션 ID를 임의 대체값으로 쓰지 않습니다.
        var productKey = First(option?.ProductKey);
        var productName = First(option?.Text, campaign.Title, productKey);
        var deliveryKey = First(groupPurchase?.ServiceAreaKey);
        return new 공동구매자동수요등록Command
        {
            커뮤니티게시글Id = campaign.SourcePostId,
            커뮤니티원장Id = campaign.CommunityLedgerId ?? string.Empty,
            상품키 = productKey,
            상품명 = productName,
            HS코드 = First(option?.HsCode, groupPurchase?.HsCode),
            온도코드 = First(option?.TemperatureCode, groupPurchase?.TemperatureCode, "상온"),
            물류방식 = First(option?.LogisticsMode, groupPurchase?.LogisticsMode, "LCL"),
            배송권키 = deliveryKey,
            배송권명 = First(groupPurchase?.ServiceAreaLabel, deliveryKey),
            // 응답의 RequestedQuantity는 전체 참여자 합계이므로 개인 수요 초안에 복사하지 않습니다.
            희망수량 = 1,
            수량단위 = First(option?.QuantityUnit, groupPurchase?.QuantityUnit, "개"),
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = $"'{campaign.Title}' 공동구매 확정 흐름에서 전달한 수요입니다.",
            목표참여자수 = groupPurchase?.MinimumParticipantCount > 0
                ? groupPurchase.MinimumParticipantCount
                : null,
            목표수량 = groupPurchase?.MinimumTotalQuantity > 0
                ? groupPurchase.MinimumTotalQuantity
                : null
        };
    }

    private static string First(params string?[] candidates)
        => candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}
