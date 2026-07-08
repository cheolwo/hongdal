using DriverApp.Models.Driver.Map;
using DriverApp.Models.Driver.Samples;
using DriverApp.Models.Driver;
using DriverApp.Services;
using System.Globalization;

namespace DriverApp;

public partial class NativeDriverHomePage : ContentPage
{
    private readonly IDriverSampleDataService _sampleDataService;
    private readonly IDriverHomeMapService _mapService;
    private readonly IDriverRecommendationDecisionService _decisionService;
    private readonly IDriverRecommendationNotificationService _recommendationNotificationService;
    private bool _isSubscribed;
    private DriverMapMarkerItem? _incomingRecommendation;
    private DriverRequestItem? _incomingRecommendationRequest;
    private DriverRequestItem? _acceptedRecommendationRequest;
    private 기사운송샘플항목? _currentTransport;
    private IReadOnlyList<DriverMapMarkerItem> _defaultRecommendationMarkers = [];
    private double _defaultCenterLatitude;
    private double _defaultCenterLongitude;
    private const double DefaultMapZoom = 11d;
    private IDispatcherTimer? _recommendationCountdownTimer;
    private bool _autoRejectingRecommendation;

    public NativeDriverHomePage(
        IDriverSampleDataService sampleDataService,
        IDriverHomeMapService mapService,
        IDriverRecommendationDecisionService decisionService,
        IDriverRecommendationNotificationService recommendationNotificationService)
    {
        InitializeComponent();
        _sampleDataService = sampleDataService;
        _mapService = mapService;
        _decisionService = decisionService;
        _recommendationNotificationService = recommendationNotificationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SubscribeEvents();
        await _sampleDataService.RefreshAsync();

        var currentLocation = _sampleDataService.기사현재위치;
        var markers = _mapService.BuildMarkers(_sampleDataService.추천의뢰목록);
        _defaultRecommendationMarkers = markers;
        _defaultCenterLatitude = (double)currentLocation.위도;
        _defaultCenterLongitude = (double)currentLocation.경도;
        var incoming = _recommendationNotificationService.GetCurrent();
        _incomingRecommendation = incoming?.Marker;
        _incomingRecommendationRequest = incoming?.Request;
        _currentTransport = _sampleDataService.현재운송조회();

        RestoreDefaultMapState();

        StatusLabel.Text = $"{currentLocation.위치명} 기준 추천 운송 {markers.Count}건을 네이티브 지도에 표시합니다.";
        TransportFooterBar.ShowTransport(_currentTransport);
        ShowIncomingRecommendation(incoming);
        LinkedRouteCard.IsVisible = false;
        RecommendationDecisionButtons.IsEnabled = true;
    }

    protected override void OnDisappearing()
    {
        StopRecommendationCountdown();
        UnsubscribeEvents();
        base.OnDisappearing();
    }

    private void OnMarkerSelected(object? sender, DriverMapMarkerItem marker)
    {
        TransportFooterBar.ShowMarker(marker);
    }

    private void ShowIncomingRecommendation(DriverIncomingRecommendation? incoming)
    {
        var marker = incoming?.Marker;
        RecommendationBanner.IsVisible = marker is not null;
        if (marker is null)
        {
            RecommendationSummaryLabel.Text = string.Empty;
            RecommendationCountdownLabel.Text = string.Empty;
            RecommendationCountdownProgress.Progress = 0d;
            StopRecommendationCountdown();
            return;
        }

        RecommendationSummaryLabel.Text = $"{marker.Title} · {marker.PickupAddress} 상차 추천이 도착했습니다. 대기 {incoming!.PendingCount}건";
        StartRecommendationCountdown(incoming);
    }

    private void StartRecommendationCountdown(DriverIncomingRecommendation incoming)
    {
        _autoRejectingRecommendation = false;
        StopRecommendationCountdown();

        _recommendationCountdownTimer = Dispatcher.CreateTimer();
        _recommendationCountdownTimer.Interval = TimeSpan.FromSeconds(1);
        _recommendationCountdownTimer.Tick += async (_, _) => await UpdateRecommendationCountdownAsync(incoming);
        _recommendationCountdownTimer.Start();

        _ = UpdateRecommendationCountdownAsync(incoming);
    }

    private void StopRecommendationCountdown()
    {
        if (_recommendationCountdownTimer is null)
        {
            return;
        }

        _recommendationCountdownTimer.Stop();
        _recommendationCountdownTimer = null;
    }

    private async Task UpdateRecommendationCountdownAsync(DriverIncomingRecommendation incoming)
    {
        var request = incoming.Request;
        var expiresAt = request.추천만료시각?.ToLocalTime() ?? incoming.ReceivedAt.AddSeconds(60);
        var startsAt = request.추천시작시각?.ToLocalTime() ?? incoming.ReceivedAt;
        var totalSeconds = Math.Max(1d, (expiresAt - startsAt).TotalSeconds);
        var remainingSeconds = Math.Max(0d, (expiresAt - DateTime.Now).TotalSeconds);
        var progress = Math.Clamp((totalSeconds - remainingSeconds) / totalSeconds, 0d, 1d);

        RecommendationCountdownLabel.Text = remainingSeconds > 0d
            ? $"{Math.Ceiling(remainingSeconds):0}초"
            : "미응답 처리";
        RecommendationCountdownProgress.Progress = progress;
        RecommendationCountdownProgress.ProgressColor = remainingSeconds > 10d ? Color.FromArgb("#22c55e") : Color.FromArgb("#f59e0b");

        if (remainingSeconds > 0d || _autoRejectingRecommendation || _decisionService.GetDecision(request.의뢰Id) is not null)
        {
            return;
        }

        _autoRejectingRecommendation = true;
        try
        {
            var decision = await _decisionService.RejectAsync(request, "60초 미응답 자동 거절");
            RecommendationCountdownLabel.Text = "자동 거절";
            RecommendationDecisionStatusLabel.Text = BuildDecisionMessage(decision);
            _recommendationNotificationService.MarkHandled(request.의뢰Id);
            RecommendationBanner.IsVisible = false;
            RestoreDefaultMapState();
            StatusLabel.Text = "응답 제한 시간이 지나 추천이 자동 거절되었습니다.";
        }
        catch (Exception ex)
        {
            _autoRejectingRecommendation = false;
            RecommendationCountdownLabel.Text = "처리 실패";
            StatusLabel.Text = ex.Message;
        }
        finally
        {
            StopRecommendationCountdown();
        }
    }

    private void OnOpenIncomingRecommendationClicked(object? sender, EventArgs e)
    {
        if (_incomingRecommendation is null)
        {
            RecommendationBanner.IsVisible = false;
            return;
        }

        MapView.CenterLatitude = _incomingRecommendation.PickupLatitude;
        MapView.CenterLongitude = _incomingRecommendation.PickupLongitude;
        MapView.Zoom = 13d;
        MapView.Markers = [_incomingRecommendation];
        MapView.RouteOverlays = BuildLinkedRouteOverlays(_currentTransport, _incomingRecommendation);
        TransportFooterBar.ShowMarker(_incomingRecommendation);
        ShowLinkedRouteCard(_currentTransport, _incomingRecommendation, _incomingRecommendationRequest);
        TitleLabel.Text = "배차 추천 확인";
        StatusLabel.Text = "현재 운송 하차 이후 이어 받을 수 있는 추천 경로를 지도에 표시했습니다.";
        RecommendationBanner.IsVisible = false;
    }

    private void OnDismissRecommendationClicked(object? sender, EventArgs e)
    {
        if (_incomingRecommendationRequest is not null)
        {
            _recommendationNotificationService.MarkDismissed(_incomingRecommendationRequest.의뢰Id);
        }

        RecommendationBanner.IsVisible = false;
        StatusLabel.Text = "추천 알림을 잠시 접었습니다. 지도 마커나 하단 목록에서 다시 확인할 수 있습니다.";
    }

    private IReadOnlyList<DriverMapRouteOverlay> BuildLinkedRouteOverlays(
        기사운송샘플항목? currentTransport,
        DriverMapMarkerItem recommendation)
    {
        var points = new List<DriverMapRoutePoint>();
        if (HasDropoffCoordinate(currentTransport))
        {
            points.Add(new DriverMapRoutePoint(
                (double)currentTransport!.하차위도!.Value,
                (double)currentTransport.하차경도!.Value,
                "현재 운송 하차지"));
        }

        points.Add(new DriverMapRoutePoint(
            recommendation.PickupLatitude,
            recommendation.PickupLongitude,
            "추천 상차지"));

        if (recommendation.DropoffLatitude != 0d && recommendation.DropoffLongitude != 0d)
        {
            points.Add(new DriverMapRoutePoint(
                recommendation.DropoffLatitude,
                recommendation.DropoffLongitude,
                "추천 하차지"));
        }

        return points.Count < 2
            ? []
            :
            [
                new DriverMapRouteOverlay(
                    recommendation.RequestId,
                    "연계 추천 경로",
                    points,
                    "#16a34a",
                    "#ecfdf5",
                    10)
            ];
    }

    private static IReadOnlyList<DriverMapRouteOverlay> BuildAcceptedRouteOverlays(DriverMapMarkerItem recommendation)
    {
        if (recommendation.DropoffLatitude == 0d || recommendation.DropoffLongitude == 0d)
        {
            return [];
        }

        return
        [
            new DriverMapRouteOverlay(
                recommendation.RequestId,
                "수락 운송 경로",
                [
                    new DriverMapRoutePoint(
                        recommendation.PickupLatitude,
                        recommendation.PickupLongitude,
                        "상차지"),
                    new DriverMapRoutePoint(
                        recommendation.DropoffLatitude,
                        recommendation.DropoffLongitude,
                        "하차지")
                ],
                "#2563eb",
                "#eff6ff",
                11)
        ];
    }

    private void ShowLinkedRouteCard(
        기사운송샘플항목? currentTransport,
        DriverMapMarkerItem recommendation,
        DriverRequestItem? request)
    {
        LinkedRouteCard.IsVisible = true;
        var emptyDistance = CalculateDistanceKm(currentTransport, recommendation);
        LinkedRouteSummaryLabel.Text = emptyDistance.HasValue
            ? $"현재 하차 후 추천 상차지까지 약 {emptyDistance.Value.ToString("0.0", CultureInfo.CurrentCulture)}km 연계됩니다."
            : "현재 하차 후 추천 상차지로 이어지는 후보 경로입니다.";
        LinkedRouteBenefitLabel.Text = emptyDistance.HasValue && emptyDistance.Value <= 8d
            ? "공차 유리"
            : "연계 검토";
        CurrentDropoffRouteLabel.Text = currentTransport is null
            ? "현재 운송 하차지: 진행 중 운송 없음"
            : $"현재 하차지: {currentTransport.하차지}";
        RecommendationPickupRouteLabel.Text = $"추천 상차지: {recommendation.PickupAddress}";
        RecommendationDropoffRouteLabel.Text = recommendation.DropoffLatitude != 0d && recommendation.DropoffLongitude != 0d
            ? $"추천 하차지: {recommendation.Summary}"
            : "추천 하차지: 추천 상세에서 확인";
        RecommendationCompactInfoLabel.Text = BuildRecommendationCompactInfo(request);
        RecommendationDecisionStatusLabel.Text = "경로와 수익을 확인한 뒤 수락, 보류, 거절을 선택할 수 있습니다.";
        RecommendationDecisionButtons.IsEnabled = true;
    }

    private async void OnAcceptRecommendationClicked(object? sender, EventArgs e)
    {
        if (_incomingRecommendationRequest is null)
        {
            RecommendationDecisionStatusLabel.Text = "처리할 추천 의뢰를 찾지 못했습니다.";
            return;
        }

        RecommendationDecisionButtons.IsEnabled = false;
        RecommendationDecisionState decision;
        try
        {
            decision = await _decisionService.AcceptAsync(_incomingRecommendationRequest);
        }
        catch (Exception ex)
        {
            RecommendationDecisionStatusLabel.Text = ex.Message;
            RecommendationDecisionButtons.IsEnabled = true;
            return;
        }

        _acceptedRecommendationRequest = _incomingRecommendationRequest;
        RecommendationDecisionStatusLabel.Text = BuildDecisionMessage(decision);
        RecommendationBanner.IsVisible = false;
        StopRecommendationCountdown();
        LinkedRouteBenefitLabel.Text = "수락됨";
        ShowAcceptedRecommendationOnMap();
        _recommendationNotificationService.MarkHandled(_incomingRecommendationRequest.의뢰Id);
        StatusLabel.Text = "배차 추천을 수락했습니다. 이제 추천 상차지로 이동해 상차 확인을 진행합니다.";
    }

    private void OnHoldRecommendationClicked(object? sender, EventArgs e)
    {
        if (_incomingRecommendationRequest is null)
        {
            RecommendationDecisionStatusLabel.Text = "처리할 추천 의뢰를 찾지 못했습니다.";
            return;
        }

        var decision = _decisionService.Hold(_incomingRecommendationRequest);
        RecommendationDecisionStatusLabel.Text = BuildDecisionMessage(decision);
        StatusLabel.Text = "추천을 보류했습니다. 지도나 추천 목록에서 다시 확인할 수 있습니다.";
    }

    private async void OnRejectRecommendationClicked(object? sender, EventArgs e)
    {
        if (_incomingRecommendationRequest is null)
        {
            RecommendationDecisionStatusLabel.Text = "처리할 추천 의뢰를 찾지 못했습니다.";
            return;
        }

        RecommendationDecisionButtons.IsEnabled = false;
        RecommendationDecisionState decision;
        try
        {
            decision = await _decisionService.RejectAsync(_incomingRecommendationRequest, "지도 연계 경로에서 거절");
        }
        catch (Exception ex)
        {
            RecommendationDecisionStatusLabel.Text = ex.Message;
            RecommendationDecisionButtons.IsEnabled = true;
            return;
        }

        RecommendationDecisionStatusLabel.Text = BuildDecisionMessage(decision);
        LinkedRouteBenefitLabel.Text = "거절됨";
        StopRecommendationCountdown();
        _recommendationNotificationService.MarkHandled(_incomingRecommendationRequest.의뢰Id);
        LinkedRouteCard.IsVisible = false;
        RecommendationBanner.IsVisible = false;
        _incomingRecommendation = null;
        _incomingRecommendationRequest = null;
        RestoreDefaultMapState();
        StatusLabel.Text = "추천을 거절했습니다. 다른 추천이 들어오면 다시 알려드립니다.";
    }

    private async void OnCancelAcceptedRecommendationClicked(object? sender, EventArgs e)
    {
        var request = _acceptedRecommendationRequest ?? _incomingRecommendationRequest;
        if (request is null)
        {
            RecommendationDecisionStatusLabel.Text = "취소할 추천 의뢰를 찾지 못했습니다.";
            return;
        }

        var currentDecision = _decisionService.GetDecision(request.의뢰Id);
        if (currentDecision?.Decision != DriverRecommendationDecisionCode.Accepted)
        {
            RecommendationDecisionStatusLabel.Text = "수락한 추천만 취소할 수 있습니다.";
            return;
        }

        RecommendationDecisionButtons.IsEnabled = false;
        RecommendationDecisionState decision;
        try
        {
            decision = await _decisionService.CancelAcceptedAsync(request, "지도 화면에서 수락 취소");
        }
        catch (Exception ex)
        {
            RecommendationDecisionStatusLabel.Text = ex.Message;
            RecommendationDecisionButtons.IsEnabled = true;
            return;
        }

        RecommendationDecisionStatusLabel.Text = BuildDecisionMessage(decision);
        LinkedRouteBenefitLabel.Text = "재배차";
        StopRecommendationCountdown();
        _recommendationNotificationService.MarkHandled(request.의뢰Id);
        _acceptedRecommendationRequest = null;
        LinkedRouteCard.IsVisible = false;
        RecommendationBanner.IsVisible = false;
        _incomingRecommendation = null;
        _incomingRecommendationRequest = null;
        RestoreDefaultMapState();
        StatusLabel.Text = "수락을 취소했습니다. 서버에서는 재배차와 화주 알림이 필요한 상태로 처리됩니다.";
    }

    private static string BuildDecisionMessage(RecommendationDecisionState decision)
        => $"{decision.Decision} · {decision.DecidedAt:HH:mm} · {decision.Memo} / 서버 후속: {decision.FollowUpPlan.OperationalMemo}";

    private void RestoreDefaultMapState()
    {
        MapView.CenterLatitude = _defaultCenterLatitude;
        MapView.CenterLongitude = _defaultCenterLongitude;
        MapView.Zoom = DefaultMapZoom;
        MapView.Markers = _defaultRecommendationMarkers;
        MapView.RouteOverlays = [];
        TransportFooterBar.ShowTransport(_currentTransport);
    }

    private void ShowAcceptedRecommendationOnMap()
    {
        if (_incomingRecommendation is null)
        {
            return;
        }

        MapView.CenterLatitude = _incomingRecommendation.PickupLatitude;
        MapView.CenterLongitude = _incomingRecommendation.PickupLongitude;
        MapView.Zoom = 13d;
        MapView.Markers = [_incomingRecommendation];
        var acceptedRoute = BuildAcceptedRouteOverlays(_incomingRecommendation);
        MapView.RouteOverlays = acceptedRoute.Count > 0
            ? acceptedRoute
            : BuildLinkedRouteOverlays(_currentTransport, _incomingRecommendation);
        TransportFooterBar.ShowMarker(_incomingRecommendation);
    }

    private static string BuildRecommendationCompactInfo(DriverRequestItem? request)
    {
        if (request is null)
        {
            return "운송 의뢰 상세 정보는 추천 상세 화면에서 확인할 수 있습니다.";
        }

        var fare = request.예상수익.HasValue
            ? $"예상수익 {request.예상수익.Value.ToString("N0", CultureInfo.CurrentCulture)}원"
            : "예상수익 미정";
        var distance = request.주행거리Km.HasValue
            ? $"주행 {request.주행거리Km.Value.ToString("0.0", CultureInfo.CurrentCulture)}km"
            : request.직선거리Km.HasValue
                ? $"직선 {request.직선거리Km.Value.ToString("0.0", CultureInfo.CurrentCulture)}km"
                : "거리 미정";

        return $"{request.추천업무유형표시} · {request.차량조건표시} · {request.시간조건표시} · {fare} · {distance}";
    }

    private void OnIncomingRecommendationChanged(DriverIncomingRecommendation? incoming)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _incomingRecommendation = incoming?.Marker;
            _incomingRecommendationRequest = incoming?.Request;
            ShowIncomingRecommendation(incoming);
            if (incoming is not null)
            {
                LinkedRouteCard.IsVisible = false;
            }
        });
    }

    private void SubscribeEvents()
    {
        if (_isSubscribed)
        {
            return;
        }

        MapView.MarkerSelected += OnMarkerSelected;
        _recommendationNotificationService.Changed += OnIncomingRecommendationChanged;
        _isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_isSubscribed)
        {
            return;
        }

        MapView.MarkerSelected -= OnMarkerSelected;
        _recommendationNotificationService.Changed -= OnIncomingRecommendationChanged;
        _isSubscribed = false;
    }

    private static double? CalculateDistanceKm(
        기사운송샘플항목? currentTransport,
        DriverMapMarkerItem recommendation)
    {
        if (!HasDropoffCoordinate(currentTransport))
        {
            return null;
        }

        var lat1 = DegreesToRadians((double)currentTransport!.하차위도!.Value);
        var lon1 = DegreesToRadians((double)currentTransport.하차경도!.Value);
        var lat2 = DegreesToRadians(recommendation.PickupLatitude);
        var lon2 = DegreesToRadians(recommendation.PickupLongitude);
        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;
        var a = Math.Pow(Math.Sin(deltaLat / 2d), 2d) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2d), 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));

        return 6371d * c;
    }

    private static bool HasDropoffCoordinate(기사운송샘플항목? currentTransport)
        => currentTransport?.하차위도 is not null &&
            currentTransport.하차경도 is not null &&
            currentTransport.하차위도 != 0m &&
            currentTransport.하차경도 != 0m;

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private async void OnOpenLegacyMenuClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    private async void OnOpenCurrentTransportClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }
}
