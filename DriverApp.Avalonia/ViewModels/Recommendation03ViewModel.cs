using DriverApp.Avalonia.Services;

namespace DriverApp.Avalonia.ViewModels;

public sealed class Recommendation03ViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store;
    public Recommendation03ViewModel(InMemoryDriverStore store) => _store = store;
}
