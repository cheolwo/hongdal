#if ANDROID
using Android.OS;
using Android.Views;
using Com.Naver.Maps.Geometry;
using Com.Naver.Maps.Map;
using Com.Naver.Maps.Map.Overlay;
using DriverApp.Controls;
using DriverApp.Models.Driver.Map;
using Microsoft.Maui.Handlers;
using AndroidColor = Android.Graphics.Color;

namespace DriverApp.Handlers;

public partial class DriverNativeMapViewHandler : ViewHandler<DriverNativeMapView, MapView>
{
    private NaverMap? _naverMap;
    private readonly List<Marker> _nativeMarkers = [];
    private readonly List<PathOverlay> _nativeRouteOverlays = [];

    protected override MapView CreatePlatformView()
    {
        var context = MauiContext?.Context ?? throw new InvalidOperationException("Android context is not available.");
        var mapView = new MapView(context);
        mapView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        mapView.OnCreate((Bundle?)null);
        mapView.OnStart();
        mapView.OnResume();
        mapView.GetMapAsync(new MapReadyCallback(this));
        return mapView;
    }

    protected override void DisconnectHandler(MapView platformView)
    {
        ClearMarkers();
        platformView.OnPause();
        platformView.OnStop();
        platformView.OnDestroy();
        base.DisconnectHandler(platformView);
    }

    private void OnMapReady(NaverMap naverMap)
    {
        _naverMap = naverMap;
        ApplyMapOptions();
        ApplyCamera();
        ApplyMarkers();
        ApplyRouteOverlays();
    }

    public static void MapCamera(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyCamera();
    }

    public static void MapMarkers(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyMarkers();
    }

    public static void MapRouteOverlays(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyRouteOverlays();
    }

    public static void MapOptions(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyMapOptions();
    }

    private void ApplyMapOptions()
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        _naverMap.MinZoom = VirtualView.MinZoom;
        _naverMap.MaxZoom = VirtualView.MaxZoom;
        _naverMap.LiteModeEnabled = false;
        _naverMap.SetLayerGroupEnabled(NaverMap.LayerGroupTraffic, VirtualView.ShowTrafficLayer);

        var uiSettings = _naverMap.UiSettings;
        uiSettings.CompassEnabled = true;
        uiSettings.ScaleBarEnabled = true;
        uiSettings.ZoomControlEnabled = true;
        uiSettings.LocationButtonEnabled = VirtualView.ShowLocationButton;
        uiSettings.SetLogoMargin(16, 16, 16, 120);

        ApplyLocationOverlay();
    }

    private void ApplyCamera()
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        var target = new LatLng(VirtualView.CenterLatitude, VirtualView.CenterLongitude);
        var update = CameraUpdate.ScrollAndZoomTo(target, VirtualView.Zoom);
        _naverMap.MoveCamera(update);
        ApplyLocationOverlay();
    }

    private void ApplyMarkers()
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        ClearMarkers();
        foreach (var item in VirtualView.Markers)
        {
            AddMarker(item, item.PickupLatitude, item.PickupLongitude, $"{item.Title} 상차");
            if (item.DropoffLatitude != 0d && item.DropoffLongitude != 0d)
            {
                AddMarker(item, item.DropoffLatitude, item.DropoffLongitude, $"{item.Title} 하차");
            }
        }
    }

    private void ApplyRouteOverlays()
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        ClearRouteOverlays();
        foreach (var item in VirtualView.RouteOverlays)
        {
            if (item.Points.Count < 2)
            {
                continue;
            }

            var coords = item.Points
                .Select(x => new LatLng(x.Latitude, x.Longitude))
                .ToList();
            var overlay = new PathOverlay
            {
                Coords = coords,
                Width = item.Width,
                Color = ParseColor(item.StrokeColor, AndroidColor.Rgb(37, 99, 235)),
                OutlineColor = ParseColor(item.OutlineColor, AndroidColor.White)
            };

            overlay.Map = _naverMap;
            _nativeRouteOverlays.Add(overlay);
        }
    }

    private void ApplyLocationOverlay()
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        var overlay = _naverMap.LocationOverlay;
        overlay.Position = new LatLng(VirtualView.CenterLatitude, VirtualView.CenterLongitude);
        overlay.CircleColor = AndroidColor.Argb(40, 25, 118, 210);
        overlay.CircleOutlineColor = AndroidColor.Argb(120, 25, 118, 210);
        overlay.CircleOutlineWidth = 2;
        overlay.Visible = VirtualView.ShowCurrentLocationOverlay;

        _naverMap.LocationTrackingMode = VirtualView.ShowCurrentLocationOverlay
            ? LocationTrackingMode.NoFollow!
            : LocationTrackingMode.None!;
    }

    private void AddMarker(DriverMapMarkerItem item, double latitude, double longitude, string caption)
    {
        if (_naverMap is null || VirtualView is null)
        {
            return;
        }

        var marker = new Marker
        {
            Position = new LatLng(latitude, longitude),
            CaptionText = caption,
            SubCaptionText = item.Summary
        };

        marker.Click += (_, _) =>
        {
            VirtualView.SendMarkerSelected(item);
        };
        marker.Map = _naverMap;
        _nativeMarkers.Add(marker);
    }

    private void ClearMarkers()
    {
        foreach (var marker in _nativeMarkers)
        {
            marker.Map = null;
            marker.Dispose();
        }

        _nativeMarkers.Clear();
    }

    private void ClearRouteOverlays()
    {
        foreach (var overlay in _nativeRouteOverlays)
        {
            overlay.Map = null;
            overlay.Dispose();
        }

        _nativeRouteOverlays.Clear();
    }

    private static AndroidColor ParseColor(string value, AndroidColor fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        try
        {
            return AndroidColor.ParseColor(value);
        }
        catch (ArgumentException)
        {
            return fallback;
        }
    }

    private sealed class MapReadyCallback(DriverNativeMapViewHandler handler) : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(NaverMap naverMap)
        {
            handler.OnMapReady(naverMap);
        }
    }
}
#endif
