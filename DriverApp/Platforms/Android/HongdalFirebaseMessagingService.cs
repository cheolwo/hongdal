using Android.App;
using Android.Content;
using Android.Util;
using DriverApp.Services;
using Firebase.Messaging;
using Hongdal.Contracts.Common.Drivers;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS0618, CS0672

namespace DriverApp.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
public sealed class HongdalFirebaseMessagingService : FirebaseMessagingService
{
    private const string Tag = "HongdalFcm";

    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        _ = RegisterTokenAsync(token);
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);
        _ = HandleMessageAsync(message.Data);
    }

    public static Task RegisterTokenAsync(string? token)
        => RunWithServicesAsync(async services =>
        {
            var tokenService = services.GetRequiredService<I기사푸시토큰등록Service>();
            await tokenService.수신토큰저장및등록Async(token);
        });

    private static Task HandleMessageAsync(IDictionary<string, string> data)
        => RunWithServicesAsync(async services =>
        {
            if (!IsDispatchRecommendation(data))
            {
                return;
            }

            var requestId = ResolveRequestId(data);
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            var sampleData = services.GetRequiredService<IDriverSampleDataService>();
            var mapService = services.GetRequiredService<IDriverHomeMapService>();
            var notificationService = services.GetRequiredService<IDriverRecommendationNotificationService>();

            await sampleData.RefreshAsync();
            var request = sampleData.추천의뢰조회(requestId)
                ?? sampleData.추천의뢰목록.FirstOrDefault();
            if (request is null)
            {
                return;
            }

            var marker = mapService.BuildMarkers([request]).FirstOrDefault();
            if (marker is null)
            {
                marker = new DriverMapMarkerItem(
                    request.의뢰Id,
                    0d,
                    0d,
                    0d,
                    0d,
                    request.화물종류,
                    request.요약설명,
                    request.픽업지);
            }

            var pendingCount = ResolvePendingCount(data);
            notificationService.Publish(new DriverIncomingRecommendation(
                marker,
                request,
                DateTime.Now,
                "FCM",
                pendingCount));
        });

    private static async Task RunWithServicesAsync(Func<IServiceProvider, Task> action)
    {
        try
        {
            await action(DriverAppServiceProvider.Services);
        }
        catch (Exception ex)
        {
            Log.Warn(Tag, ex.ToString());
        }
    }

    private static bool IsDispatchRecommendation(IDictionary<string, string> data)
        => data.TryGetValue("type", out var type)
           && string.Equals(type, "DriverDispatchRecommendation", StringComparison.OrdinalIgnoreCase);

    private static string ResolveRequestId(IDictionary<string, string> data)
    {
        if (data.TryGetValue("topRequestId", out var topRequestId)
            && !string.IsNullOrWhiteSpace(topRequestId))
        {
            return topRequestId;
        }

        if (!data.TryGetValue("requestIds", out var requestIds)
            || string.IsNullOrWhiteSpace(requestIds))
        {
            return string.Empty;
        }

        return requestIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? string.Empty;
    }

    private static int ResolvePendingCount(IDictionary<string, string> data)
        => data.TryGetValue("recommendationCount", out var countText)
           && int.TryParse(countText, out var count)
            ? count
            : 1;
}

#pragma warning restore CS0618, CS0672
