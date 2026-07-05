using DriverApp.Avalonia.ViewModels;

namespace DriverApp.Avalonia.Services;

public sealed class NavigationService : INavigationService
{
    private readonly MainShellViewModel _shell;

    public NavigationService(MainShellViewModel shell)
    {
        _shell = shell;
    }

    public void Navigate(string section)
    {
        _shell.Navigate(section);
    }
}
