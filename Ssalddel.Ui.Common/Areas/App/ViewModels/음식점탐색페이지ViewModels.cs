using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 음식배달페이지접근상태
{
    확인전,
    사용가능,
    기능비활성,
    오류
}

/// <summary>음식점 탐색 페이지의 서버 기능 플래그만 판정합니다.</summary>
public sealed partial class 음식배달페이지접근ViewModel(
    I음식배달페이지접근Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(사용가능))]
    [NotifyPropertyChangedFor(nameof(기능비활성))]
    public partial 음식배달페이지접근상태 화면상태 { get; private set; }

    public bool 사용가능 => 화면상태 == 음식배달페이지접근상태.사용가능;
    public bool 기능비활성 => 화면상태 == 음식배달페이지접근상태.기능비활성;

    public async Task<bool> 확인Async(CancellationToken cancellationToken = default)
    {
        화면상태 = 음식배달페이지접근상태.확인전;
        var succeeded = await 작업실행Async(
            async token =>
            {
                화면상태 = await service.기능활성여부Async(token)
                    ? 음식배달페이지접근상태.사용가능
                    : 음식배달페이지접근상태.기능비활성;
            },
            "음식 배달 기능 상태를 확인했습니다.",
            cancellationToken,
            ex => $"음식 배달 기능 상태를 확인하지 못했습니다. {ex.Message}");
        if (!succeeded)
        {
            화면상태 = 취소됨
                ? 음식배달페이지접근상태.확인전
                : 음식배달페이지접근상태.오류;
        }

        return succeeded;
    }
}

/// <summary>서버 탐색 정책, 공개 권역과 사용자가 명시한 조회 기준만 관리합니다.</summary>
public sealed partial class 음식점탐색기준ViewModel(
    I음식점탐색정책읽기Service policyService,
    I음식점공개읽기Service restaurantService) : 업무작업ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(조회가능))]
    public partial RestaurantSearchPolicyDto? 정책 { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(조회가능))]
    public partial IReadOnlyList<음식점탐색권역응답> 권역목록 { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(선택권역))]
    [NotifyPropertyChangedFor(nameof(조회가능))]
    public partial string? 선택배달권키 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(조회가능))]
    public partial double 반경Km { get; set; } = 7d;

    [ObservableProperty]
    public partial bool 초기화됨 { get; private set; }

    public 음식점탐색권역응답? 선택권역 => 권역목록.FirstOrDefault(item =>
        string.Equals(item.배달권키, 선택배달권키, StringComparison.Ordinal));

    public bool 조회가능 => 초기화됨
                           && 정책 is not null
                           && 선택권역 is not null
                           && 반경Km >= 정책.MinRadiusKm
                           && 반경Km <= 정책.MaxRadiusKm;

    public Task<bool> 준비Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var policyTask = policyService.조회Async(token);
                var scopeTask = restaurantService.권역목록Async(token);
                await Task.WhenAll(policyTask, scopeTask);
                정책 = await policyTask;
                권역목록 = await scopeTask;
                반경Km = 정책.DefaultRadiusKm;
                초기화됨 = true;
            },
            "음식점 탐색 기준을 불러왔습니다.",
            cancellationToken,
            ex => $"음식점 탐색 기준을 불러오지 못했습니다. {ex.Message}");

    public void 빠른반경설정(double radiusKm)
    {
        if (정책 is null)
        {
            return;
        }

        반경Km = Math.Clamp(radiusKm, 정책.MinRadiusKm, 정책.MaxRadiusKm);
    }
}

/// <summary>사용자가 선택한 권역·반경·검색 조건의 서버 목록과 페이징만 담당합니다.</summary>
public sealed partial class 음식점공개목록ViewModel(
    I음식점공개읽기Service service) : 업무작업ViewModelBase
{
    private string? _배달권키;
    private decimal _반경Km;

    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool 주문가능만 { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(음식점목록))]
    [NotifyPropertyChangedFor(nameof(전체건수))]
    [NotifyPropertyChangedFor(nameof(현재페이지))]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial 음식점공개목록응답 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<음식점공개요약응답> 음식점목록 => 응답.Items;
    public int 전체건수 => 응답.TotalCount;
    public int 현재페이지 => 응답.Page;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 응답.PageSize)));
    public bool 결과없음 => 초기화됨 && 전체건수 == 0;
    public bool 검색조건사용중 => !string.IsNullOrWhiteSpace(검색어) || 주문가능만;

    public Task<bool> 조회Async(
        string? deliveryScopeKey,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deliveryScopeKey))
        {
            return Task.FromResult(유효성실패("조회할 공개 행정권역을 선택해 주세요."));
        }

        _배달권키 = deliveryScopeKey.Trim();
        _반경Km = (decimal)radiusKm;
        return 조회CoreAsync(1, cancellationToken);
    }

    public Task<bool> 페이지조회Async(int page, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_배달권키))
        {
            return Task.FromResult(유효성실패("먼저 공개 행정권역을 선택해 조회해 주세요."));
        }

        return 조회CoreAsync(Math.Max(1, page), cancellationToken);
    }

    public Task<bool> 새로고침Async(CancellationToken cancellationToken = default)
        => 페이지조회Async(Math.Max(1, 현재페이지), cancellationToken);

    private Task<bool> 조회CoreAsync(int page, CancellationToken cancellationToken)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.목록Async(new 음식점공개목록조회요청
                {
                    배달권키 = _배달권키!,
                    반경Km = _반경Km,
                    검색어 = 검색어,
                    주문가능만 = 주문가능만,
                    Page = page,
                    PageSize = 12
                }, token);
                초기화됨 = true;
            },
            "공개 음식점 목록을 불러왔습니다.",
            cancellationToken,
            ex => $"공개 음식점 목록을 불러오지 못했습니다. {ex.Message}");
}

/// <summary>주소나 목록에서 선택한 정확한 restaurantId 한 건과 공개 메뉴만 조회합니다.</summary>
public sealed partial class 음식점공개상세ViewModel(
    I음식점공개읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial long? 요청RestaurantId { get; private set; }

    [ObservableProperty]
    public partial 음식점공개상세응답? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(long restaurantId, CancellationToken cancellationToken = default)
    {
        if (restaurantId <= 0)
        {
            return Task.FromResult(유효성실패("조회할 음식점 ID를 확인해 주세요."));
        }

        요청RestaurantId = restaurantId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(restaurantId, token);
                찾을수없음 = 상세 is null;
            },
            "음식점과 공개 메뉴를 불러왔습니다.",
            cancellationToken,
            ex => $"음식점 상세를 불러오지 못했습니다. {ex.Message}");
    }

    public void 선택해제()
    {
        요청RestaurantId = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }
}

/// <summary>접근·탐색 기준·목록·정확한 상세 ViewModel을 조립합니다.</summary>
public sealed class 음식점탐색PageViewModel : 조립ViewModelBase
{
    public 음식점탐색PageViewModel(
        음식배달페이지접근ViewModel access,
        음식점탐색기준ViewModel criteria,
        음식점공개목록ViewModel list,
        음식점공개상세ViewModel detail)
    {
        접근 = 하위ViewModel등록(access);
        기준 = 하위ViewModel등록(criteria);
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 음식배달페이지접근ViewModel 접근 { get; }
    public 음식점탐색기준ViewModel 기준 { get; }
    public 음식점공개목록ViewModel 목록 { get; }
    public 음식점공개상세ViewModel 상세 { get; }
}
