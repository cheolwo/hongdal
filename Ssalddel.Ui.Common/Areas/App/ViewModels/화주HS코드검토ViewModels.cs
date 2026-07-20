using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 화주HS코드검토접근상태
{
    확인전,
    사용가능,
    로그인필요,
    기능비활성,
    오류
}

/// <summary>로그인과 서버 기능 플래그에 따른 화면 진입 상태만 담당합니다.</summary>
public sealed partial class 화주HS코드검토접근ViewModel : 업무작업ViewModelBase
{
    private readonly I화주HS코드검토접근Service _service;

    public 화주HS코드검토접근ViewModel(
        I화주HS코드검토접근Service service,
        ISsalddel현재사용자Context currentUserContext)
    {
        _service = service;
        현재사용자Context연결(currentUserContext);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(사용가능))]
    [NotifyPropertyChangedFor(nameof(로그인필요))]
    [NotifyPropertyChangedFor(nameof(기능비활성))]
    public partial 화주HS코드검토접근상태 화면상태 { get; private set; }

    public bool 사용가능 => 화면상태 == 화주HS코드검토접근상태.사용가능;
    public bool 로그인필요 => 화면상태 == 화주HS코드검토접근상태.로그인필요;
    public bool 기능비활성 => 화면상태 == 화주HS코드검토접근상태.기능비활성;

    public async Task<bool> 확인Async(CancellationToken cancellationToken = default)
    {
        화면상태 = 화주HS코드검토접근상태.확인전;
        var succeeded = await 작업실행Async(
            async token =>
            {
                var enabled = await _service.기능활성여부Async(token);
                화면상태 = !enabled
                    ? 화주HS코드검토접근상태.기능비활성
                    : 현재사용자.인증됨
                        ? 화주HS코드검토접근상태.사용가능
                        : 화주HS코드검토접근상태.로그인필요;
            },
            "HS 코드 검토 화면의 사용 가능 상태를 확인했습니다.",
            cancellationToken,
            ex => $"통관·무역 데이터 기능 상태를 확인하지 못했습니다. {ex.Message}");

        if (!succeeded)
        {
            화면상태 = 취소됨
                ? 화주HS코드검토접근상태.확인전
                : 화주HS코드검토접근상태.오류;
        }

        return succeeded;
    }
}

/// <summary>검색 조건, 페이지 이동과 HS 코드 검토 목록 응답만 담당합니다.</summary>
public sealed partial class 화주HS코드검토목록ViewModel(
    I화주HS코드검토Client client) : 업무작업ViewModelBase
{
    private const int DefaultPageSize = 30;

    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int? 업무분류 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial IReadOnlyList<화주HS코드검토항목응답> 검토목록 { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    public partial int 전체건수 { get; private set; }

    [ObservableProperty]
    public partial int 현재페이지 { get; private set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    public partial int 페이지크기 { get; private set; } = DefaultPageSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(비어있음))]
    public partial bool 초기화됨 { get; private set; }

    public bool 비어있음 => 초기화됨 && 검토목록.Count == 0;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 페이지크기)));

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 페이지조회Async(1, cancellationToken);

    public Task<bool> 페이지조회Async(
        int page,
        CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var response = await client.목록조회Async(
                    검색어,
                    업무분류,
                    Math.Max(1, page),
                    DefaultPageSize,
                    token);
                검토목록 = response.Items;
                전체건수 = response.TotalCount;
                현재페이지 = response.Page;
                페이지크기 = response.PageSize;
                초기화됨 = true;
            },
            "HS 코드 검토 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"HS 코드 검토 목록을 불러오지 못했습니다. 잠시 뒤 다시 시도해 주세요. {ex.Message}");
}

/// <summary>주소와 목록에서 선택한 정확한 reviewId 한 건의 상세 상태만 담당합니다.</summary>
public sealed partial class 화주HS코드검토상세ViewModel(
    I화주HS코드검토Client client) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 요청ReviewId { get; private set; }

    [ObservableProperty]
    public partial 화주HS코드검토상세응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(
        long reviewId,
        CancellationToken cancellationToken = default)
    {
        if (reviewId <= 0)
        {
            return Task.FromResult(유효성실패("조회할 HS 코드 검토 ID를 확인해 주세요."));
        }

        요청ReviewId = reviewId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await client.상세조회Async(reviewId, token);
                찾을수없음 = 상세 is null;
            },
            "HS 코드 검토 상세를 불러왔습니다.",
            cancellationToken,
            ex => $"HS 코드 검토 상세를 불러오지 못했습니다. 잠시 뒤 다시 시도해 주세요. {ex.Message}");
    }

    public void 선택해제()
    {
        요청ReviewId = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>서로 다른 책임의 세 ViewModel을 페이지 구성요소에 전달합니다.</summary>
public sealed class 화주HS코드검토PageViewModel : 조립ViewModelBase
{
    public 화주HS코드검토PageViewModel(
        화주HS코드검토접근ViewModel access,
        화주HS코드검토목록ViewModel list,
        화주HS코드검토상세ViewModel detail)
    {
        접근 = 하위ViewModel등록(access);
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 화주HS코드검토접근ViewModel 접근 { get; }
    public 화주HS코드검토목록ViewModel 목록 { get; }
    public 화주HS코드검토상세ViewModel 상세 { get; }
}
