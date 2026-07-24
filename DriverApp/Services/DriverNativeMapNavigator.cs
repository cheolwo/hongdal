using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace DriverApp.Services;

public sealed class DriverNativeMapNavigator(IServiceProvider services) : IDriverNativeMapNavigator
{
    public Task OpenAsync()
        => MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (rootPage is not NavigationPage navigation)
            {
                throw new InvalidOperationException("기사 앱 NavigationPage를 찾지 못했습니다.");
            }

            if (navigation.CurrentPage is NativeDriverHomePage)
            {
                return;
            }

            var mapPage = services.GetRequiredService<NativeDriverHomePage>();
            await navigation.PushAsync(mapPage);
        });
}
