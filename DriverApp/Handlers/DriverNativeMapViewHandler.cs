using DriverApp.Controls;
using Microsoft.Maui.Handlers;

namespace DriverApp.Handlers;

public partial class DriverNativeMapViewHandler
{
    public static readonly IPropertyMapper<DriverNativeMapView, DriverNativeMapViewHandler> Mapper =
        new PropertyMapper<DriverNativeMapView, DriverNativeMapViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(DriverNativeMapView.CenterLatitude)] = MapCamera,
            [nameof(DriverNativeMapView.CenterLongitude)] = MapCamera,
            [nameof(DriverNativeMapView.Zoom)] = MapCamera,
            [nameof(DriverNativeMapView.Markers)] = MapMarkers,
            [nameof(DriverNativeMapView.ShowTrafficLayer)] = MapOptions,
            [nameof(DriverNativeMapView.ShowLocationButton)] = MapOptions,
            [nameof(DriverNativeMapView.ShowCurrentLocationOverlay)] = MapOptions,
            [nameof(DriverNativeMapView.MinZoom)] = MapOptions,
            [nameof(DriverNativeMapView.MaxZoom)] = MapOptions
        };

    public DriverNativeMapViewHandler() : base(Mapper)
    {
    }
}
