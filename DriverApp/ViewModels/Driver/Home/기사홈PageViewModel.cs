using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DriverApp.Models.Driver.Home;
using DriverApp.Services;
using DriverApp.ViewModels.Driver;
using DriverApp.ViewModels.Driver.Features;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace DriverApp.ViewModels.Driver.Home;

public sealed partial class 기사홈PageViewModel : 기사PageViewModelBase
{
    public 기사홈PageViewModel(기사프로필기능ViewModel 프로필기능)
    {
        this.프로필기능 = 하위ViewModel등록(프로필기능);
        화면 = 기사홈ViewModel.Empty();
    }

    public 기사프로필기능ViewModel 프로필기능 { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(주요행동라벨))]
    [NotifyPropertyChangedFor(nameof(주요행동경로))]
    public partial 기사홈ViewModel 화면 { get; private set; }

    public bool 불러오는중 => 처리중;
    public bool 오류발생 => 상태 == PageViewModel상태.실패;
    public bool 표시준비됨 => 초기화됨;

    public string 주요행동라벨 => 화면.주요행동문구;
    public string 주요행동경로 => Resolve주요행동경로(화면.주요행동코드);

    public Task<bool> InitializeAsync() => 초기화Async();

    [RelayCommand(AllowConcurrentExecutions = false)]
    public Task<bool> RefreshAsync()
        => 새로고침Async();

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        await 프로필기능.홈조회.실행Async(cancellationToken);
        if (프로필기능.홈조회.오류발생)
        {
            throw new InvalidOperationException(
                프로필기능.홈조회.오류메시지 ?? "기사 홈 정보를 불러오지 못했습니다.");
        }

        var response = 프로필기능.홈조회.결과
            ?? throw new InvalidOperationException("기사 홈 정보를 불러오지 못했습니다.");
        화면 = 기사홈ViewModel.From(response);
    }

    private static string Resolve주요행동경로(string actionCode) => actionCode switch
    {
        "VIEW_CURRENT_TRANSPORT" => DriverRoutes.CurrentTransport,
        "START_WORK" => DriverRoutes.WorkStart,
        "VIEW_RECOMMENDATIONS" => DriverRoutes.Recommendations,
        "VIEW_RESERVATION" => DriverRoutes.Reservations,
        "CHECK_NOTIFICATION" => DriverRoutes.NotificationSettings,
        "VIEW_SETTLEMENT" => DriverRoutes.CurrentMonthSettlement,
        _ => DriverRoutes.Recommendations
    };
}
