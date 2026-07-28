using WarehouseManagerApp.Services;

namespace WarehouseManagerApp.ViewModels.Warehouse;

/// <summary>출고 운송의뢰 페이지의 기능 노출과 창고 업무 인증만 관리합니다.</summary>
public sealed class 창고운송의뢰초안PageViewModel : 창고PageViewModelBase
{
    private readonly WarehousePageAvailabilityService _availability;
    private bool _초기화됨;
    private bool _초기화중;
    private bool _기능사용가능;
    private string _기능안내 = "출고 운송의뢰 기능 상태를 확인하고 있습니다.";
    private string? _페이지오류메시지;

    public 창고운송의뢰초안PageViewModel(창고작업세션상태ViewModel 세션, 창고로그인ViewModel 인증, WarehousePageAvailabilityService availability)
        : base(세션, 창고PageCodes.일반운송의뢰초안, "출고 운송의뢰", 창고운영ProfileCodes.일반입출고)
    {
        _availability = availability;
        this.인증 = 구성요소등록(인증);
    }

    public 창고로그인ViewModel 인증 { get; }
    public bool 초기화됨 { get => _초기화됨; private set => SetProperty(ref _초기화됨, value); }
    public bool 초기화중 { get => _초기화중; private set => SetProperty(ref _초기화중, value); }
    public bool 기능사용가능 { get => _기능사용가능; private set => SetProperty(ref _기능사용가능, value); }
    public string 기능안내 { get => _기능안내; private set => SetProperty(ref _기능안내, value); }
    public string? 페이지오류메시지 { get => _페이지오류메시지; private set => SetProperty(ref _페이지오류메시지, value); }

    public async Task<bool> 초기화Async(CancellationToken cancellationToken = default)
    {
        초기화됨=false;초기화중=true;페이지오류메시지=null;
        try{var available=await _availability.GetTransportRequestDraftAsync(cancellationToken);기능사용가능=available.IsEnabled;기능안내=available.Notice;if(기능사용가능)await 인증.초기화Async(cancellationToken);}
        catch(OperationCanceledException) when(!cancellationToken.IsCancellationRequested){기능사용가능=false;페이지오류메시지="기능 상태 확인 시간이 초과되었습니다.";}
        catch(HttpRequestException){기능사용가능=false;페이지오류메시지="서버에서 창고 기능 상태를 확인하지 못했습니다.";}
        catch(Exception) when(!cancellationToken.IsCancellationRequested){기능사용가능=false;페이지오류메시지="출고 운송의뢰 기능 응답을 처리하지 못했습니다.";}
        finally{초기화중=false;초기화됨=true;}
        return 기능사용가능&&인증.창고업무접근가능;
    }

    public bool 인증후준비(){초기화됨=true;return 기능사용가능&&인증.창고업무접근가능;}
    public void 인증해제적용()=>초기화됨=true;
}
