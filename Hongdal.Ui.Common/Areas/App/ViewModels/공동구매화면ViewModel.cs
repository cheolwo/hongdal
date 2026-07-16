namespace Hongdal.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공동구매 페이지가 하나만 주입받아도 모집, 합의, 공급, 물류와 실행 하위 기능을 조립할 수 있는 루트 ViewModel입니다.
/// 하위 ViewModel은 각각 DI 등록되어 필요한 세부 페이지에서 따로 주입할 수도 있습니다.
/// </summary>
public sealed class 공동구매화면ViewModel : 조립ViewModelBase
{
    public 공동구매화면ViewModel(
        공동구매화면상태ViewModel 상태,
        공동구매모집기능ViewModel 모집,
        공동구매합의기능ViewModel 합의,
        공동구매공급기능ViewModel 공급,
        공동구매물류기능ViewModel 물류,
        공동구매실행기능ViewModel 실행)
    {
        this.상태 = 하위ViewModel등록(상태);
        this.모집 = 하위ViewModel등록(모집);
        this.합의 = 하위ViewModel등록(합의);
        this.공급 = 하위ViewModel등록(공급);
        this.물류 = 하위ViewModel등록(물류);
        this.실행 = 하위ViewModel등록(실행);
    }

    public 공동구매화면상태ViewModel 상태 { get; }
    public 공동구매모집기능ViewModel 모집 { get; }
    public 공동구매합의기능ViewModel 합의 { get; }
    public 공동구매공급기능ViewModel 공급 { get; }
    public 공동구매물류기능ViewModel 물류 { get; }
    public 공동구매실행기능ViewModel 실행 { get; }

    public bool 처리중 => 모집.처리중 || 합의.처리중 || 공급.처리중 || 물류.처리중 || 실행.처리중;

    public Task<bool> 초기화Async(
        string? communityScope = null,
        CancellationToken cancellationToken = default)
        => 모집.목록.목록조회Async(communityScope, cancellationToken);
}
