using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>마트 피킹 주문의 검색 조건, 서버 페이징과 목록 결과만 관리합니다.</summary>
public sealed partial class 마트피킹주문목록ViewModel(
    I마트피킹읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long? 창고Id { get; set; }

    [ObservableProperty]
    public partial string 작업상태 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(주문목록))]
    [NotifyPropertyChangedFor(nameof(전체건수))]
    [NotifyPropertyChangedFor(nameof(현재페이지))]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial 마트피킹주문목록응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<마트피킹주문요약응답> 주문목록 => 응답.Items;
    public int 전체건수 => 응답.TotalCount;
    public int 현재페이지 => 응답.Page;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 응답.PageSize)));
    public bool 결과없음 => 초기화됨 && 전체건수 == 0;
    public bool 검색조건사용중
        => !string.IsNullOrWhiteSpace(검색어)
           || 창고Id is > 0
           || !string.IsNullOrWhiteSpace(작업상태);

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(1, cancellationToken);

    public Task<bool> 페이지조회Async(int page, CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, page), cancellationToken);

    public Task<bool> 새로고침Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, 현재페이지), cancellationToken);

    public Task<bool> 조건초기화후조회Async(CancellationToken cancellationToken = default)
    {
        검색어 = string.Empty;
        창고Id = null;
        작업상태 = string.Empty;
        return 조회Async(cancellationToken);
    }

    public void 결과초기화()
    {
        응답 = new 마트피킹주문목록응답();
        초기화됨 = false;
        작업상태초기화();
    }

    private Task<bool> 조회CoreAsync(int page, CancellationToken cancellationToken)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.목록Async(new 마트피킹주문목록조회요청
                {
                    검색어 = 검색어,
                    창고Id = 창고Id,
                    작업상태 = 작업상태,
                    Page = page,
                    PageSize = 12
                }, token);
                초기화됨 = true;
            },
            "마트 피킹 주문 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"마트 피킹 주문 목록을 불러오지 못했습니다. {ex.Message}");
}

/// <summary>주소나 목록에서 선택한 정확한 orderId 한 건만 조회합니다.</summary>
public sealed partial class 마트피킹주문상세ViewModel(
    I마트피킹읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 요청OrderId { get; private set; }

    [ObservableProperty]
    public partial 마트피킹주문상세응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(long orderId, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return Task.FromResult(유효성실패("조회할 마트 주문 ID를 확인해 주세요."));
        }

        요청OrderId = orderId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(orderId, token);
                찾을수없음 = 상세 is null;
            },
            "마트 피킹 주문 상세를 불러왔습니다.",
            cancellationToken,
            ex => $"마트 피킹 주문 상세를 불러오지 못했습니다. {ex.Message}");
    }

    public void 선택해제()
    {
        요청OrderId = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>마트 피킹 주문 목록과 정확한 주문 상세 조회만 조립합니다.</summary>
public sealed class 마트피킹작업PageViewModel : 조립ViewModelBase
{
    public 마트피킹작업PageViewModel(
        마트피킹주문목록ViewModel list,
        마트피킹주문상세ViewModel detail)
    {
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 마트피킹주문목록ViewModel 목록 { get; }
    public 마트피킹주문상세ViewModel 상세 { get; }
    public bool 처리중 => 목록.처리중 || 상세.처리중;

    public Task<bool> 목록초기화Async(CancellationToken cancellationToken = default)
    {
        상세.선택해제();
        return 목록.조회Async(cancellationToken);
    }

    public Task<bool> 상세초기화Async(
        long orderId,
        CancellationToken cancellationToken = default)
        => 상세.조회Async(orderId, cancellationToken);

    public async Task<bool> 초기화Async(
        long? orderId,
        CancellationToken cancellationToken = default)
    {
        return orderId is > 0
            ? await 상세초기화Async(orderId.Value, cancellationToken)
            : await 목록초기화Async(cancellationToken);
    }

    public void 결과초기화()
    {
        목록.결과초기화();
        상세.선택해제();
    }
}
