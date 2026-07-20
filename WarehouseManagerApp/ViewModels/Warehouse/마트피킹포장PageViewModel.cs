using Ssalddel.Ui.Common.Areas.App.ViewModels;
using WarehouseManagerApp.Services;

namespace WarehouseManagerApp.ViewModels.Warehouse;

/// <summary>마트 피킹 페이지의 기능 접근·인증·조회 순서만 조율합니다.</summary>
public sealed class 마트피킹포장PageViewModel : 창고PageViewModelBase
{
    private readonly WarehousePageAvailabilityService _페이지사용가능성;
    private bool _초기화됨;
    private bool _초기화중;
    private bool _기능사용가능;
    private long? _주문Id;
    private string _기능안내 = "마트 피킹 조회 기능 상태를 확인하고 있습니다.";
    private string? _페이지오류메시지;

    public 마트피킹포장PageViewModel(
        창고작업세션상태ViewModel 세션,
        창고로그인ViewModel 인증,
        WarehousePageAvailabilityService 페이지사용가능성,
        마트피킹작업PageViewModel 작업조회)
        : base(
            세션,
            창고PageCodes.마트피킹포장,
            "마트 피킹·포장",
            창고운영ProfileCodes.마트도심)
    {
        _페이지사용가능성 = 페이지사용가능성;
        this.인증 = 구성요소등록(인증);
        this.작업조회 = 구성요소등록(작업조회);
    }

    public 창고로그인ViewModel 인증 { get; }
    public 마트피킹작업PageViewModel 작업조회 { get; }

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

    public long? 주문Id
    {
        get => _주문Id;
        private set => SetProperty(ref _주문Id, value);
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

    public bool 처리중 => 초기화중 || 인증.처리중 || 작업조회.처리중;

    public async Task<bool> 초기화Async(
        long? orderId,
        CancellationToken cancellationToken = default)
    {
        주문Id = orderId is > 0 ? orderId : null;
        초기화됨 = false;
        초기화중 = true;
        페이지오류메시지 = null;
        try
        {
            var availability = await _페이지사용가능성.GetMartPickingAsync(cancellationToken);
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
            페이지오류메시지 = "마트 피킹 기능 상태 확인 시간이 초과되었습니다.";
        }
        catch (HttpRequestException)
        {
            기능사용가능 = false;
            페이지오류메시지 = "서버에서 마트 피킹 기능 상태를 확인하지 못했습니다.";
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            기능사용가능 = false;
            페이지오류메시지 = "마트 피킹 기능 상태 응답을 처리하지 못했습니다.";
        }
        finally
        {
            초기화중 = false;
        }

        if (!기능사용가능 || !인증.창고업무접근가능)
        {
            초기화됨 = true;
            return false;
        }

        return await 인증후조회Async(cancellationToken);
    }

    public async Task<bool> 인증후조회Async(CancellationToken cancellationToken = default)
    {
        if (!기능사용가능 || !인증.창고업무접근가능 || 초기화중)
        {
            초기화됨 = true;
            return false;
        }

        초기화중 = true;
        try
        {
            return await 작업조회.초기화Async(주문Id, cancellationToken);
        }
        finally
        {
            초기화중 = false;
            초기화됨 = true;
        }
    }

    public Task<bool> 다시조회Async(CancellationToken cancellationToken = default)
        => 인증후조회Async(cancellationToken);

    public void 인증해제적용()
    {
        작업조회.결과초기화();
        초기화됨 = true;
    }
}
