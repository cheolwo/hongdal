#if ANDROID
using Android.Content;
using Android.OS;
using Android.Views;
using Com.Naver.Maps.Geometry;
using Com.Naver.Maps.Map;
using Com.Naver.Maps.Map.Overlay;
using DriverApp.Controls;
using DriverApp.Models.Driver.Map;
using Microsoft.Maui.Handlers;

namespace DriverApp.Handlers;

public partial class DriverNativeMapViewHandler : ViewHandler<DriverNativeMapView, MapView>
{
    private NaverMap? _naverMap;
    private readonly List<Marker> _nativeMarkers = [];

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
        ApplyCamera();
        ApplyMarkers();
    }

    public static void MapCamera(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyCamera();
    }

    public static void MapMarkers(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
        handler.ApplyMarkers();
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

    private sealed class MapReadyCallback(DriverNativeMapViewHandler handler) : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(NaverMap naverMap)
        {
            handler.OnMapReady(naverMap);
        }
    }
}
#endif
