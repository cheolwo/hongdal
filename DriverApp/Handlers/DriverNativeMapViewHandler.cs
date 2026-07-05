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
            [nameof(DriverNativeMapView.Markers)] = MapMarkers
        };

    public DriverNativeMapViewHandler() : base(Mapper)
    {
    }
}
