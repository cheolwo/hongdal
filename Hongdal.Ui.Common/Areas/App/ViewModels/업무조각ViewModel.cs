using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public enum 업무조각유형
{
    목록조회,
    상세조회,
    등록,
    수정,
    처리,
    삭제
}

/// <summary>
/// Razor Class Library의 작은 화면 조각이 공통으로 주입받을 수 있는 업무 계약입니다.
/// 표현 방식은 테이블·카드·폼 중 자유롭게 선택하고 ViewModel은 업무 상태만 제공합니다.
/// </summary>
public interface I업무조각ViewModel : INotifyPropertyChanged
{
    string 업무코드 { get; }
    string 업무명 { get; }
    업무조각유형 업무유형 { get; }
    Api작업상태 상태 { get; }
    bool 처리중 { get; }
    string? 오류메시지 { get; }
    현재사용자Snapshot 현재사용자 { get; }
    bool 사용자확인됨 { get; }
}

public interface I목록조회ViewModel<out TItem> : I업무조각ViewModel
{
    IReadOnlyList<TItem> 항목목록 { get; }
    Task<bool> 조회Async(CancellationToken cancellationToken = default);
}

public interface I상세조회ViewModel<out TItem> : I업무조각ViewModel
{
    TItem? 항목 { get; }
    Task<bool> 조회Async(CancellationToken cancellationToken = default);
}

public interface I명령ViewModel<out TDraft> : I업무조각ViewModel
{
    TDraft 초안 { get; }
    Task<bool> 실행Async(CancellationToken cancellationToken = default);
}

public abstract class 업무조각ViewModelBase(
    string 업무코드,
    string 업무명,
    업무조각유형 업무유형) : 업무작업ViewModelBase, I업무조각ViewModel
{
    public string 업무코드 { get; } = 업무코드;
    public string 업무명 { get; } = 업무명;
    public 업무조각유형 업무유형 { get; } = 업무유형;
}

/// <summary>
/// 기존 업무 ViewModel의 공개 기능 하나를 RCL 주입 단위로 노출하는 얇은 어댑터입니다.
/// 원본 ViewModel과 실행 상태를 공유하므로 기존 화면과 새 조각 컴포넌트를 함께 사용할 수 있습니다.
/// </summary>
public abstract class 위임업무조각ViewModelBase<TViewModel> : ObservableObject, I업무조각ViewModel, IDisposable
    where TViewModel : 업무작업ViewModelBase
{
    protected 위임업무조각ViewModelBase(
        TViewModel 원본,
        string 업무코드,
        string 업무명,
        업무조각유형 업무유형)
    {
        this.원본 = 원본;
        this.업무코드 = 업무코드;
        this.업무명 = 업무명;
        this.업무유형 = 업무유형;
        원본.PropertyChanged += 원본상태변경;
    }

    protected TViewModel 원본 { get; }
    public string 업무코드 { get; }
    public string 업무명 { get; }
    public 업무조각유형 업무유형 { get; }
    public Api작업상태 상태 => 원본.상태;
    public bool 처리중 => 원본.처리중;
    public string? 오류메시지 => 원본.오류메시지;
    public 현재사용자Snapshot 현재사용자 => 원본.현재사용자;
    public bool 사용자확인됨 => 원본.사용자확인됨;

    public void Dispose()
    {
        원본.PropertyChanged -= 원본상태변경;
        GC.SuppressFinalize(this);
    }

    private void 원본상태변경(object? sender, PropertyChangedEventArgs e)
        => OnPropertyChanged(string.Empty);
}
