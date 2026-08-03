using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Controllers.Common;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class HongikAcademyContentMapLayerQueryTests
{
    [Fact]
    public async Task 검증지리기록이없으면_명시적빈레이어와원천한계를반환한다()
    {
        var useCase = new 홍익학당철학영상MapLayer조회UseCase(
            new FakeTimeProvider(new DateTimeOffset(2026, 8, 2, 12, 0, 0, TimeSpan.Zero)));

        var response = await useCase.조회Async();

        Assert.Equal(HongikAcademyContentMapLayerKeys.PhilosophyVideo, response.LayerKey);
        Assert.False(response.HasVerifiedGeographicRecords);
        Assert.Equal(0, response.VerifiedGeographicRecordCount);
        Assert.Equal(HongikAcademyContentMapProvenanceStatusCodes.NoVerifiedGeographicRecords,
            response.GeographicScopeCode);
        var source = Assert.Single(response.Sources);
        Assert.Equal("community-prajna-publication-policy", source.SourceKey);
        Assert.Equal(0, source.VerifiedGeographicRecordCount);
        Assert.Null(source.SourceUrl);
        Assert.Contains(response.Notices, notice => notice.Contains("개인 주소", StringComparison.Ordinal));
    }

    [Fact]
    public void 공개Controller는_읽기전용홍익학당레이어Route를노출한다()
    {
        var controller = typeof(홍익학당철학영상MapController);

        Assert.NotNull(controller.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(HongikAcademyContentMapRoutes.LayerApi,
            controller.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Null(controller.GetMethod(nameof(홍익학당철학영상MapController.레이어조회))
            ?.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
