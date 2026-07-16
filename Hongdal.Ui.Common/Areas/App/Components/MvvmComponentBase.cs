using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Hongdal.Ui.Common.Areas.App.Components;

/// <summary>
/// Razor Component에 ViewModel을 주입하고 속성 변경을 자동으로 다시 그리는 공통 기반 클래스입니다.
/// </summary>
public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IDisposable
    where TViewModel : class, INotifyPropertyChanged
{
    [Inject]
    protected TViewModel ViewModel { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.PropertyChanged += ViewModel속성변경;
    }

    public virtual void Dispose()
    {
        ViewModel.PropertyChanged -= ViewModel속성변경;
        if (ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private void ViewModel속성변경(object? sender, PropertyChangedEventArgs e)
        => _ = InvokeAsync(StateHasChanged);
}
