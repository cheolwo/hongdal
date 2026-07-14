using Hongdal.Contracts.Common.Community;

namespace Hongdal.Tests.Contracts.Common.Community;

public sealed class CommunityWorkClassificationCatalogTests
{
    [Fact]
    public void 업무_분류는_중복되지_않는_원장_종류와_기능_설정을_가진다()
    {
        var classifications = CommunityWorkClassificationCatalog.All;
        var ledgerTemplateKeys = classifications.SelectMany(x => x.LedgerTemplateKeys).ToArray();

        Assert.Equal(classifications.Count, classifications.Select(x => x.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ledgerTemplateKeys.Length, ledgerTemplateKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(classifications, x => Assert.False(string.IsNullOrWhiteSpace(x.FeatureFlagKey)));
    }

    [Fact]
    public void 화물_운송_분류는_운송_원장과_국내_운송_기능에_연결된다()
    {
        var classification = CommunityWorkClassificationCatalog.FindByWorkflowTag("국내 화물 운송");

        Assert.NotNull(classification);
        Assert.Equal("DomesticTransportWorkflow", classification.FeatureFlagKey);
        Assert.Equal([CommunityLedgerTemplateKeys.CargoTransport], classification.LedgerTemplateKeys);
    }
}
