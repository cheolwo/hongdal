using Avalonia.Controls;
using Avalonia.Interactivity;
using DriverApp.Avalonia.ViewModels;

namespace DriverApp.Avalonia.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContext = new LoginViewModel(new Services.InMemoryDriverStore(), new Services.InMemoryAuthSessionStore());
    }

    private void OnLoginClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            if (vm.Login())
            {
                if (TopLevel.GetTopLevel(this) is Window window && window.DataContext is MainShellViewModel shell)
                {
                    shell.Navigate("Dashboard");
                }
            }
        }
    }

    private void GoDashboard(object? sender, RoutedEventArgs e) => NavigateShell("Dashboard");
    private void GoProfile(object? sender, RoutedEventArgs e) => NavigateShell("Profile01");
    private void GoWork(object? sender, RoutedEventArgs e) => NavigateShell("Work02");
    private void GoRecommendation(object? sender, RoutedEventArgs e) => NavigateShell("Recommendation03");
    private void GoDispatch(object? sender, RoutedEventArgs e) => NavigateShell("Dispatch04");

    private void NavigateShell(string section)
    {
        if (TopLevel.GetTopLevel(this) is Window window && window.DataContext is MainShellViewModel shell)
        {
            shell.Navigate(section);
        }
    }
}
