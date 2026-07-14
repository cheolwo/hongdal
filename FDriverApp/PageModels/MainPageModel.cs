using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FDriverApp.Services;
using Hongdal.Contracts.Common.Drivers;
using Hongdal.Contracts.Driver.Food;
using Hongdal.Contracts.Driver.Work;

namespace FDriverApp.PageModels;

public sealed partial class MainPageModel : ObservableObject
{
    private const string DrivingStatus = "운행중";
    private readonly FDriverAppProfile _profile;
    private readonly IFDriverAuthSession _authSession;
    private readonly FDriverAuthApiService _authApi;
    private readonly IFoodDeliveryDriverApiService _api;
    private readonly IFDriverLocationService _locationService;
    private bool _initialized;

    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isAuthenticated;
    [ObservableProperty] private bool _isOnDuty;
    [ObservableProperty] private string _loginId = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _today = DateTime.Now.ToString("M월 d일 dddd");
    [ObservableProperty] private string _statusMessage = "기사 로그인 후 배달 업무를 시작할 수 있습니다.";
    [ObservableProperty] private string _currentArea = "현재 위치 확인 전";
    [ObservableProperty] private int _pendingDeliveryTickets;
    [ObservableProperty] private int _recommendedTickets;
    [ObservableProperty] private decimal _todayExpectedPayout;
    [ObservableProperty] private DeliveryTicketPreview? _selectedTicket;
    [ObservableProperty] private ActiveDeliveryPreview? _activeDelivery;
    [ObservableProperty] private string _workStage = "추천 대기";
    [ObservableProperty] private string _nextActionGuide = "운행을 시작하면 현재 위치 기준 추천을 확인합니다.";
    [ObservableProperty] private string _settlementText = "이번 달 정산 조회 전";
    [ObservableProperty] private string _routeStatusText = "경로 조회 전";
    [ObservableProperty] private IReadOnlyList<DriverMapMarkerItem> _mapMarkers = [];
    [ObservableProperty] private IReadOnlyList<DriverMapRouteOverlay> _selectedRouteOverlays = [];
    [ObservableProperty] private double _mapCenterLatitude = 37.5665d;
    [ObservableProperty] private double _mapCenterLongitude = 126.9780d;
    [ObservableProperty] private double _currentLocationLatitude;
    [ObservableProperty] private double _currentLocationLongitude;
    [ObservableProperty] private bool _hasCurrentLocation;

    public MainPageModel(
        FDriverAppProfile profile,
        IFDriverAuthSession authSession,
        FDriverAuthApiService authApi,
        IFoodDeliveryDriverApiService api,
        IFDriverLocationService locationService)
    {
        _profile = profile;
        _authSession = authSession;
        _authApi = authApi;
        _api = api;
        _locationService = locationService;
    }

    public string AppName => _profile.DisplayName;
    public string DriverRole => _profile.DriverRole;
    public bool IsSignedOut => !IsAuthenticated;
    public string SignedInUserText => string.IsNullOrWhiteSpace(_authSession.UserName)
        ? DriverRole
        : $"{_authSession.UserName} · {DriverRole}";
    public string WorkToggleText => IsOnDuty ? "운행 종료" : "운행 시작";
    public string WorkToggleColor => IsOnDuty ? "#B91C1C" : "#0F766E";
    public ObservableCollection<DeliveryTicketPreview> RecommendedTicketItems { get; } = [];
    public ObservableCollection<ActiveDeliveryPreview> ActiveDeliveryItems { get; } = [];
    public ObservableCollection<FoodDeliveryBundlePreview> BundleCandidateItems { get; } = [];

    public string PickupPointText => CurrentRouteOffer is null
        ? "음식점 픽업지 없음"
        : $"픽업: {CurrentRouteOffer.Pickup.Label} · {CurrentRouteOffer.Pickup.Address}";
    public string DropoffPointText => CurrentRouteOffer is null
        ? "고객 전달지 없음"
        : $"전달: {CurrentRouteOffer.Dropoff.Address}";
    public string SelectedRouteText => CurrentRouteOffer is null
        ? "선택된 배달권 없음"
        : $"{CurrentRouteOffer.Pickup.Label} → {CurrentRouteOffer.Dropoff.Label}";
    public string ActiveWorkSummary => ActiveDeliveryItems.Count switch
    {
        0 => "진행 중인 배달 없음",
        1 => $"{ActiveDeliveryItems[0].OrderSummary} · {WorkStage}",
        _ => $"묶음 배달 {ActiveDeliveryItems.Count}건 · {WorkStage}"
    };
    public string ActiveWorkRouteText => ActiveDelivery is null
        ? "수락한 배달권이 없습니다."
        : $"{ActiveDelivery.Pickup.Label} → {ActiveDelivery.Dropoff.Label}";
    public string ActiveWorkPayoutText => ActiveDeliveryItems.Count == 0
        ? "정산 예정 없음"
        : $"{ActiveDeliveryItems.Sum(x => x.DriverPayout).ToString("N0", CultureInfo.CurrentCulture)}원";
    public bool HasActiveWork => ActiveDelivery is not null;
    public bool CanConfirmPickup => !IsBusy && ActiveDelivery?.WorkStatus == DriverWorkOfferStatus.MovingToPickup;
    public bool CanCompleteDelivery => !IsBusy && ActiveDelivery?.WorkStatus == DriverWorkOfferStatus.MovingToDropoff;
    public bool CanAcceptSelectedTicket => !IsBusy && SelectedTicket is not null && ActiveDeliveryItems.Count < 3;
    public bool HasBundleCandidates => BundleCandidateItems.Count > 0;

    private DriverWorkOfferDto? CurrentRouteOffer
        => ActiveDelivery?.ToDriverWorkOffer(_profile) ?? SelectedTicket?.ToDriverWorkOffer(_profile);

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _authSession.RestoreAsync();
        IsAuthenticated = _authSession.IsAuthenticated;
        if (IsAuthenticated)
        {
            await ReloadAsync(updateLocation: true);
        }
    }

    public void ApplyEntryFocus(string? focus)
    {
        StatusMessage = focus?.Trim().ToLowerInvariant() switch
        {
            "restaurant" => "음식점 픽업 정보를 확인하세요. 선택한 배달권의 음식점 위치와 픽업 경로를 지도에 표시합니다.",
            "dispatch" => "배차 추천을 확인하세요. 지도 마커나 추천 배달권을 선택하면 경로가 이어집니다.",
            "delivery" => "운송·배달 흐름을 확인하세요. 픽업 확인부터 주문자 전달 완료까지 현재 단계를 이어서 처리합니다.",
            "customer" => "주문자 전달 정보를 확인하세요. 선택한 배달권의 전달 위치와 도착 경로를 지도에 표시합니다.",
            "bundle" => "묶음 배달 후보를 확인하세요. 동선과 예상 정산을 비교한 뒤 한 묶음만 선택할 수 있습니다.",
            "route" => "현재 위치와 픽업·전달 경로를 확인하세요. 선택한 배달권의 실제 도로 경로를 우선 표시합니다.",
            "settlement" => $"정산 현황을 확인하세요. {SettlementText}",
            "workspace" => "음식 배달 업무 공간을 열었습니다. 배차부터 전달 완료까지 한 흐름으로 처리합니다.",
            _ => StatusMessage
        };
    }

    [RelayCommand]
    private async Task Login()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "기사 계정을 확인하고 있습니다.";
        var error = await _authApi.LoginAsync(LoginId, Password);
        if (error is not null)
        {
            StatusMessage = error;
            IsBusy = false;
            return;
        }

        Password = string.Empty;
        IsAuthenticated = true;
        IsBusy = false;
        await ReloadAsync(updateLocation: true);
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authSession.ClearAsync();
        IsAuthenticated = false;
        IsOnDuty = false;
        ClearWorkspace();
        StatusMessage = "로그아웃했습니다.";
    }

    [RelayCommand]
    private void OpenProfile()
    {
        StatusMessage = $"로그인 기사: {SignedInUserText}";
    }

    [RelayCommand]
    private void OpenCurrentDelivery()
    {
        StatusMessage = ActiveDelivery is null
            ? "진행 중인 음식 배달이 없습니다."
            : $"{ActiveDelivery.OfferId} 현재 단계: {WorkStage}";
    }

    [RelayCommand]
    private async Task ToggleWork()
    {
        if (!IsAuthenticated || IsBusy)
        {
            return;
        }

        await RunApiAsync(async () =>
        {
            if (IsOnDuty)
            {
                await _api.StopWorkAsync();
                IsOnDuty = false;
                StatusMessage = "운행을 종료했습니다. 진행 중 배달은 계속 확인할 수 있습니다.";
            }
            else
            {
                var location = await CaptureLocationAsync(sendToServer: false);
                var startLocation = location is null
                    ? "음식 배달 앱 운행 시작"
                    : FormattableString.Invariant($"{location.Latitude:0.000000},{location.Longitude:0.000000}");
                await _api.StartWorkAsync(startLocation);
                IsOnDuty = true;
                await CaptureLocationAsync(sendToServer: true);
                StatusMessage = "운행을 시작했습니다. 현재 위치 기준 추천을 불러왔습니다.";
            }

            NotifyWorkState();
            await LoadWorkspaceAsync();
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (!IsAuthenticated || IsRefreshing)
        {
            return;
        }

        IsRefreshing = true;
        await ReloadAsync(updateLocation: IsOnDuty);
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SelectTicket(DeliveryTicketPreview ticket)
    {
        SelectedTicket = ticket;
        StatusMessage = $"{ticket.TicketId} 배달권을 선택했습니다.";
        await RefreshRouteAsync();
    }

    public Task SelectTicketByIdAsync(string ticketId)
    {
        var ticket = RecommendedTicketItems.FirstOrDefault(x => string.Equals(x.TicketId, ticketId, StringComparison.Ordinal));
        return ticket is null ? Task.CompletedTask : SelectTicket(ticket);
    }

    [RelayCommand]
    private async Task AcceptTicket(DeliveryTicketPreview ticket)
    {
        SelectedTicket = ticket;
        await AcceptOfferAsync(ticket.TicketId);
    }

    [RelayCommand]
    private async Task AcceptSelectedTicket()
    {
        if (SelectedTicket is null)
        {
            StatusMessage = "선택된 배달권이 없습니다.";
            return;
        }

        await AcceptOfferAsync(SelectedTicket.TicketId);
    }

    [RelayCommand]
    private async Task AcceptBundle(FoodDeliveryBundlePreview bundle)
    {
        await RunApiAsync(async () =>
        {
            var result = await _api.AcceptBundleAsync(bundle.OfferIds);
            StatusMessage = result.Message;
            await LoadWorkspaceAsync();
        });
    }

    [RelayCommand]
    private async Task ConfirmPickup()
    {
        if (ActiveDelivery is null)
        {
            StatusMessage = "진행 중인 배달이 없습니다.";
            return;
        }

        await RunApiAsync(async () =>
        {
            var result = await _api.ConfirmPickupAsync(ActiveDelivery.OfferId);
            StatusMessage = result.Message;
            await LoadWorkspaceAsync();
        });
    }

    [RelayCommand]
    private async Task CompleteDelivery()
    {
        if (ActiveDelivery is null)
        {
            StatusMessage = "진행 중인 배달이 없습니다.";
            return;
        }

        await RunApiAsync(async () =>
        {
            var result = await _api.CompleteAsync(ActiveDelivery.OfferId);
            StatusMessage = result.Message;
            await LoadWorkspaceAsync();
        });
    }

    private async Task AcceptOfferAsync(string offerId)
    {
        await RunApiAsync(async () =>
        {
            var result = await _api.AcceptAsync(offerId);
            StatusMessage = result.Message;
            await LoadWorkspaceAsync();
        });
    }

    private async Task ReloadAsync(bool updateLocation)
    {
        await RunApiAsync(async () =>
        {
            var workStatus = await _api.GetWorkStatusAsync();
            IsOnDuty = string.Equals(workStatus?.Status, DrivingStatus, StringComparison.OrdinalIgnoreCase);
            NotifyWorkState();
            if (updateLocation && IsOnDuty)
            {
                await CaptureLocationAsync(sendToServer: true);
            }

            await LoadWorkspaceAsync();
        });
    }

    private async Task LoadWorkspaceAsync()
    {
        var workspace = await _api.GetWorkspaceAsync();
        RecommendedTicketItems.Clear();
        foreach (var item in workspace.Recommendations)
        {
            RecommendedTicketItems.Add(DeliveryTicketPreview.From(item));
        }

        ActiveDeliveryItems.Clear();
        foreach (var item in workspace.ActiveDeliveries)
        {
            ActiveDeliveryItems.Add(ActiveDeliveryPreview.From(item));
        }

        BundleCandidateItems.Clear();
        foreach (var item in workspace.BundleCandidates)
        {
            BundleCandidateItems.Add(FoodDeliveryBundlePreview.From(item));
        }

        ActiveDelivery = ActiveDeliveryItems.FirstOrDefault();
        SelectedTicket = SelectedTicket is null
            ? RecommendedTicketItems.FirstOrDefault()
            : RecommendedTicketItems.FirstOrDefault(x => x.TicketId == SelectedTicket.TicketId)
              ?? RecommendedTicketItems.FirstOrDefault();
        PendingDeliveryTickets = RecommendedTicketItems.Count + ActiveDeliveryItems.Count;
        RecommendedTickets = RecommendedTicketItems.Count;
        TodayExpectedPayout = RecommendedTicketItems.Sum(x => x.DriverPayout);
        SettlementText = $"{workspace.Settlement.년도}년 {workspace.Settlement.월}월 · "
                         + $"배차 {workspace.Settlement.배차건수:N0}건 · 이용료 {workspace.Settlement.이용료:N0}원"
                         + (workspace.Settlement.결제완료 ? " · 납부 완료" : string.Empty);
        MapMarkers = RecommendedTicketItems.Select(ToMapMarker)
            .Concat(ActiveDeliveryItems.Select(ToMapMarker))
            .ToArray();
        SetWorkStage();
        NotifyWorkspaceState();
        await RefreshRouteAsync();

        if (string.IsNullOrWhiteSpace(StatusMessage) || StatusMessage.Contains("불러오", StringComparison.Ordinal))
        {
            StatusMessage = $"추천 {RecommendedTicketItems.Count}건, 진행 {ActiveDeliveryItems.Count}건을 확인했습니다.";
        }
    }

    private async Task<FDriverLocationSnapshot?> CaptureLocationAsync(bool sendToServer)
    {
        var location = await _locationService.GetCurrentAsync();
        if (location is null)
        {
            CurrentArea = "위치 권한 또는 GPS 확인 필요";
            return null;
        }

        CurrentLocationLatitude = (double)location.Latitude;
        CurrentLocationLongitude = (double)location.Longitude;
        HasCurrentLocation = true;
        MapCenterLatitude = CurrentLocationLatitude;
        MapCenterLongitude = CurrentLocationLongitude;
        CurrentArea = FormattableString.Invariant($"현재 위치 {location.Latitude:0.0000}, {location.Longitude:0.0000}");
        if (sendToServer)
        {
            await _api.UpdateLocationAsync(new 기사위치갱신요청
            {
                AppKey = _profile.AppKey,
                위도 = location.Latitude,
                경도 = location.Longitude,
                정확도_m = location.AccuracyMeters,
                상차접근허용반경Km = 3m,
                운행상태 = DrivingStatus,
                기록시각 = location.RecordedAtUtc
            });
        }

        return location;
    }

    private async Task RefreshRouteAsync()
    {
        var routeStops = BuildRouteStops();
        if (routeStops.Count == 0)
        {
            SelectedRouteOverlays = [];
            RouteStatusText = "표시할 경로가 없습니다.";
            RefreshRouteLabels();
            return;
        }

        var startLatitude = HasCurrentLocation
            ? (decimal)CurrentLocationLatitude
            : routeStops[0].Latitude;
        var startLongitude = HasCurrentLocation
            ? (decimal)CurrentLocationLongitude
            : routeStops[0].Longitude;
        var stops = HasCurrentLocation ? routeStops : routeStops.Skip(1).ToArray();
        if (stops.Count == 0)
        {
            SelectedRouteOverlays = [];
            return;
        }

        try
        {
            var route = await _api.GetRouteAsync(new FoodDeliveryDriverRouteRequestDto
            {
                StartLatitude = startLatitude,
                StartLongitude = startLongitude,
                Stops = stops
            });
            SelectedRouteOverlays = route.Points.Count < 2
                ? []
                :
                [
                    new DriverMapRouteOverlay(
                        ActiveDelivery?.OfferId ?? SelectedTicket?.TicketId ?? "food-route",
                        "음식 배달 경로",
                        route.Points.Select((x, index) => new DriverMapRoutePoint(
                            (double)x.Latitude,
                            (double)x.Longitude,
                            index == 0 ? "현재 위치" : "배달 경로")).ToArray(),
                        StrokeColor: route.IsEstimated ? "#64748B" : "#2563EB")
                ];
            RouteStatusText = $"{(route.IsEstimated ? "추정 경로" : "실시간 도로 경로")} · {route.DistanceKm:0.0}km · 약 {route.DurationMinutes}분";
        }
        catch (FDriverApiException ex)
        {
            RouteStatusText = $"경로 조회 실패 · {ShortMessage(ex.Message)}";
            SelectedRouteOverlays = [];
        }

        RefreshRouteLabels();
    }

    private IReadOnlyList<FoodDeliveryDriverRouteStopDto> BuildRouteStops()
    {
        if (ActiveDeliveryItems.Count > 0)
        {
            var pickups = ActiveDeliveryItems
                .Where(x => x.WorkStatus == DriverWorkOfferStatus.MovingToPickup)
                .Select(x => x.Pickup)
                .Where(HasCoordinates)
                .Select(x => ToRouteStop($"픽업 · {x.Label}", x));
            var dropoffs = ActiveDeliveryItems
                .Select(x => x.Dropoff)
                .Where(HasCoordinates)
                .Select(x => ToRouteStop($"전달 · {x.Label}", x));
            return pickups.Concat(dropoffs).Take(6).ToArray();
        }

        if (SelectedTicket is null)
        {
            return [];
        }

        return new[] { SelectedTicket.Pickup, SelectedTicket.Dropoff }
            .Where(HasCoordinates)
            .Select(x => ToRouteStop(x.Label, x))
            .ToArray();
    }

    private async Task RunApiAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        NotifyCommandState();
        try
        {
            await action();
        }
        catch (FDriverApiException ex)
        {
            StatusMessage = ShortMessage(ex.Message);
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authSession.ClearAsync();
                IsAuthenticated = false;
                ClearWorkspace();
            }
        }
        finally
        {
            IsBusy = false;
            NotifyCommandState();
        }
    }

    private void SetWorkStage()
    {
        WorkStage = ActiveDelivery?.WorkStatus switch
        {
            DriverWorkOfferStatus.MovingToPickup when ActiveDeliveryItems.Count > 1 => $"묶음 픽업 {ActiveDeliveryItems.Count}건",
            DriverWorkOfferStatus.MovingToPickup => "음식점 이동 중",
            DriverWorkOfferStatus.MovingToDropoff when ActiveDeliveryItems.Count > 1 => $"묶음 전달 {ActiveDeliveryItems.Count}건",
            DriverWorkOfferStatus.MovingToDropoff => "고객 주소 이동 중",
            _ => "추천 대기"
        };
        NextActionGuide = ActiveDelivery?.WorkStatus switch
        {
            DriverWorkOfferStatus.MovingToPickup => "음식점에서 주문을 받은 뒤 픽업 확인을 눌러 주세요.",
            DriverWorkOfferStatus.MovingToDropoff => "고객에게 전달한 뒤 전달 완료를 눌러 주세요.",
            _ when IsOnDuty => "지도에서 추천 배달권을 선택하세요.",
            _ => "운행 시작을 눌러 현재 위치 추천을 활성화하세요."
        };
    }

    private void ClearWorkspace()
    {
        RecommendedTicketItems.Clear();
        ActiveDeliveryItems.Clear();
        BundleCandidateItems.Clear();
        SelectedTicket = null;
        ActiveDelivery = null;
        MapMarkers = [];
        SelectedRouteOverlays = [];
        PendingDeliveryTickets = 0;
        RecommendedTickets = 0;
        TodayExpectedPayout = 0m;
        SettlementText = "이번 달 정산 조회 전";
        NotifyWorkspaceState();
    }

    partial void OnIsAuthenticatedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSignedOut));
        OnPropertyChanged(nameof(SignedInUserText));
    }

    partial void OnIsOnDutyChanged(bool value) => NotifyWorkState();
    partial void OnSelectedTicketChanged(DeliveryTicketPreview? value) => RefreshRouteLabels();
    partial void OnActiveDeliveryChanged(ActiveDeliveryPreview? value)
    {
        RefreshRouteLabels();
        NotifyWorkspaceState();
    }

    partial void OnWorkStageChanged(string value) => OnPropertyChanged(nameof(ActiveWorkSummary));

    private void NotifyWorkState()
    {
        OnPropertyChanged(nameof(WorkToggleText));
        OnPropertyChanged(nameof(WorkToggleColor));
    }

    private void NotifyWorkspaceState()
    {
        OnPropertyChanged(nameof(ActiveWorkSummary));
        OnPropertyChanged(nameof(ActiveWorkRouteText));
        OnPropertyChanged(nameof(ActiveWorkPayoutText));
        OnPropertyChanged(nameof(HasActiveWork));
        OnPropertyChanged(nameof(HasBundleCandidates));
        NotifyCommandState();
    }

    private void NotifyCommandState()
    {
        OnPropertyChanged(nameof(CanConfirmPickup));
        OnPropertyChanged(nameof(CanCompleteDelivery));
        OnPropertyChanged(nameof(CanAcceptSelectedTicket));
    }

    private void RefreshRouteLabels()
    {
        OnPropertyChanged(nameof(PickupPointText));
        OnPropertyChanged(nameof(DropoffPointText));
        OnPropertyChanged(nameof(SelectedRouteText));
    }

    private static DriverMapMarkerItem ToMapMarker(DeliveryTicketPreview ticket)
        => new(
            ticket.TicketId,
            ticket.Pickup.Latitude,
            ticket.Pickup.Longitude,
            ticket.Dropoff.Latitude,
            ticket.Dropoff.Longitude,
            ticket.RestaurantName,
            $"{ticket.OrderSummary} · {ticket.DriverPayoutText}",
            ticket.Pickup.Address,
            ticket.Dropoff.Address,
            "음식점 픽업",
            "고객 전달");

    private static DriverMapMarkerItem ToMapMarker(ActiveDeliveryPreview delivery)
        => new(
            delivery.OfferId,
            delivery.Pickup.Latitude,
            delivery.Pickup.Longitude,
            delivery.Dropoff.Latitude,
            delivery.Dropoff.Longitude,
            delivery.RestaurantName,
            $"진행 중 · {delivery.OrderSummary}",
            delivery.Pickup.Address,
            delivery.Dropoff.Address,
            "진행 픽업",
            "진행 전달");

    private static bool HasCoordinates(DriverWorkStopDto stop)
        => stop.Latitude != 0d && stop.Longitude != 0d;

    private static FoodDeliveryDriverRouteStopDto ToRouteStop(string label, DriverWorkStopDto stop)
        => new()
        {
            Label = label,
            Latitude = (decimal)stop.Latitude,
            Longitude = (decimal)stop.Longitude
        };

    private static string ShortMessage(string message)
    {
        var clean = message.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return clean.Length <= 180 ? clean : $"{clean[..180]}…";
    }
}

public sealed record DeliveryTicketPreview(
    string TicketId,
    string OrderSummary,
    string RestaurantName,
    DriverWorkStopDto Pickup,
    DriverWorkStopDto Dropoff,
    double DistanceKm,
    decimal DriverPayout,
    string RecommendationReason)
{
    public string RestaurantAddress => Pickup.Address;
    public string DropoffAddress => Dropoff.Address;
    public string DistanceText => $"{DistanceKm.ToString("0.0", CultureInfo.CurrentCulture)}km";
    public string DriverPayoutText => $"{DriverPayout.ToString("N0", CultureInfo.CurrentCulture)}원";

    public static DeliveryTicketPreview From(FoodDeliveryDriverOfferDto item)
        => new(
            item.OfferId,
            item.OrderSummary,
            item.RestaurantName,
            ToStop(item.Pickup),
            ToStop(item.Dropoff),
            (double)(item.DistanceKm ?? 0m),
            item.DriverPayout,
            item.RecommendationReason);

    public DriverWorkOfferDto ToDriverWorkOffer(FDriverAppProfile profile)
        => new(
            TicketId,
            profile.AppKey,
            profile.DriverDomain,
            profile.PrimaryWorkType,
            OrderSummary,
            $"{RestaurantName} 픽업 · {Dropoff.Address} 전달",
            Pickup,
            Dropoff,
            DriverPayout,
            DistanceKm,
            RecommendationReason);

    internal static DriverWorkStopDto ToStop(FoodDeliveryDriverStopDto stop)
        => new(
            stop.Label,
            stop.Address,
            (double)(stop.Latitude ?? 0m),
            (double)(stop.Longitude ?? 0m),
            stop.TargetAtUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(stop.TargetAtUtc.Value, DateTimeKind.Utc))
                : null);
}

public sealed record ActiveDeliveryPreview(
    long TransportId,
    string OfferId,
    string OrderSummary,
    string RestaurantName,
    DriverWorkStopDto Pickup,
    DriverWorkStopDto Dropoff,
    decimal DriverPayout,
    string TransportStatus,
    string WorkStatus)
{
    public static ActiveDeliveryPreview From(FoodDeliveryDriverActiveDeliveryDto item)
        => new(
            item.TransportId,
            item.OfferId,
            item.OrderSummary,
            item.RestaurantName,
            DeliveryTicketPreview.ToStop(item.Pickup),
            DeliveryTicketPreview.ToStop(item.Dropoff),
            item.DriverPayout,
            item.TransportStatus,
            item.WorkStatus);

    public DriverWorkOfferDto ToDriverWorkOffer(FDriverAppProfile profile)
        => new(
            OfferId,
            profile.AppKey,
            profile.DriverDomain,
            profile.PrimaryWorkType,
            OrderSummary,
            $"{RestaurantName} 픽업 · {Dropoff.Address} 전달",
            Pickup,
            Dropoff,
            DriverPayout,
            null,
            "진행 중인 음식 배달",
            WorkStatus);
}

public sealed record FoodDeliveryBundlePreview(
    string BundleId,
    IReadOnlyList<string> OfferIds,
    string Title,
    string Reason,
    decimal TotalPayout,
    decimal EstimatedRouteKm)
{
    public string CountText => $"{OfferIds.Count}건 묶음";
    public string PayoutText => $"{TotalPayout:N0}원";
    public string RouteText => $"예상 {EstimatedRouteKm:0.0}km";

    public static FoodDeliveryBundlePreview From(FoodDeliveryBundleCandidateDto item)
        => new(item.BundleId, item.OfferIds, item.Title, item.Reason, item.TotalPayout, item.EstimatedRouteKm);
}
