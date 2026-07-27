using Ssalddel.Contracts.Common.DeliveryZones;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Services.DeliveryZones;
using 살뜰.도메인.배달권;

namespace Ssalddel.Tests.Architecture;

public sealed class PlatformDeliveryZoneMetadataTests
{
    [Fact]
    public void 플랫폼배달권은_공통계약에서_영속투영까지_탐색된다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.PlatformDeliveryZoneLedger,
            typeof(원장배달권연결요청).Assembly,
            typeof(플랫폼배달권).Assembly,
            typeof(원장배달권투영Service).Assembly);

        Assert.Contains(metadata, x => x.ComponentType == typeof(원장배달권연결요청));
        Assert.Contains(metadata, x => x.ComponentType == typeof(플랫폼배달권));
        Assert.Contains(metadata, x => x.ComponentType == typeof(원장배달권투영));
        var service = Assert.Single(metadata, x => x.ComponentType == typeof(원장배달권투영Service));
        var transportBridge = Assert.Single(metadata, x => x.ComponentType == typeof(운송원장배달권연결Service));
        Assert.Equal(typeof(I원장배달권투영Service), service.ContractType);
        Assert.Equal(typeof(I운송원장배달권연결Service), transportBridge.ContractType);
        Assert.True(service.Effects.HasFlag(SsalddelCodeEffect.PersistentRead));
        Assert.True(service.Effects.HasFlag(SsalddelCodeEffect.PersistentWrite));
        Assert.All(metadata, x => Assert.False(string.IsNullOrWhiteSpace(x.Boundary)));
    }
}
