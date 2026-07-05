#if !ANDROID
using DriverApp.Controls;
using Microsoft.Maui.Handlers;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NativeGrid = Microsoft.UI.Xaml.Controls.Grid;
using NativeBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using NativeColors = Microsoft.UI.Colors;
using NativeHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using NativeVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
#elif IOS || MACCATALYST
using UIKit;
#endif

namespace DriverApp.Handlers;

#if WINDOWS
public partial class DriverNativeMapViewHandler : ViewHandler<DriverNativeMapView, NativeGrid>
{
    protected override NativeGrid CreatePlatformView()
    {
        return new NativeGrid
        {
            Background = new NativeBrush(NativeColors.LightGray),
            Children =
            {
                new TextBlock
                {
                    Text = "네이티브 지도는 Android 기사 앱에서 Naver SDK로 렌더링됩니다.",
                    HorizontalAlignment = NativeHorizontalAlignment.Center,
                    VerticalAlignment = NativeVerticalAlignment.Center,
                    Foreground = new NativeBrush(NativeColors.Black)
                }
            }
        };
    }

    public static void MapCamera(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }

    public static void MapMarkers(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }
}
#elif IOS || MACCATALYST
public partial class DriverNativeMapViewHandler : ViewHandler<DriverNativeMapView, UIView>
{
    protected override UIView CreatePlatformView() => new()
    {
        BackgroundColor = UIColor.LightGray
    };

    public static void MapCamera(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }

    public static void MapMarkers(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }
}
#else
public partial class DriverNativeMapViewHandler : ViewHandler<DriverNativeMapView, object>
{
    protected override object CreatePlatformView() => new();

    public static void MapCamera(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }

    public static void MapMarkers(DriverNativeMapViewHandler handler, DriverNativeMapView view)
    {
    }
}
#endif
#endif
