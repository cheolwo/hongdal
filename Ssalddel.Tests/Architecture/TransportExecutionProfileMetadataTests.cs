using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Transport;
using 살뜰.Services.Dispatch.Engine;

namespace Ssalddel.Tests.Architecture;

public sealed class TransportExecutionProfileMetadataTests
{
    [Fact]
    public void 운송실행프로필은_공통계약과_순수분류경계로탐색된다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.TransportExecutionProfile,
            typeof(운송실행프로필Dto).Assembly,
            typeof(운송실행프로필Factory).Assembly);

        var contract = Assert.Single(metadata, x => x.ComponentType == typeof(운송실행프로필Dto));
        var factory = Assert.Single(metadata, x => x.ComponentType == typeof(운송실행프로필Factory));

        Assert.Equal(SsalddelCodeLayer.Contract, contract.Layer);
        Assert.Equal(SsalddelCodeLayer.Domain, factory.Layer);
        Assert.Equal(SsalddelCodeEffect.None, contract.Effects);
        Assert.Equal(SsalddelCodeEffect.None, factory.Effects);
        Assert.All(metadata, x => Assert.False(string.IsNullOrWhiteSpace(x.Boundary)));
    }
}
