using DriverApp.Models.Driver.Map;

namespace DriverApp.Controls;

public sealed class DriverNativeMapView : View
{
    public static readonly BindableProperty CenterLatitudeProperty = BindableProperty.Create(
        nameof(CenterLatitude),
        typeof(double),
        typeof(DriverNativeMapView),
        37.5665d);

    public static readonly BindableProperty CenterLongitudeProperty = BindableProperty.Create(
        nameof(CenterLongitude),
        typeof(double),
        typeof(DriverNativeMapView),
        126.9780d);

    public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
        nameof(Zoom),
        typeof(double),
        typeof(DriverNativeMapView),
        11d);

    public static readonly BindableProperty MarkersProperty = BindableProperty.Create(
        nameof(Markers),
        typeof(IReadOnlyList<DriverMapMarkerItem>),
        typeof(DriverNativeMapView),
        Array.Empty<DriverMapMarkerItem>());

    public double CenterLatitude
    {
        get => (double)GetValue(CenterLatitudeProperty);
        set => SetValue(CenterLatitudeProperty, value);
    }

    public double CenterLongitude
    {
        get => (double)GetValue(CenterLongitudeProperty);
        set => SetValue(CenterLongitudeProperty, value);
    }

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public IReadOnlyList<DriverMapMarkerItem> Markers
    {
        get => (IReadOnlyList<DriverMapMarkerItem>)GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    public event EventHandler<DriverMapMarkerItem>? MarkerSelected;

    public void SendMarkerSelected(DriverMapMarkerItem marker)
    {
        MarkerSelected?.Invoke(this, marker);
    }
}
