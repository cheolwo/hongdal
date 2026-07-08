using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

#pragma warning disable CA1416

namespace DriverApp.Services;

public sealed class Android기사위치송신Service : I기사위치송신Service
{
    public bool IsRunning { get; private set; }
    public event Action? Changed;

    public Task StartAsync(기사위치송신시작요청 request, CancellationToken cancellationToken = default)
    {
        var context = Platform.AppContext;
        var intent = new Intent(context, typeof(DriverLocationForegroundService));
        intent.SetAction(DriverLocationForegroundService.ActionStart);
        intent.PutExtra(DriverLocationForegroundService.ExtraIntervalSeconds, request.권장위치전송간격초);
        intent.PutExtra(DriverLocationForegroundService.ExtraPickupApproachRadiusKm, (double)request.상차접근허용반경Km);
        intent.PutExtra(DriverLocationForegroundService.ExtraDrivingStatus, request.운행상태);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(intent);
        }
        else
        {
            context.StartService(intent);
        }

        IsRunning = true;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        var context = Platform.AppContext;
        var intent = new Intent(context, typeof(DriverLocationForegroundService));
        intent.SetAction(DriverLocationForegroundService.ActionStop);
        context.StartService(intent);

        IsRunning = false;
        Changed?.Invoke();
        return Task.CompletedTask;
    }
}

#pragma warning restore CA1416
