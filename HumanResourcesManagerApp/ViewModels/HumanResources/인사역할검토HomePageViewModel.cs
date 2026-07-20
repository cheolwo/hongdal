using HumanResourcesManagerApp.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace HumanResourcesManagerApp.ViewModels.HumanResources;

/// <summary>인사 홈의 기능 접근·인증·역할 검토 조회 순서만 조율합니다.</summary>
public sealed class 인사역할검토HomePageViewModel : 조립ViewModelBase
{
    private readonly HumanResourcesPageAvailabilityService _pageAvailability;
    private bool _초기화됨;
    private bool _초기화중;
    private bool _기능사용가능;
    private Guid? _검토Id;
    private string _기능안내 = "HR 역할 검토 기능 상태를 확인하고 있습니다.";
    private string? _페이지오류메시지;

    public 인사역할검토HomePageViewModel(
        HumanResourcesPageAvailabilityService pageAvailability,
        인사로그인ViewModel authentication,
        인사역할검토PageViewModel roleReviews)
    {
        _pageAvailability = pageAvailability;
        인증 = 하위ViewModel등록(authentication);
        역할검토 = 하위ViewModel등록(roleReviews);
    }

    public 인사로그인ViewModel 인증 { get; }
    public 인사역할검토PageViewModel 역할검토 { get; }

    public bool 초기화됨
    {
        get => _초기화됨;
        private set => SetProperty(ref _초기화됨, value);
    }

    public bool 초기화중
    {
        get => _초기화중;
        private set => SetProperty(ref _초기화중, value);
    }

    public bool 기능사용가능
    {
        get => _기능사용가능;
        private set => SetProperty(ref _기능사용가능, value);
    }

    public Guid? 검토Id
    {
        get => _검토Id;
        private set => SetProperty(ref _검토Id, value);
    }

    public string 기능안내
    {
        get => _기능안내;
        private set => SetProperty(ref _기능안내, value);
    }

    public string? 페이지오류메시지
    {
        get => _페이지오류메시지;
        private set => SetProperty(ref _페이지오류메시지, value);
    }

    public bool 처리중 => 초기화중 || 인증.처리중 || 역할검토.처리중;

    public async Task<bool> 초기화Async(
        Guid? reviewId,
        CancellationToken cancellationToken = default)
    {
        검토Id = reviewId is { } id && id != Guid.Empty ? id : null;
        초기화됨 = false;
        초기화중 = true;
        페이지오류메시지 = null;
        try
        {
            var availability = await _pageAvailability.GetRoleReviewsAsync(cancellationToken);
            기능사용가능 = availability.IsEnabled;
            기능안내 = availability.Notice;
            if (기능사용가능)
            {
                await 인증.초기화Async(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            기능사용가능 = false;
            페이지오류메시지 = "HR 역할 검토 기능 상태 확인 시간이 초과되었습니다.";
        }
        catch (HttpRequestException)
        {
            기능사용가능 = false;
            페이지오류메시지 = "서버에서 HR 역할 검토 기능 상태를 확인하지 못했습니다.";
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            기능사용가능 = false;
            페이지오류메시지 = "HR 역할 검토 기능 상태 응답을 처리하지 못했습니다.";
        }
        finally
        {
            초기화중 = false;
        }

        if (!기능사용가능 || !인증.역할검토접근가능)
        {
            초기화됨 = true;
            return false;
        }

        return await 인증후조회Async(cancellationToken);
    }

    public async Task<bool> 인증후조회Async(CancellationToken cancellationToken = default)
    {
        if (!기능사용가능 || !인증.역할검토접근가능 || 초기화중)
        {
            초기화됨 = true;
            return false;
        }

        초기화중 = true;
        try
        {
            return await 역할검토.초기화Async(검토Id, cancellationToken);
        }
        finally
        {
            초기화중 = false;
            초기화됨 = true;
        }
    }

    public Task<bool> 다시조회Async(CancellationToken cancellationToken = default)
        => 인증후조회Async(cancellationToken);

    public async Task 로그아웃Async(CancellationToken cancellationToken = default)
    {
        await 인증.로그아웃Async(cancellationToken);
        역할검토.결과초기화();
        초기화됨 = true;
    }
}
