using FDriverApp.Controls;
using Microsoft.Maui.Handlers;

namespace FDriverApp.Handlers;

public partial class FDriverNativeMapViewHandler
{
    public static readonly IPropertyMapper<FDriverNativeMapView, FDriverNativeMapViewHandler> Mapper =
        new PropertyMapper<FDriverNativeMapView, FDriverNativeMapViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(FDriverNativeMapView.CenterLatitude)] = MapCamera,
            [nameof(FDriverNativeMapView.CenterLongitude)] = MapCamera,
            [nameof(FDriverNativeMapView.Zoom)] = MapCamera,
            [nameof(FDriverNativeMapView.Markers)] = MapMarkers,
            [nameof(FDriverNativeMapView.RouteOverlays)] = MapRouteOverlays,
            [nameof(FDriverNativeMapView.ShowTrafficLayer)] = MapOptions,
            [nameof(FDriverNativeMapView.ShowLocationButton)] = MapOptions,
            [nameof(FDriverNativeMapView.ShowCurrentLocationOverlay)] = MapOptions,
            [nameof(FDriverNativeMapView.MinZoom)] = MapOptions,
            [nameof(FDriverNativeMapView.MaxZoom)] = MapOptions
        };

    public FDriverNativeMapViewHandler() : base(Mapper)
    {
    }
}
