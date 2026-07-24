using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Shipper.ImportFood;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Shipper;
using Ssalddel.Services.External.Apify;
using 살뜰.Services.External.Mfds;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Architecture;

public sealed class ExternalIntegrationFeatureMetadataTests
{
    [Fact]
    public void ApifyGateway_비용발생외부호출Feature흐름을복구한다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.ApifyActorIntegration,
            typeof(ApifyActorGateway).Assembly);

        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(IApifyActorGateway)
            && item.Layer == SsalddelCodeLayer.ExternalAdapter);
        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(ApifyActorGateway)
            && item.Effects.HasFlag(SsalddelCodeEffect.MayIncurExternalCost)
            && item.Effects.HasFlag(SsalddelCodeEffect.ThirdPartyApiCall));
    }

    [Fact]
    public void 한글표시사항_Controller부터외부Adapter까지Feature흐름을복구한다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.ImportedFoodKoreanLabelIntegration,
            typeof(수입식품한글표시사항Controller).Assembly);

        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(수입식품한글표시사항Controller)
            && item.Layer == SsalddelCodeLayer.Api);
        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(수입식품한글표시사항조회QueryHandler)
            && item.Layer == SsalddelCodeLayer.Application);
        Assert.Contains(metadata, item =>
            item.ComponentType == typeof(수입식품한글표시사항조회Service)
            && item.Layer == SsalddelCodeLayer.ExternalAdapter);

        var version = typeof(수입식품한글표시사항Controller)
            .GetCustomAttribute<SsalddelApiVersionAttribute>();
        Assert.NotNull(version);
        Assert.Equal(SsalddelProductVersion.V1_5, version!.Version);
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, version.FeatureKey);
    }
}
