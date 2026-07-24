using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Driver.Action03;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Controllers.Shipper.Request01;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.ApiMetadata;

public sealed class SsalddelApiClassificationTests
{
    [Fact]
    public void RepresentativeAppControllers_ExposeKoreanBusinessMeaning()
    {
        AssertClassification(
            typeof(인연연결Controller),
            "인연 형성",
            "커뮤니티 참여자",
            "요청하기");
        AssertClassification(
            typeof(공동구매자동집단화Controller),
            "공동구매 수요·모집",
            "주문자",
            "판단하기");
        AssertClassification(
            typeof(화주운송의뢰Controller),
            "운송 의뢰",
            "화주",
            "요청하기");
        AssertClassification(
            typeof(기사배차액션Controller),
            "배차",
            "기사",
            "판단하기");
        AssertClassification(
            typeof(WarehouseOperationsController),
            "창고 이행",
            "창고 관리자",
            "실행하기");
    }

    [Fact]
    public void ExecutionFeature_IsSeparatedFromIntroductionHistory()
    {
        var controllerType = typeof(화주운송의뢰Controller);
        var introducedIn = controllerType.GetCustomAttribute<SsalddelApiIntroducedInAttribute>();
        var feature = controllerType.GetCustomAttribute<SsalddelApiFeatureAttribute>();

        Assert.Equal("2.0", introducedIn?.VersionLabel);
        Assert.True(string.IsNullOrWhiteSpace(introducedIn?.FeatureKey));
        Assert.Equal(VersionFeatureFlagKeys.DomesticTransportWorkflow, feature?.FeatureKey);
    }

    [Fact]
    public void WorkRelationshipApi_ExplainsWorkSignalToCommunityRelationshipFlow()
    {
        var metadata = SsalddelApiClassificationReader.Read(
            typeof(WorkRelationshipSnapshotsController));

        Assert.Contains("업무 활동 신호", metadata.Capabilities);
        Assert.Contains("인연 형성", metadata.Capabilities);
        Assert.Contains("둘러보기", metadata.Operations);
        Assert.Equal("커뮤니티 신뢰", metadata.Workflow);
        Assert.Equal("0.0", metadata.IntroducedIn);
        Assert.Null(metadata.FeatureKey);
    }

    private static void AssertClassification(
        Type controllerType,
        string capability,
        string audience,
        string operation)
    {
        var metadata = SsalddelApiClassificationReader.Read(controllerType);

        Assert.Contains(capability, metadata.Capabilities);
        Assert.Contains(audience, metadata.Audiences);
        Assert.Contains(operation, metadata.Operations);
        Assert.False(string.IsNullOrWhiteSpace(metadata.IntroducedIn));
    }
}
