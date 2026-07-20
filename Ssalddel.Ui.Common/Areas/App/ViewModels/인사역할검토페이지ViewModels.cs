using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>HR 역할 배정 원장의 검색 조건, 서버 페이징과 목록 결과만 관리합니다.</summary>
public sealed partial class 인사역할검토목록ViewModel(
    I인사역할검토읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial string 검색어 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 원장유형 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 상태코드 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 참여자유형 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string 범위유형 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(검토목록))]
    [NotifyPropertyChangedFor(nameof(전체건수))]
    [NotifyPropertyChangedFor(nameof(현재페이지))]
    [NotifyPropertyChangedFor(nameof(총페이지수))]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial HrRoleReviewListResponse 응답 { get; private set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(결과없음))]
    public partial bool 초기화됨 { get; private set; }

    public IReadOnlyList<HrRoleReviewSummaryResponse> 검토목록 => 응답.Items;
    public int 전체건수 => 응답.TotalCount;
    public int 현재페이지 => 응답.Page;
    public int 총페이지수 => Math.Max(1, (int)Math.Ceiling(전체건수 / (double)Math.Max(1, 응답.PageSize)));
    public bool 결과없음 => 초기화됨 && 전체건수 == 0;
    public bool 검색조건사용중
        => !string.IsNullOrWhiteSpace(검색어)
           || !string.IsNullOrWhiteSpace(원장유형)
           || !string.IsNullOrWhiteSpace(상태코드)
           || !string.IsNullOrWhiteSpace(참여자유형)
           || !string.IsNullOrWhiteSpace(범위유형);

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(1, cancellationToken);

    public Task<bool> 페이지조회Async(int page, CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, page), cancellationToken);

    public Task<bool> 새로고침Async(CancellationToken cancellationToken = default)
        => 조회CoreAsync(Math.Max(1, 현재페이지), cancellationToken);

    public Task<bool> 조건초기화후조회Async(CancellationToken cancellationToken = default)
    {
        검색어 = string.Empty;
        원장유형 = string.Empty;
        상태코드 = string.Empty;
        참여자유형 = string.Empty;
        범위유형 = string.Empty;
        return 조회Async(cancellationToken);
    }

    public void 결과초기화()
    {
        응답 = new HrRoleReviewListResponse();
        초기화됨 = false;
        작업상태초기화();
    }

    private Task<bool> 조회CoreAsync(int page, CancellationToken cancellationToken)
        => 작업실행Async(
            async token =>
            {
                응답 = await service.목록Async(new HrRoleReviewListRequest
                {
                    Search = 검색어,
                    SourceCode = 원장유형,
                    StatusCode = 상태코드,
                    ParticipantCategory = 참여자유형,
                    ScopeType = 범위유형,
                    Page = page,
                    PageSize = 15
                }, token);
                초기화됨 = true;
            },
            "HR 역할 검토 목록을 불러왔습니다.",
            cancellationToken,
            오류문구);

    private static string 오류문구(Exception exception)
        => exception is SsalddelApiException { StatusCode: 401 }
            ? "로그인 세션이 만료되었습니다. 다시 로그인해 주세요."
            : exception is SsalddelApiException { StatusCode: 403 }
                ? "이 계정에는 HR 역할 검토 권한이 없습니다."
                : $"HR 역할 검토 목록을 불러오지 못했습니다. {exception.Message}";
}

/// <summary>주소나 목록에서 선택한 정확한 reviewId 한 건만 조회합니다.</summary>
public sealed partial class 인사역할검토상세ViewModel(
    I인사역할검토읽기Service service) : 업무작업ViewModelBase
{
    [ObservableProperty]
    public partial Guid? 요청ReviewId { get; private set; }

    [ObservableProperty]
    public partial HrRoleReviewDetailResponse? 상세 { get; private set; }

    [ObservableProperty]
    public partial bool 찾을수없음 { get; private set; }

    public Task<bool> 조회Async(Guid reviewId, CancellationToken cancellationToken = default)
    {
        if (reviewId == Guid.Empty)
        {
            return Task.FromResult(유효성실패("조회할 HR 역할 검토 ID를 확인해 주세요."));
        }

        요청ReviewId = reviewId;
        상세 = null;
        찾을수없음 = false;
        return 작업실행Async(
            async token =>
            {
                상세 = await service.상세Async(reviewId, token);
                찾을수없음 = 상세 is null;
            },
            "HR 역할 검토 상세를 불러왔습니다.",
            cancellationToken,
            오류문구);
    }

    public void 선택해제()
    {
        요청ReviewId = null;
        상세 = null;
        찾을수없음 = false;
        작업상태초기화();
    }

    private static string 오류문구(Exception exception)
        => exception is SsalddelApiException { StatusCode: 401 }
            ? "로그인 세션이 만료되었습니다. 다시 로그인해 주세요."
            : exception is SsalddelApiException { StatusCode: 403 }
                ? "이 계정에는 HR 역할 검토 상세 권한이 없습니다."
                : $"HR 역할 검토 상세를 불러오지 못했습니다. {exception.Message}";
}

/// <summary>HR 역할 검토 목록과 정확한 지원·배정 원장 상세 조회만 조립합니다.</summary>
public sealed class 인사역할검토PageViewModel : 조립ViewModelBase
{
    public 인사역할검토PageViewModel(
        인사역할검토목록ViewModel list,
        인사역할검토상세ViewModel detail)
    {
        목록 = 하위ViewModel등록(list);
        상세 = 하위ViewModel등록(detail);
    }

    public 인사역할검토목록ViewModel 목록 { get; }
    public 인사역할검토상세ViewModel 상세 { get; }
    public bool 처리중 => 목록.처리중 || 상세.처리중;

    public async Task<bool> 초기화Async(
        Guid? reviewId,
        CancellationToken cancellationToken = default)
    {
        if (!await 목록.조회Async(cancellationToken))
        {
            return false;
        }

        if (reviewId is { } id && id != Guid.Empty)
        {
            return await 상세.조회Async(id, cancellationToken);
        }

        상세.선택해제();
        return true;
    }

    public void 결과초기화()
    {
        목록.결과초기화();
        상세.선택해제();
    }
}
