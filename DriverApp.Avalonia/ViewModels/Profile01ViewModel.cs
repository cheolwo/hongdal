using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class Profile01ViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    public Profile01ViewModel(InMemoryDriverStore store) => _store = store;
}
