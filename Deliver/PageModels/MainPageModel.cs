using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deliver.Services;

namespace Deliver.PageModels;

public sealed partial class MainPageModel : ObservableObject
{
    private readonly DeliveryDriverAppProfile _profile;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _today = DateTime.Now.ToString("M월 d일 dddd");

    [ObservableProperty]
    private string _statusMessage = "점심 피크 전 대기 중입니다.";

    [ObservableProperty]
    private string _currentArea = "서울 강서구 화곡동";

    [ObservableProperty]
    private int _pendingDeliveryTickets = 8;

    [ObservableProperty]
    private int _recommendedTickets = 3;

    [ObservableProperty]
    private decimal _todayExpectedPayout = 48600m;

    [ObservableProperty]
    private DeliveryTicketPreview? _selectedTicket;

    public string AppName => _profile.DisplayName;

    public string DriverRole => _profile.DriverRole;

    public IReadOnlyList<DeliveryTicketPreview> RecommendedTicketItems { get; } =
    [
        new("FD-10021", "김치찌개 2개", "맛있는집 화곡점", "화곡동 112-7", "1.2km", 4200m, "픽업 6분 내"),
        new("FD-10022", "아메리카노 3잔", "카페모아 우장산", "우장산역 3번 출구", "1.8km", 4700m, "묶음 가능"),
        new("FD-10023", "치킨 반반", "홍달치킨 등촌", "등촌동 24-3", "2.4km", 5600m, "단건 추천")
    ];

    public MainPageModel(DeliveryDriverAppProfile profile)
    {
        _profile = profile;
        SelectedTicket = RecommendedTicketItems.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        await Task.Delay(150);
        StatusMessage = "배달권 추천을 새로 확인했습니다.";
        IsRefreshing = false;
    }

    [RelayCommand]
    private void SelectTicket(DeliveryTicketPreview ticket)
    {
        SelectedTicket = ticket;
        StatusMessage = $"{ticket.TicketId} 배달권을 선택했습니다.";
    }

    [RelayCommand]
    private void AcceptSelectedTicket()
    {
        if (SelectedTicket is null)
        {
            StatusMessage = "선택된 배달권이 없습니다.";
            return;
        }

        StatusMessage = $"{SelectedTicket.TicketId} 배달권을 수락했습니다.";
    }
}

public sealed record DeliveryTicketPreview(
    string TicketId,
    string OrderSummary,
    string RestaurantName,
    string DropoffAddress,
    string DistanceText,
    decimal DriverPayout,
    string RecommendationReason)
{
    public string DriverPayoutText => $"{DriverPayout:N0}원";
}
