namespace Ssalddel.Tests.Architecture;

public sealed class DriverNotificationPageCompositionTests
{
    [Fact]
    public void 알림_route는_실제_payload_연결만_담당하고_샘플_목록을_가장하지_않는다()
    {
        var root = FindRepositoryRoot();
        var pagePath = Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "DriverNotificationsPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 120);
        Assert.Contains("DriverNotificationDeepLinkResolver.ResolveHref", source);
        Assert.Contains("영속 알림 목록 API", source);
        Assert.Contains("DriverRoutes.NotificationSettings", source);
        Assert.DoesNotContain("NotificationItems", source);
        Assert.DoesNotContain("PayloadSamples", source);
        Assert.DoesNotContain("HD-WEB-001", source);
        Assert.DoesNotContain("DateTime.Now", source);
    }

    [Theory]
    [InlineData("DriverDispatchRecommendation", "REQ 20", null, "/driver/recommendations/REQ%2020")]
    [InlineData("DispatchRecommendation", "REQ 20", null, "/driver/recommendations/REQ%2020")]
    [InlineData("DispatchAccepted", "REQ 20", null, "/driver/transports/current?acceptedRequestId=REQ%2020")]
    [InlineData("TransportPickupReady", null, 31L, "/driver/transports/31/pickup")]
    [InlineData("TransportDropoffReady", null, 31L, "/driver/transports/31/dropoff")]
    [InlineData("SettlementReady", null, null, "/driver/settlements/current-month")]
    [InlineData("unknown", null, null, "/driver/notifications")]
    public void payload는_허용된_전용_route로만_연결한다(
        string payloadType,
        string? requestId,
        long? transportId,
        string expected)
    {
        var href = Ssalddel.WebApp.Services.DriverNotificationDeepLinkResolver.ResolveHref(
            payloadType,
            requestId,
            transportId);

        Assert.Equal(expected, href);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
