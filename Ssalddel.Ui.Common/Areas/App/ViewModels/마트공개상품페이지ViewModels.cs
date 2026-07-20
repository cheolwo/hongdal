using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 마트페이지접근상태
{
    확인전,
    사용가능,
    기능비활성,
    오류
}

/// <summary>마트 공개 상품 페이지의 서버 기능 플래그만 판정합니다.</summary>
public sealed partial class 마트페이지접근ViewModel(
    I마트페이지접근Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(사용가능))]
    [NotifyPropertyChangedFor(nameof(기능비활성))]
    public partial 마트페이지접근상태 화면상태 { get; private set; }

    public bool 사용가능 => 화면상태 == 마트페이지접근상태.사용가능;
    public bool 기능비활성 => 화면상태 == 마트페이지접근상태.기능비활성;

    public async Task<bool> 확인Async(CancellationToken cancellationToken = default)
    {
        화면상태 = 마트페이지접근상태.확인전;
        var succeeded = await 작업실행Async(
            async token =>
            {
                화면상태 = await service.기능활성여부Async(token)
                    ? 마트페이지접근상태.사용가능
                    : 마트페이지접근상태.기능비활성;
            },
            "알뜰살뜰 마트 기능 상태를 확인했습니다.",
            cancellationToken,
            ex => $"알뜰살뜰 마트 기능 상태를 확인하지 못했습니다. {ex.Message}");
        if (!succeeded)
        {
            화면상태 = 취소됨 ? 마트페이지접근상태.확인전 : 마트페이지접근상태.오류;
        }

        return succeeded;
    }
}

/// <summary>공개 상품 검색 조건, 서버 페이징과 목록 결과만 관리합니다.</summary>
public sealed partial class 마트공개상품목록ViewModel(
    I마트공개상품읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool 판매가능만 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(상품목록))]
    [NotifyPropertyChangedFor(nameof(전체건수))]
    [NotifyPropertyChangedFor(nameof(현재페이지))]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial 마트공개상품목록응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<마트공개상품요약응답> 상품목록 => 응답.Items;
    public int 전체건수 => 응답.TotalCount;
    public int 현재페이지 => 응답.Page;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 응답.PageSize)));
    public bool 결과없음 => 초기화됨 && 전체건수 == 0;
    public bool 검색조건사용중 => !string.IsNullOrWhiteSpace(검색어) || 판매가능만;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(1, cancellationToken);

    public Task<bool> 페이지조회Async(int page, CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, page), cancellationToken);

    public Task<bool> 새로고침Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, 현재페이지), cancellationToken);

    private Task<bool> 조회CoreAsync(int page, CancellationToken cancellationToken)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.목록Async(new 마트공개상품목록조회요청
                {
                    검색어 = 검색어,
                    판매가능만 = 판매가능만,
                    Page = page,
                    PageSize = 12
                }, token);
                초기화됨 = true;
            },
            "마트 공개 상품 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"마트 공개 상품 목록을 불러오지 못했습니다. {ex.Message}");
}

/// <summary>주소나 목록에서 선택한 정확한 productId 한 건만 조회합니다.</summary>
public sealed partial class 마트공개상품상세ViewModel(
    I마트공개상품읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 요청ProductId { get; private set; }

    [ObservableProperty]
    public partial 마트공개상품상세응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(long productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return Task.FromResult(유효성실패("조회할 마트 상품 ID를 확인해 주세요."));
        }

        요청ProductId = productId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(productId, token);
                찾을수없음 = 상세 is null;
            },
            "마트 공개 상품 상세를 불러왔습니다.",
            cancellationToken,
            ex => $"마트 공개 상품 상세를 불러오지 못했습니다. {ex.Message}");
    }

    public void 선택해제()
    {
        요청ProductId = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>기능 접근, 공개 목록과 정확한 상세 ViewModel을 조립합니다.</summary>
public sealed class 마트공개상품PageViewModel : 조립ViewModelBase
{
    public 마트공개상품PageViewModel(
        마트페이지접근ViewModel access,
        마트공개상품목록ViewModel list,
        마트공개상품상세ViewModel detail)
    {
        접근 = 하위ViewModel등록(access);
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 마트페이지접근ViewModel 접근 { get; }
    public 마트공개상품목록ViewModel 목록 { get; }
    public 마트공개상품상세ViewModel 상세 { get; }
}
