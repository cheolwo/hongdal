using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class Work02ViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    public Work02ViewModel(InMemoryDriverStore store) => _store = store;
}
