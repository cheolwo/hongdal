using DriverApp.Models.Driver.Map;
using DriverApp.Services;

namespace DriverApp;

public partial class NativeDriverHomePage : ContentPage
{
    private readonly IDriverSampleDataService _sampleDataService;
    private readonly IDriverHomeMapService _mapService;

    public NativeDriverHomePage(IDriverSampleDataService sampleDataService, IDriverHomeMapService mapService)
    {
        InitializeComponent();
        _sampleDataService = sampleDataService;
        _mapService = mapService;
        MapView.MarkerSelected += OnMarkerSelected;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var currentLocation = _sampleDataService.기사현재위치;
        var markers = _mapService.BuildMarkers(_sampleDataService.추천의뢰목록);

        MapView.CenterLatitude = (double)currentLocation.위도;
        MapView.CenterLongitude = (double)currentLocation.경도;
        MapView.Zoom = 11d;
        MapView.Markers = markers;

        StatusLabel.Text = $"{currentLocation.위치명} 기준 추천 운송 {markers.Count}건을 네이티브 지도에 표시합니다.";
    }

    protected override void OnDisappearing()
    {
        MapView.MarkerSelected -= OnMarkerSelected;
        base.OnDisappearing();
    }

    private void OnMarkerSelected(object? sender, DriverMapMarkerItem marker)
    {
        SelectedRequestLabel.Text = marker.Title;
        SelectedRequestSummaryLabel.Text = $"{marker.RequestId} · {marker.Summary} · {marker.PickupAddress}";
    }

    private async void OnOpenLegacyMenuClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }
}
