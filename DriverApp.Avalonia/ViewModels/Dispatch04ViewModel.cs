using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class Dispatch04ViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    public Dispatch04ViewModel(InMemoryDriverStore store) => _store = store;
}
