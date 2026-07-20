using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed partial class 운송의뢰초안원장ViewModel(I출고예정검토페이지Service service) : 업무작업ViewModelBase
{
    [ObservableProperty] public partial long? 조회대상Id { get; private set; }
    [ObservableProperty] public partial 출고예정검토상세응답? 원장 { get; private set; }
    [ObservableProperty] public partial bool 대상없음 { get; private set; }

    public void 조회대상설정(long? id)
    {
        조회대상Id = id is > 0 ? id : null;
        원장 = null;
        대상없음 = false;
        작업상태초기화();
    }

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
    {
        if (조회대상Id is not > 0)
            return Task.FromResult(유효성실패("운송의뢰 초안을 작성할 출고예정 원장을 선택해 주세요."));

        원장 = null;
        대상없음 = false;
        return 작업실행Async(
            async token =>
            {
                원장 = await service.상세조회Async(조회대상Id.Value, token);
                대상없음 = 원장 is null;
            },
            "선택한 출고예정 원장을 같은 ID로 조회했습니다.",
            cancellationToken,
            ex => $"운송의뢰 초안의 출고예정 원장을 조회하지 못했습니다. {ex.Message}");
    }
}

public sealed record 운송의뢰로컬초안검토결과(
    long OutboundPlanId,
    string DraftReference,
    string DestinationSummary,
    DateTime PickupAt,
    DateTime ArrivalAt,
    string VehicleType,
    string HandlingNote);

public sealed partial class 운송의뢰초안작성ViewModel : ObservableObject
{
    public static IReadOnlyList<string> 차량유형목록 { get; } = ["1톤 카고", "1톤 냉장탑차", "2.5톤 카고", "2.5톤 냉장탑차", "냉동탑차"];

    [ObservableProperty] public partial 출고예정검토상세응답? 원장 { get; private set; }
    [ObservableProperty] public partial string 하차지요약 { get; set; } = string.Empty;
    [ObservableProperty] public partial DateTime? 희망상차일 { get; set; }
    [ObservableProperty] public partial TimeSpan? 희망상차시각 { get; set; }
    [ObservableProperty] public partial DateTime? 희망도착일 { get; set; }
    [ObservableProperty] public partial TimeSpan? 희망도착시각 { get; set; }
    [ObservableProperty] public partial string 차량유형 { get; set; } = string.Empty;
    [ObservableProperty] public partial string 취급메모 { get; set; } = string.Empty;
    [ObservableProperty] public partial bool 상품수량확인 { get; set; }
    [ObservableProperty] public partial string? 검증오류 { get; private set; }
    [ObservableProperty] public partial 운송의뢰로컬초안검토결과? 검토결과 { get; private set; }

    public bool 작성가능 => 원장?.CanStartTransportRequestDraft == true;
    public bool 검토완료 => 검토결과 is not null;

    public void 원장설정(출고예정검토상세응답? plan)
    {
        원장 = plan;
        하차지요약 = string.Empty;
        희망상차일 = DateTime.Today.AddDays(1);
        희망상차시각 = new TimeSpan(9, 0, 0);
        희망도착일 = DateTime.Today.AddDays(1);
        희망도착시각 = new TimeSpan(11, 0, 0);
        차량유형 = string.Empty;
        취급메모 = string.Empty;
        상품수량확인 = false;
        검토초기화();
        OnPropertyChanged(nameof(작성가능));
    }

    public bool 입력값검토()
    {
        검토초기화();
        if (!작성가능 || 원장 is null)
            return 실패("출고예정 검토 조건을 먼저 충족해 주세요.");

        var destination = 하차지요약.Trim();
        if (destination.Length is < 5 or > 200)
            return 실패("하차지 요약을 5자 이상 200자 이하로 입력해 주세요.");
        if (희망상차일 is null || 희망상차시각 is null || 희망도착일 is null || 희망도착시각 is null)
            return 실패("희망 상차·도착 일시를 모두 입력해 주세요.");

        var pickupAt = 희망상차일.Value.Date + 희망상차시각.Value;
        var arrivalAt = 희망도착일.Value.Date + 희망도착시각.Value;
        if (arrivalAt <= pickupAt)
            return 실패("희망 도착 일시는 희망 상차 일시보다 뒤여야 합니다.");
        if (!차량유형목록.Contains(차량유형, StringComparer.Ordinal))
            return 실패("지원하는 차량 유형을 선택해 주세요.");
        if (취급메모.Trim().Length > 300)
            return 실패("취급 메모는 300자 이하로 입력해 주세요.");
        if (!상품수량확인)
            return 실패("표시된 상품과 출고 수량을 확인해 주세요.");

        검토결과 = new 운송의뢰로컬초안검토결과(
            원장.OutboundPlanId,
            $"OUT-{원장.OutboundPlanId:D6}-LOCAL",
            destination,
            pickupAt,
            arrivalAt,
            차량유형,
            취급메모.Trim());
        OnPropertyChanged(nameof(검토완료));
        return true;
    }

    public void 다시수정() => 검토초기화();

    partial void On하차지요약Changed(string value) => 검토초기화();
    partial void On희망상차일Changed(DateTime? value) => 검토초기화();
    partial void On희망상차시각Changed(TimeSpan? value) => 검토초기화();
    partial void On희망도착일Changed(DateTime? value) => 검토초기화();
    partial void On희망도착시각Changed(TimeSpan? value) => 검토초기화();
    partial void On차량유형Changed(string value) => 검토초기화();
    partial void On취급메모Changed(string value) => 검토초기화();
    partial void On상품수량확인Changed(bool value) => 검토초기화();

    private bool 실패(string message)
    {
        검증오류 = message;
        return false;
    }

    private void 검토초기화()
    {
        검증오류 = null;
        검토결과 = null;
        OnPropertyChanged(nameof(검토완료));
    }
}

public sealed partial class 운송의뢰초안PageViewModel : 조립ViewModelBase
{
    public 운송의뢰초안PageViewModel(운송의뢰초안원장ViewModel ledger, 운송의뢰초안작성ViewModel draft)
    {
        원장 = 하위ViewModel등록(ledger);
        초안 = 하위ViewModel등록(draft);
    }

    public 운송의뢰초안원장ViewModel 원장 { get; }
    public 운송의뢰초안작성ViewModel 초안 { get; }
    [ObservableProperty] public partial bool 초기화됨 { get; private set; }

    public async Task<bool> 초기화Async(long? outboundPlanId, CancellationToken cancellationToken = default)
    {
        초기화됨 = false;
        원장.조회대상설정(outboundPlanId);
        if (outboundPlanId is not > 0)
        {
            초안.원장설정(null);
            초기화됨 = true;
            return true;
        }

        var loaded = await 원장.조회Async(cancellationToken);
        초안.원장설정(원장.원장);
        초기화됨 = true;
        return loaded && 원장.원장?.OutboundPlanId == outboundPlanId;
    }
}
