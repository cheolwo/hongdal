using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Hongdal.Ui.Common.Areas.App.Components;

/// <summary>
/// Razor Component에 ViewModel을 주입하고 속성 변경을 자동으로 다시 그리는 공통 기반 클래스입니다.
/// </summary>
public abstract class MvvmComponentBase<TViewModel> : OwningComponentBase<TViewModel>
    where TViewModel : class, INotifyPropertyChanged
{
    private bool _subscribed;

    protected TViewModel ViewModel => Service;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += ViewModel속성변경;
        _subscribed = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            구독해제();
        }

        base.Dispose(disposing);
    }

    protected override ValueTask DisposeAsyncCore()
    {
        구독해제();
        return base.DisposeAsyncCore();
    }

    private void 구독해제()
    {
        if (!_subscribed)
        {
            return;
        }

        ViewModel.PropertyChanged -= ViewModel속성변경;
        _subscribed = false;
    }

    private void ViewModel속성변경(object? sender, PropertyChangedEventArgs e)
        => _ = InvokeAsync(StateHasChanged);
}
