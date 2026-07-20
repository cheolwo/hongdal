using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Food;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>로그인 주문자의 음식 주문 검색·상태 필터·서버 페이징만 담당합니다.</summary>
public sealed partial class 주문자음식주문목록ViewModel(
    I주문자음식주문읽기Service service) : 업무작업ViewModelBase
{
    private const int DefaultPageSize = 12;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(검색조건있음))]
    [NotifyPropertyChangedFor(nameof(원장없음))]
    [NotifyPropertyChangedFor(nameof(검색결과없음))]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(검색조건있음))]
    [NotifyPropertyChangedFor(nameof(원장없음))]
    [NotifyPropertyChangedFor(nameof(검색결과없음))]
    public partial string? 상태필터 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(주문목록))]
    [NotifyPropertyChangedFor(nameof(전체건수))]
    [NotifyPropertyChangedFor(nameof(현재페이지))]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    [NotifyPropertyChangedFor(nameof(원장없음))]
    [NotifyPropertyChangedFor(nameof(검색결과없음))]
    public partial 주문자음식주문목록응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(원장없음))]
    [NotifyPropertyChangedFor(nameof(검색결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<주문자음식주문요약응답> 주문목록 => 응답.Items;
    public int 전체건수 => 응답.TotalCount;
    public int 현재페이지 => 응답.Page;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 응답.PageSize)));
    public bool 검색조건있음 => !string.IsNullOrWhiteSpace(검색어) || !string.IsNullOrWhiteSpace(상태필터);
    public bool 원장없음 => 초기화됨 && 전체건수 == 0 && !검색조건있음;
    public bool 검색결과없음 => 초기화됨 && 전체건수 == 0 && 검색조건있음;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 페이지조회Async(1, cancellationToken);

    public Task<bool> 페이지조회Async(int page, CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.목록Async(new 주문자음식주문목록조회요청
                {
                    검색어 = 검색어,
                    상태 = 상태필터,
                    Page = Math.Max(1, page),
                    PageSize = DefaultPageSize
                }, token);
                초기화됨 = true;
            },
            "내 음식 주문 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"내 음식 주문 목록을 불러오지 못했습니다. {ex.Message}");

    public void 필터초기화()
    {
        검색어 = string.Empty;
        상태필터 = null;
    }

    public void 세션초기화()
    {
        응답 = new 주문자음식주문목록응답();
        초기화됨 = false;
        필터초기화();
        작업상태초기화();
    }
}

/// <summary>명시적으로 선택한 정확한 orderNo 한 건의 소유자 상세만 담당합니다.</summary>
public sealed partial class 주문자음식주문상세ViewModel(
    I주문자음식주문읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string? 요청OrderNo { get; private set; }

    [ObservableProperty]
    public partial 주문자음식주문상세응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(string orderNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNo))
        {
            return Task.FromResult(유효성실패("조회할 음식 주문번호를 확인해 주세요."));
        }

        요청OrderNo = orderNo.Trim();
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(요청OrderNo, token);
                찾을수없음 = 상세 is null;
            },
            "음식 주문 상세를 불러왔습니다.",
            cancellationToken,
            ex => $"음식 주문 상세를 불러오지 못했습니다. {ex.Message}");
    }

    public void 선택해제()
    {
        요청OrderNo = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>기능 접근, 주문자 인증, 음식 주문 목록과 정확한 상세를 조립합니다.</summary>
public sealed class 주문자음식주문PageViewModel : 조립ViewModelBase
{
    public 주문자음식주문PageViewModel(
        음식배달페이지접근ViewModel access,
        주문자앱인증ViewModel authentication,
        주문자음식주문목록ViewModel list,
        주문자음식주문상세ViewModel detail)
    {
        접근 = 하위ViewModel등록(access);
        인증 = 하위ViewModel등록(authentication);
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 음식배달페이지접근ViewModel 접근 { get; }
    public 주문자앱인증ViewModel 인증 { get; }
    public 주문자음식주문목록ViewModel 목록 { get; }
    public 주문자음식주문상세ViewModel 상세 { get; }
}
