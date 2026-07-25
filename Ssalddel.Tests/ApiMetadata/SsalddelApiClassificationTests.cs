using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Admin;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Controllers.Admin.Dispatch;
using Ssalddel.Controllers.Admin.HumanResources;
using Ssalddel.Controllers.Admin.Master06;
using Ssalddel.Controllers.Admin.Orderer;
using Ssalddel.Controllers.Admin.Settlement;
using Ssalddel.Controllers.Admin.TraditionalMarkets;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Driver.Action03;
using Ssalddel.Controllers.Driver.Food;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Controllers.Platform;
using Ssalddel.Controllers.Shipper.Request01;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
            typeof(창고작업Controller),
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
            typeof(업무관계SnapshotController));

        Assert.Contains("업무 활동 신호", metadata.Capabilities);
        Assert.Contains("인연 형성", metadata.Capabilities);
        Assert.Contains("둘러보기", metadata.Operations);
        Assert.Equal("커뮤니티 신뢰", metadata.Workflow);
        Assert.Equal("0.0", metadata.IntroducedIn);
        Assert.Null(metadata.FeatureKey);
    }

    [Theory]
    [InlineData("Ssalddel.Controllers.Orderer", "주문자")]
    [InlineData("Ssalddel.Controllers.Shipper", "화주")]
    [InlineData("Ssalddel.Controllers.Driver", "기사")]
    public void RoleAppControllers_HaveAudienceAndBusinessCapability(
        string namespacePrefix,
        string audience)
    {
        var controllers = GetConcreteControllers()
            .Where(type => type.Namespace?.StartsWith(namespacePrefix, StringComparison.Ordinal) == true)
            .ToArray();

        Assert.NotEmpty(controllers);
        Assert.All(controllers, controller =>
        {
            var metadata = SsalddelApiClassificationReader.Read(controller);
            Assert.Contains(audience, metadata.Audiences);
            Assert.True(
                metadata.Capabilities.Count > 0,
                $"{controller.FullName}에 한국어 업무 영역이 없습니다.");
        });
    }

    [Fact]
    public void CommunityAndWarehouseEntryPoints_InheritTheirBusinessAudience()
    {
        var community = SsalddelApiClassificationReader.Read(typeof(커뮤니티게시글Controller));
        var warehouseTypes = new[]
        {
            typeof(창고작업Controller),
            typeof(창고업무관점조회Controller),
            typeof(상차업무관점조회Controller),
            typeof(하차업무관점조회Controller)
        };

        Assert.Contains("커뮤니티 참여자", community.Audiences);
        Assert.Contains("커뮤니티 정보 둘러보기", community.Capabilities);
        Assert.All(warehouseTypes, controller =>
        {
            var metadata = SsalddelApiClassificationReader.Read(controller);
            Assert.Contains("창고 관리자", metadata.Audiences);
            Assert.Contains("창고 이행", metadata.Capabilities);
        });
    }

    [Fact]
    public void CommunityFacingApiControllers_AreIncludedInCommonBoundary()
    {
        var controllers = GetConcreteControllers()
            .Where(type =>
                type.GetCustomAttributes<SsalddelApiGrowthTrackAttribute>(inherit: true)
                    .Any(attribute => attribute.Track == SsalddelApiGrowthTrack.Community))
            .Where(type =>
                type.Namespace?.StartsWith("Ssalddel.Controllers.Admin", StringComparison.Ordinal) != true)
            .ToArray();

        Assert.NotEmpty(controllers);
        Assert.Contains(typeof(커뮤니티게시글Controller), controllers);
        Assert.Contains(typeof(커뮤니티원장공유Controller), controllers);
        Assert.Contains(typeof(인연연결Controller), controllers);
        Assert.All(controllers, controller =>
        {
            Assert.Equal("Ssalddel.Controllers.Common", controller.Namespace);
        });
    }

    [Fact]
    public void CommonBoundary_SeparatesSharedBusinessFromPlatformTechnicalApis()
    {
        var sharedBusinessControllers = new[]
        {
            typeof(커뮤니티게시글Controller),
            typeof(커뮤니티원장공유Controller),
            typeof(업무관계SnapshotController),
            typeof(주문원장Controller)
        };
        var platformControllers = new[]
        {
            typeof(VersionFeatureFlagsController),
            typeof(MobilePushInstallationsController),
            typeof(SampleImagesController),
            typeof(KieAi콜백Controller),
            typeof(공공데이터ApiMetadataController),
            typeof(공개LocaleController),
            typeof(파일업로드Controller)
        };

        Assert.All(sharedBusinessControllers, controller =>
            Assert.Equal("Ssalddel.Controllers.Common", controller.Namespace));
        Assert.All(platformControllers, controller =>
            Assert.Equal("Ssalddel.Controllers.Platform", controller.Namespace));
    }

    [Theory]
    [InlineData(typeof(국내공동구매협의Controller), "국내공동구매협의Controller")]
    [InlineData(typeof(국내공동구매생산자연결Controller), "국내공동구매생산자연결Controller")]
    [InlineData(typeof(국내공동구매이행계획Controller), "국내공동구매이행계획Controller")]
    [InlineData(typeof(국내공동구매차량추천Controller), "국내공동구매차량추천Controller")]
    [InlineData(typeof(공공데이터조회Controller), "공공데이터조회Controller")]
    [InlineData(typeof(음식점탐색공개정책Controller), "음식점탐색공개정책Controller")]
    [InlineData(typeof(음식배달기사업무Controller), "음식배달기사업무Controller")]
    [InlineData(typeof(창고작업Controller), "창고작업Controller")]
    [InlineData(typeof(창고업무관점조회Controller), "창고업무관점조회Controller")]
    [InlineData(typeof(상차업무관점조회Controller), "상차업무관점조회Controller")]
    [InlineData(typeof(하차업무관점조회Controller), "하차업무관점조회Controller")]
    public void Controller_DomainNameIsKoreanAndTechnicalSuffixIsEnglish(
        Type controllerType,
        string expectedName)
    {
        Assert.Equal(expectedName, controllerType.Name);
        Assert.EndsWith("Controller", controllerType.Name, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(typeof(국내공동구매협의Controller), "DomesticGroupPurchaseNegotiationsController")]
    [InlineData(typeof(국내공동구매생산자연결Controller), "DomesticGroupPurchaseProducerConnectionsController")]
    [InlineData(typeof(국내공동구매이행계획Controller), "DomesticGroupPurchaseFulfillmentPlansController")]
    [InlineData(typeof(국내공동구매차량추천Controller), "DomesticGroupPurchaseVehicleRecommendationsController")]
    [InlineData(typeof(공공데이터조회Controller), "PublicDataLookupController")]
    [InlineData(typeof(음식점탐색공개정책Controller), "RestaurantSearchPolicyPublicController")]
    [InlineData(typeof(음식배달기사업무Controller), "FoodDeliveryDriverController")]
    [InlineData(typeof(창고작업Controller), "WarehouseOperationsController")]
    [InlineData(typeof(창고업무관점조회Controller), "WarehousePerspectiveReadController")]
    [InlineData(typeof(상차업무관점조회Controller), "LoadingPerspectiveReadController")]
    [InlineData(typeof(하차업무관점조회Controller), "UnloadingPerspectiveReadController")]
    public void RenamedController_PreservesExistingApiMetadataContractName(
        Type controllerType,
        string expectedContractName)
    {
        var contractName = controllerType.GetCustomAttribute<SsalddelApiContractNameAttribute>();

        Assert.NotNull(contractName);
        Assert.Equal(expectedContractName, contractName.Name);
    }

    [Fact]
    public void RoleAppControllerActions_UseKoreanDomainNames()
    {
        var roleAppNamespacePrefixes = new[]
        {
            "Ssalddel.Controllers.Orderer",
            "Ssalddel.Controllers.Shipper",
            "Ssalddel.Controllers.Driver"
        };

        var actions = GetConcreteControllers()
            .Where(type => roleAppNamespacePrefixes.Any(prefix =>
                type.Namespace?.StartsWith(prefix, StringComparison.Ordinal) == true))
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any())
            .ToArray();

        Assert.NotEmpty(actions);
        Assert.All(actions, action =>
        {
            Assert.False(
                action.Name[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z',
                $"{action.DeclaringType?.Name}.{action.Name}의 업무 동작 이름을 한국어로 바꿔야 합니다.");
        });
    }

    [Fact]
    public void WarehouseControllerActions_UseKoreanDomainNames()
    {
        var warehouseControllers = new[]
        {
            typeof(창고작업Controller),
            typeof(창고업무관점조회Controller),
            typeof(상차업무관점조회Controller),
            typeof(하차업무관점조회Controller)
        };

        var actions = warehouseControllers
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any())
            .ToArray();

        Assert.NotEmpty(actions);
        Assert.All(actions, action =>
        {
            Assert.False(
                action.Name[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z',
                $"{action.DeclaringType?.Name}.{action.Name}의 업무 동작 이름을 한국어로 바꿔야 합니다.");
        });
    }

    [Fact]
    public void RenamedDomainControllers_PreserveContractAndUseKoreanDomainNames()
    {
        var controllers = GetRenamedCommonControllers()
            .Concat(GetRenamedAdminControllers())
            .Concat(GetRenamedPlatformControllers())
            .ToArray();

        Assert.NotEmpty(controllers);
        Assert.All(controllers, controller =>
        {
            Assert.False(
                controller.Name[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z',
                $"{controller.Name}의 업무 이름을 한국어로 바꿔야 합니다.");

            var contractName = controller.GetCustomAttribute<SsalddelApiContractNameAttribute>();
            Assert.NotNull(contractName);
            Assert.False(string.IsNullOrWhiteSpace(contractName.Name));
            Assert.NotEqual(controller.Name, contractName.Name);
        });
    }

    [Fact]
    public void CommonAndAdminDomainControllerActions_UseKoreanOrApprovedTechnicalPrefixes()
    {
        var controllers = GetCommonDomainActionControllers()
            .Concat(GetAdminDomainActionControllers())
            .ToArray();

        var actions = controllers
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any())
            .ToArray();

        Assert.NotEmpty(actions);
        Assert.All(actions, action =>
        {
            var startsWithKorean = action.Name[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z');
            var startsWithApprovedTechnicalPrefix = new[]
            {
                "AI",
                "YouTube",
                "SocialMedia",
                "Card",
                "Event"
            }.Any(prefix => action.Name.StartsWith(prefix, StringComparison.Ordinal));

            Assert.True(
                startsWithKorean || startsWithApprovedTechnicalPrefix,
                $"{action.DeclaringType?.Name}.{action.Name}의 업무 동작 이름을 한국어로 바꿔야 합니다.");
        });
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

    private static IEnumerable<Type> GetConcreteControllers()
        => typeof(인연연결Controller).Assembly
            .GetTypes()
            .Where(type =>
                typeof(ControllerBase).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                type.Name.EndsWith("Controller", StringComparison.Ordinal));

    private static Type[] GetRenamedCommonControllers() =>
    [
        typeof(농수산정보Controller),
        typeof(공동조달계획Controller),
        typeof(커뮤니티기사운행가능Controller),
        typeof(기사커뮤니티문의Controller),
        typeof(커뮤니티동적주제FeedController),
        typeof(커뮤니티키워드알림Controller),
        typeof(커뮤니티키워드구독Controller),
        typeof(커뮤니티원장블록배정Controller),
        typeof(커뮤니티원장역할접근Controller),
        typeof(커뮤니티게시글식재료가격참고Controller),
        typeof(커뮤니티게시글참여기회Controller),
        typeof(공동주문관점조회Controller),
        typeof(인사역할지원Controller),
        typeof(개별주문관점조회Controller),
        typeof(육류수입준비Controller),
        typeof(운영시장ProfileController),
        typeof(판매채널Controller),
        typeof(제3자물류사업자Controller),
        typeof(전통시장물류거점Controller),
        typeof(전통시장Controller),
        typeof(비자지원Controller),
        typeof(업무관계SnapshotController)
    ];

    private static Type[] GetRenamedAdminControllers() =>
    [
        typeof(음식점탐색정책Controller),
        typeof(커뮤니티작성이미지Controller),
        typeof(커뮤니티정보수집Controller),
        typeof(커뮤니티게시글일정Controller),
        typeof(홍익학당CardController),
        typeof(공식음식조리법ArchiveController),
        typeof(배차AI판단사례Controller),
        typeof(국내화물배차AI검토Controller),
        typeof(음식배달배차AI검토Controller),
        typeof(고용계약Controller),
        typeof(참여혜택Controller),
        typeof(인사역할검토Controller),
        typeof(인사역할Controller),
        typeof(사회보험신고Controller),
        typeof(제3자물류사업자접촉Controller),
        typeof(플랫폼이익환원Controller),
        typeof(전통시장물류거점AdminController)
    ];

    private static Type[] GetRenamedPlatformControllers() =>
    [
        typeof(공공데이터ApiMetadataController),
        typeof(공개LocaleController)
    ];

    private static Type[] GetCommonDomainActionControllers() =>
    [
        .. GetRenamedCommonControllers(),
        typeof(커뮤니티게시글첨부Controller),
        typeof(커뮤니티게시글참여Controller),
        typeof(커뮤니티게시글운영Controller),
        typeof(노드스티커상점Controller),
        typeof(커뮤니티게시글Controller),
        typeof(커뮤니티게시판Controller),
        typeof(커뮤니티대화Controller),
        typeof(커뮤니티원장공유Controller),
        typeof(커뮤니티투표Controller),
        typeof(커뮤니티활동신호Controller)
    ];

    private static Type[] GetAdminDomainActionControllers() =>
    [
        .. GetRenamedAdminControllers(),
        typeof(보조기능설정Controller),
        typeof(공동구매물류워크플로우AdminController),
        typeof(공동구매커머스이행계획AdminController),
        typeof(공동구매해외선적추적AdminController),
        typeof(주문자집단운영주체AdminController)
    ];
}
