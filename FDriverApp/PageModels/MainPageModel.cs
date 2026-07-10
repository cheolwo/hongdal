using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FDriverApp.Services;
using Hongdal.Contracts.Common.Drivers;
using System.Globalization;

namespace FDriverApp.PageModels;

public sealed partial class MainPageModel : ObservableObject
{
    private readonly FDriverAppProfile _profile;

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

    [ObservableProperty]
    private DriverWorkOfferDto? _activeWorkOffer;

    [ObservableProperty]
    private string _workStage = "추천 대기";

    [ObservableProperty]
    private string _nextActionGuide = "지도에서 추천 배달권을 선택하세요.";

    public string AppName => _profile.DisplayName;

    public string DriverRole => _profile.DriverRole;

    public IReadOnlyList<DeliveryTicketPreview> RecommendedTicketItems { get; } =
    [
        new(
            "FD-10021",
            "김치찌개 2개",
            "맛있는집 화곡점",
            "서울 강서구 화곡로 152",
            "화곡동 112-7",
            37.5432d,
            126.8398d,
            37.5485d,
            126.8421d,
            1.2d,
            4200m,
            "픽업 6분 내"),
        new(
            "FD-10022",
            "아메리카노 3잔",
            "카페모아 우장산",
            "서울 강서구 강서로 245",
            "우장산역 3번 출구",
            37.5482d,
            126.8356d,
            37.5489d,
            126.8378d,
            1.8d,
            4700m,
            "묶음 가능"),
        new(
            "FD-10023",
            "치킨 반반",
            "홍달치킨 등촌",
            "서울 강서구 공항대로 351",
            "등촌동 24-3",
            37.5587d,
            126.8505d,
            37.5558d,
            126.8580d,
            2.4d,
            5600m,
            "단건 추천")
    ];

    public IReadOnlyList<DriverMapMarkerItem> MapMarkers { get; }

    public IReadOnlyList<DriverMapRouteOverlay> SelectedRouteOverlays
    {
        get
        {
            var offer = CurrentRouteOffer;
            return offer is null
                ? []
                :
                [
                    new DriverMapRouteOverlay(
                        offer.OfferId,
                        $"{offer.Pickup.Label} -> {offer.Dropoff.Label}",
                        [
                            new DriverMapRoutePoint(
                                offer.Pickup.Latitude,
                                offer.Pickup.Longitude,
                                "음식점 픽업"),
                            new DriverMapRoutePoint(
                                offer.Dropoff.Latitude,
                                offer.Dropoff.Longitude,
                                "고객 전달")
                        ])
                ];
        }
    }

    public double MapCenterLatitude => CurrentRouteOffer?.Pickup.Latitude ?? 37.548d;

    public double MapCenterLongitude => CurrentRouteOffer?.Pickup.Longitude ?? 126.842d;

    public string PickupPointText => CurrentRouteOffer is null
        ? "음식점 픽업지 없음"
        : $"픽업: {CurrentRouteOffer.Pickup.Label} · {CurrentRouteOffer.Pickup.Address}";

    public string DropoffPointText => CurrentRouteOffer is null
        ? "고객 전달지 없음"
        : $"전달: {CurrentRouteOffer.Dropoff.Address}";

    public string SelectedRouteText => CurrentRouteOffer is null
        ? "선택된 배달권 없음"
        : $"{CurrentRouteOffer.Pickup.Label}에서 픽업 후 {CurrentRouteOffer.Dropoff.Address}로 전달";

    public string ActiveWorkSummary => ActiveWorkOffer is null
        ? "진행 중인 배달 없음"
        : $"{ActiveWorkOffer.Title} · {WorkStage}";

    public string ActiveWorkRouteText => ActiveWorkOffer is null
        ? "수락한 배달권이 없습니다."
        : $"{ActiveWorkOffer.Pickup.Label} → {ActiveWorkOffer.Dropoff.Label}";

    public string ActiveWorkPayoutText => ActiveWorkOffer is null
        ? "정산 예정 없음"
        : $"{ActiveWorkOffer.DriverPayout.ToString("N0", CultureInfo.CurrentCulture)}원";

    public bool HasActiveWork => ActiveWorkOffer is not null;

    public bool CanConfirmPickup => ActiveWorkOffer?.Status is DriverWorkOfferStatus.MovingToPickup or DriverWorkOfferStatus.Accepted;

    public bool CanCompleteDelivery => ActiveWorkOffer?.Status == DriverWorkOfferStatus.MovingToDropoff;

    public bool CanAcceptSelectedTicket => SelectedTicket is not null && ActiveWorkOffer is null;

    private DriverWorkOfferDto? CurrentRouteOffer => ActiveWorkOffer ?? SelectedTicket?.ToDriverWorkOffer(_profile);

    public MainPageModel(FDriverAppProfile profile)
    {
        _profile = profile;
        MapMarkers = RecommendedTicketItems.Select(ToMapMarker).ToArray();
        SelectedTicket = RecommendedTicketItems.FirstOrDefault();
    }

    [RelayCommand]
    private void OpenProfile()
    {
        StatusMessage = $"{DriverRole} 내 정보 메뉴로 연결할 예정입니다.";
    }

    [RelayCommand]
    private void OpenCurrentDelivery()
    {
        StatusMessage = ActiveWorkOffer is null
            ? "진행 중인 배달이 없습니다. 추천 배달권을 선택해 주세요."
            : $"{ActiveWorkOffer.OfferId} 현재 단계: {WorkStage}";
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
        if (ActiveWorkOffer is not null)
        {
            StatusMessage = "진행 중인 배달이 있어 현재 경로를 유지합니다.";
            return;
        }

        SelectedTicket = ticket;
        StatusMessage = $"{ticket.TicketId} 배달권을 선택했습니다.";
    }

    public void SelectTicketById(string ticketId)
    {
        var ticket = RecommendedTicketItems.FirstOrDefault(x => string.Equals(x.TicketId, ticketId, StringComparison.Ordinal));
        if (ticket is null)
        {
            return;
        }

        SelectTicket(ticket);
    }

    [RelayCommand]
    private void AcceptTicket(DeliveryTicketPreview ticket)
    {
        if (ActiveWorkOffer is not null)
        {
            StatusMessage = "이미 진행 중인 배달권이 있습니다.";
            return;
        }

        SelectedTicket = ticket;
        AcceptSelectedTicket();
    }

    [RelayCommand]
    private void AcceptSelectedTicket()
    {
        if (SelectedTicket is null)
        {
            StatusMessage = "선택된 배달권이 없습니다.";
            return;
        }

        if (ActiveWorkOffer is not null)
        {
            StatusMessage = "이미 진행 중인 배달권이 있습니다.";
            return;
        }

        ActiveWorkOffer = SelectedTicket.ToDriverWorkOffer(_profile) with
        {
            Status = DriverWorkOfferStatus.MovingToPickup
        };
        WorkStage = "음식점 이동 중";
        NextActionGuide = "음식점 도착 후 픽업 확인을 눌러 주세요.";
        StatusMessage = $"{SelectedTicket.TicketId} 배달권을 수락했습니다.";
    }

    [RelayCommand]
    private void ConfirmPickup()
    {
        if (ActiveWorkOffer is null)
        {
            StatusMessage = "진행 중인 배달권이 없습니다.";
            return;
        }

        ActiveWorkOffer = ActiveWorkOffer with
        {
            Status = DriverWorkOfferStatus.MovingToDropoff
        };
        WorkStage = "고객 주소 이동 중";
        NextActionGuide = "고객에게 전달한 뒤 전달 완료를 눌러 주세요.";
        StatusMessage = $"{ActiveWorkOffer.OfferId} 픽업을 확인했습니다.";
    }

    [RelayCommand]
    private void CompleteDelivery()
    {
        if (ActiveWorkOffer is null)
        {
            StatusMessage = "진행 중인 배달권이 없습니다.";
            return;
        }

        ActiveWorkOffer = ActiveWorkOffer with
        {
            Status = DriverWorkOfferStatus.Completed
        };
        WorkStage = "전달 완료";
        NextActionGuide = "정산 대기 상태로 전환되었습니다.";
        StatusMessage = $"{ActiveWorkOffer.OfferId} 전달을 완료했습니다.";
    }

    partial void OnSelectedTicketChanged(DeliveryTicketPreview? value)
    {
        RefreshRouteState();
    }

    partial void OnActiveWorkOfferChanged(DriverWorkOfferDto? value)
    {
        RefreshRouteState();
        OnPropertyChanged(nameof(ActiveWorkSummary));
        OnPropertyChanged(nameof(ActiveWorkRouteText));
        OnPropertyChanged(nameof(ActiveWorkPayoutText));
        OnPropertyChanged(nameof(HasActiveWork));
        OnPropertyChanged(nameof(CanConfirmPickup));
        OnPropertyChanged(nameof(CanCompleteDelivery));
        OnPropertyChanged(nameof(CanAcceptSelectedTicket));
    }

    partial void OnWorkStageChanged(string value)
    {
        OnPropertyChanged(nameof(ActiveWorkSummary));
    }

    private void RefreshRouteState()
    {
        OnPropertyChanged(nameof(SelectedRouteOverlays));
        OnPropertyChanged(nameof(MapCenterLatitude));
        OnPropertyChanged(nameof(MapCenterLongitude));
        OnPropertyChanged(nameof(PickupPointText));
        OnPropertyChanged(nameof(DropoffPointText));
        OnPropertyChanged(nameof(SelectedRouteText));
        OnPropertyChanged(nameof(CanAcceptSelectedTicket));
    }

    private static DriverMapMarkerItem ToMapMarker(DeliveryTicketPreview ticket)
    {
        return new DriverMapMarkerItem(
            ticket.TicketId,
            ticket.RestaurantLatitude,
            ticket.RestaurantLongitude,
            ticket.CustomerLatitude,
            ticket.CustomerLongitude,
            ticket.RestaurantName,
            $"{ticket.OrderSummary} · {ticket.DriverPayoutText}",
            ticket.RestaurantAddress,
            ticket.DropoffAddress,
            "음식점 픽업",
            "고객 전달");
    }
}

public sealed record DeliveryTicketPreview(
    string TicketId,
    string OrderSummary,
    string RestaurantName,
    string RestaurantAddress,
    string DropoffAddress,
    double RestaurantLatitude,
    double RestaurantLongitude,
    double CustomerLatitude,
    double CustomerLongitude,
    double DistanceKm,
    decimal DriverPayout,
    string RecommendationReason)
{
    public string DistanceText => $"{DistanceKm.ToString("0.0", CultureInfo.CurrentCulture)}km";

    public string DriverPayoutText => $"{DriverPayout.ToString("N0", CultureInfo.CurrentCulture)}원";

    public DriverWorkOfferDto ToDriverWorkOffer(FDriverAppProfile profile)
    {
        return new DriverWorkOfferDto(
            TicketId,
            profile.AppKey,
            profile.DriverDomain,
            profile.PrimaryWorkType,
            OrderSummary,
            $"{RestaurantName} 픽업 · {DropoffAddress} 전달",
            new DriverWorkStopDto(RestaurantName, RestaurantAddress, RestaurantLatitude, RestaurantLongitude, DateTimeOffset.Now.AddMinutes(8)),
            new DriverWorkStopDto("고객 주소", DropoffAddress, CustomerLatitude, CustomerLongitude, DateTimeOffset.Now.AddMinutes(24)),
            DriverPayout,
            DistanceKm,
            RecommendationReason);
    }
}
