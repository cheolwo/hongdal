#if !ANDROID
using FDriverApp.Controls;
using Microsoft.Maui.Handlers;
#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using NativeBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using NativeColors = Microsoft.UI.Colors;
using NativeGrid = Microsoft.UI.Xaml.Controls.Grid;
using NativeHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using NativeVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
#elif IOS || MACCATALYST
using UIKit;
#endif

namespace FDriverApp.Handlers;

#if WINDOWS
public partial class FDriverNativeMapViewHandler : ViewHandler<FDriverNativeMapView, NativeGrid>
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
                    Text = "네이티브 지도는 Android F 드라이버 앱에서 Naver SDK로 렌더링됩니다.",
                    HorizontalAlignment = NativeHorizontalAlignment.Center,
                    VerticalAlignment = NativeVerticalAlignment.Center,
                    Foreground = new NativeBrush(NativeColors.Black)
                }
            }
        };
    }

    public static void MapCamera(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapMarkers(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapRouteOverlays(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapOptions(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }
}
#elif IOS || MACCATALYST
public partial class FDriverNativeMapViewHandler : ViewHandler<FDriverNativeMapView, UIView>
{
    protected override UIView CreatePlatformView() => new()
    {
        BackgroundColor = UIColor.LightGray
    };

    public static void MapCamera(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapMarkers(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapRouteOverlays(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapOptions(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }
}
#else
public partial class FDriverNativeMapViewHandler : ViewHandler<FDriverNativeMapView, object>
{
    protected override object CreatePlatformView() => new();

    public static void MapCamera(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapMarkers(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapRouteOverlays(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }

    public static void MapOptions(FDriverNativeMapViewHandler handler, FDriverNativeMapView view)
    {
    }
}
#endif
#endif
