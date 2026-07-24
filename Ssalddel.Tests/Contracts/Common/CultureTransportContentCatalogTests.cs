using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class CultureTransportContentCatalogTests
{
    [Fact]
    public void 문화교통콘텐츠는_0점0부터1점5안에서_문화근거수요이동준비를잇는다()
    {
        Assert.Equal("문화교통", CultureTransportContentCatalog.ProductName);
        Assert.Equal(
            [
                CultureTransportContentCatalog.CultureStoryKey,
                CultureTransportContentCatalog.PriceEvidenceKey,
                CultureTransportContentCatalog.SharedDemandKey,
                CultureTransportContentCatalog.RouteReadinessKey
            ],
            CultureTransportContentCatalog.All.Select(item => item.Key));
        Assert.All(
            CultureTransportContentCatalog.All,
            item => Assert.True(
                SsalddelProductRoadmapCatalog.IsCultureTransportVersion(item.Version)));
        Assert.All(
            CultureTransportContentCatalog.All,
            item =>
            {
                Assert.StartsWith("문화교통 · ", item.WorkflowTag, StringComparison.Ordinal);
                Assert.NotEmpty(item.RequiredEvidence);
                Assert.NotEmpty(item.PublicationBoundary);
            });
    }

    [Fact]
    public void 문화교통콘텐츠는_거래확정이아닌_근거와사람의확인을경계로둔다()
    {
        var combinedBoundary = string.Join(
            Environment.NewLine,
            CultureTransportContentCatalog.All.Select(item => item.PublicationBoundary));

        Assert.Contains("판매 권고", combinedBoundary, StringComparison.Ordinal);
        Assert.Contains("주문·결제·계약", combinedBoundary, StringComparison.Ordinal);
        Assert.Contains("자동 확정하지 않습니다", combinedBoundary, StringComparison.Ordinal);
    }
}
