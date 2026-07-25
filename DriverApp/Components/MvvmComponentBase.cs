namespace DriverApp.Components;

/// <summary>
/// 기존 기사 앱 namespace를 유지하면서 공통 MVVM component 수명 구현을 사용합니다.
/// </summary>
public abstract class MvvmComponentBase<TViewModel>
    : Ssalddel.Ui.Common.Areas.App.Components.MvvmComponentBase<TViewModel>
    where TViewModel : class, System.ComponentModel.INotifyPropertyChanged
{
}
