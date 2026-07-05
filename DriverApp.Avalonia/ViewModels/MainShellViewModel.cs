using DriverApp.Avalonia.Services;
using DriverApp.Avalonia.Views;
using System.Collections.Generic;

namespace DriverApp.Avalonia.ViewModels;

public sealed class MainShellViewModel : ViewModelBase
{
    private readonly InMemoryDriverStore _store = new();
    private readonly InMemoryAuthSessionStore _sessionStore = new();
    private object _currentViewModel;
    private ShellNavigationItem? _selectedNavItem;

    public MainShellViewModel()
    {
        NavigationItems = new List<ShellNavigationItem>
        {
            new("로그인", "Login"),
            new("대시보드", "Dashboard"),
            new("01 프로필", "Profile01"),
            new("02 근무", "Work02"),
            new("03 추천", "Recommendation03"),
            new("04 배차", "Dispatch04")
        };

        Header = new ShellHeaderViewModel();
        LoginViewModel = new LoginViewModel(_store, _sessionStore);
        DashboardViewModel = new DashboardViewModel(_store, _sessionStore, this);
        Profile01ViewModel = new Profile01ViewModel(_store);
        Work02ViewModel = new Work02ViewModel(_store);
        Recommendation03ViewModel = new Recommendation03ViewModel(_store);
        Dispatch04ViewModel = new Dispatch04ViewModel(_store);
        _currentViewModel = new LoginView { DataContext = LoginViewModel };
        _selectedNavItem = NavigationItems[0];
        UpdateHeader("Login");
    }

    public ShellHeaderViewModel Header { get; }

    public IReadOnlyList<ShellNavigationItem> NavigationItems { get; }

    public LoginViewModel LoginViewModel { get; }
    public DashboardViewModel DashboardViewModel { get; }
    public Profile01ViewModel Profile01ViewModel { get; }
    public Work02ViewModel Work02ViewModel { get; }
    public Recommendation03ViewModel Recommendation03ViewModel { get; }
    public Dispatch04ViewModel Dispatch04ViewModel { get; }

    public object CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public ShellNavigationItem? SelectedNavItem
    {
        get => _selectedNavItem;
        set
        {
            if (SetProperty(ref _selectedNavItem, value) && value is not null)
            {
                Navigate(value.Tag);
            }
        }
    }

    public void Navigate(string tag)
    {
        CurrentViewModel = tag switch
        {
            "Login" => new LoginView { DataContext = LoginViewModel },
            "Dashboard" => new DashboardView { DataContext = DashboardViewModel },
            "Profile01" => new Profile01View { DataContext = Profile01ViewModel },
            "Work02" => new Work02View { DataContext = Work02ViewModel },
            "Recommendation03" => new Recommendation03View { DataContext = Recommendation03ViewModel },
            "Dispatch04" => new Dispatch04View { DataContext = Dispatch04ViewModel },
            _ => new LoginView { DataContext = LoginViewModel }
        };

        UpdateHeader(tag);
    }

    private void UpdateHeader(string tag)
    {
        Header.Title = tag switch
        {
            "Dashboard" => "DriverApp Dashboard",
            "Profile01" => "01 프로필",
            "Work02" => "02 근무",
            "Recommendation03" => "03 추천",
            "Dispatch04" => "04 배차",
            _ => "DriverApp"
        };

        Header.Subtitle = tag switch
        {
            "Dashboard" => "로그인 후 기사 업무를 확인합니다.",
            "Profile01" => "기사 프로필과 등록 정보를 확인합니다.",
            "Work02" => "근무 상태와 예약 흐름을 관리합니다.",
            "Recommendation03" => "추천 의뢰와 콜 범위를 확인합니다.",
            "Dispatch04" => "배차 계획 신청/조회 화면입니다.",
            _ => "메모리 기반 기사 업무 앱"
        };

        Header.UserLabel = LoginViewModel.LoggedInSession is null
            ? "미로그인"
            : $"{LoginViewModel.LoggedInSession.DriverName} 님";
    }
}
