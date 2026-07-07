using System.Reflection;
using Hongdal.ApiMetadata;
using Hongdal.Controllers.Admin.HumanResources;
using Hongdal.Controllers.Admin.Orderer;
using Hongdal.Controllers.Common;
using Hongdal.Controllers.Orderer;
using Hongdal.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using 홍달.Services.Versioning;

namespace Hongdal.Tests.ApiMetadata;

public sealed class HongdalApiVersionAttributeTests
{
    [Theory]
    [InlineData(HongdalProductVersion.V1_0, "1.0")]
    [InlineData(HongdalProductVersion.V1_5, "1.5")]
    [InlineData(HongdalProductVersion.V2_0, "2.0")]
    [InlineData(HongdalProductVersion.V2_5, "2.5")]
    [InlineData(HongdalProductVersion.V3_0, "3.0")]
    [InlineData(HongdalProductVersion.V3_5, "3.5")]
    public void GetLabel_ReturnsStableProductVersionLabel(HongdalProductVersion version, string expected)
    {
        Assert.Equal(expected, HongdalProductVersionLabels.GetLabel(version));
    }

    [Theory]
    [InlineData(HongdalApiGrowthTrack.Community, "Community")]
    [InlineData(HongdalApiGrowthTrack.OrdererGroupCommerce, "Orderer Group Commerce")]
    public void GetLabel_ReturnsStableGrowthTrackLabel(HongdalApiGrowthTrack track, string expected)
    {
        Assert.Equal(expected, HongdalApiGrowthTrackLabels.GetLabel(track));
    }

    [Theory]
    [InlineData(HongdalWorkflow.DomesticTransport, "국내 화물 운송")]
    [InlineData(HongdalWorkflow.GroupPurchaseImport, "공동주문 수입")]
    [InlineData(HongdalWorkflow.WarehouseFulfillment, "창고 입출고")]
    [InlineData(HongdalWorkflow.SalesChannelFulfillment, "판매채널 출고")]
    [InlineData(HongdalWorkflow.HrParticipation, "참여 인력 관리")]
    public void GetLabel_ReturnsStableWorkflowLabel(HongdalWorkflow workflow, string expected)
    {
        Assert.Equal(expected, HongdalWorkflowLabels.GetLabel(workflow));
    }

    [Theory]
    [InlineData(HongdalWorkflowRelationKind.References, "참조")]
    [InlineData(HongdalWorkflowRelationKind.Calls, "호출")]
    [InlineData(HongdalWorkflowRelationKind.HandsOffTo, "인계")]
    [InlineData(HongdalWorkflowRelationKind.Feeds, "공급")]
    [InlineData(HongdalWorkflowRelationKind.PublishesSignalTo, "신호 공개")]
    [InlineData(HongdalWorkflowRelationKind.OperatesWith, "공동 운영")]
    public void GetLabel_ReturnsStableWorkflowRelationKindLabel(HongdalWorkflowRelationKind kind, string expected)
    {
        Assert.Equal(expected, HongdalWorkflowRelationKindLabels.GetLabel(kind));
    }

    [Fact]
    public void WorkflowRelations_RecordCrossWorkflowIntegrationPoints()
    {
        var relations = HongdalWorkflowRelations.GetAll();

        Assert.Contains(relations, relation =>
            relation.Source == HongdalWorkflow.GroupPurchaseImport &&
            relation.Target == HongdalWorkflow.DomesticTransport &&
            relation.Kind == HongdalWorkflowRelationKind.HandsOffTo);
        Assert.Contains(relations, relation =>
            relation.Source == HongdalWorkflow.GroupPurchaseImport &&
            relation.Target == HongdalWorkflow.CustomsAndTradeData &&
            relation.Kind == HongdalWorkflowRelationKind.References);
        Assert.Contains(relations, relation =>
            relation.Source == HongdalWorkflow.SalesChannelFulfillment &&
            relation.Target == HongdalWorkflow.WarehouseFulfillment &&
            relation.Kind == HongdalWorkflowRelationKind.Calls);
        Assert.Contains(relations, relation =>
            relation.Source == HongdalWorkflow.DomesticTransport &&
            relation.Target == HongdalWorkflow.CommunityTrust &&
            relation.Kind == HongdalWorkflowRelationKind.PublishesSignalTo);
        Assert.All(relations, relation => Assert.False(string.IsNullOrWhiteSpace(relation.Summary)));
    }

    [Fact]
    public void WorkflowRelations_CanBeQueriedByIncomingAndOutgoingWorkflow()
    {
        var groupPurchaseOutgoing = HongdalWorkflowRelations.GetOutgoing(HongdalWorkflow.GroupPurchaseImport);
        var domesticTransportIncoming = HongdalWorkflowRelations.GetIncoming(HongdalWorkflow.DomesticTransport);

        Assert.Contains(groupPurchaseOutgoing, relation => relation.Target == HongdalWorkflow.DomesticTransport);
        Assert.Contains(groupPurchaseOutgoing, relation => relation.Target == HongdalWorkflow.SalesChannelFulfillment);
        Assert.Contains(domesticTransportIncoming, relation => relation.Source == HongdalWorkflow.GroupPurchaseImport);
        Assert.Contains(domesticTransportIncoming, relation => relation.Source == HongdalWorkflow.HongdalMart);
    }

    [Fact]
    public void WorkflowParticipants_RecordPrimaryUsersAndBoundaries()
    {
        var groupPurchaseParticipants = HongdalWorkflowParticipants.GetByWorkflow(HongdalWorkflow.GroupPurchaseImport);
        var domesticBoundary = HongdalWorkflowParticipants.GetBoundarySummary(HongdalWorkflow.DomesticTransport);

        Assert.Contains(groupPurchaseParticipants, participant =>
            participant.ActorName == "주문자 집단 대표" &&
            participant.IsPrimary);
        Assert.Contains(groupPurchaseParticipants, participant =>
            participant.ActorName == "주문자" &&
            participant.IsPrimary);
        Assert.Contains(groupPurchaseParticipants, participant =>
            participant.ActorName == "플랫폼 운영자" &&
            !participant.IsPrimary);
        Assert.Contains("상차", domesticBoundary);
        Assert.Contains("정산", domesticBoundary);
    }

    [Fact]
    public void WorkflowScreens_RecordAppAndScreenBoundaries()
    {
        var domesticScreens = HongdalWorkflowScreens.GetByWorkflow(HongdalWorkflow.DomesticTransport);
        var groupPurchaseScreens = HongdalWorkflowScreens.GetByWorkflowAndActor(
            HongdalWorkflow.GroupPurchaseImport,
            "Orderer");

        Assert.Contains(domesticScreens, screen =>
            screen.ActorCode == "Shipper" &&
            screen.AppCode == "ShipperApp" &&
            screen.Route == "/shipper/request");
        Assert.Contains(domesticScreens, screen =>
            screen.ActorCode == "Driver" &&
            screen.AppCode == "DriverApp" &&
            screen.Route == "/driver/recommendations");
        Assert.Contains(groupPurchaseScreens, screen =>
            screen.AppCode == "OrdererApp" &&
            screen.Route == "/group-purchase");
    }

    [Fact]
    public void VersionFeatureFlagsController_ReturnsWorkflowStatesAndRelations()
    {
        var controller = new VersionFeatureFlagsController(new FakeVersionFeatureFlagService());

        var result = controller.Get();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Hongdal.Contracts.Common.Versioning.VersionFeatureFlagsResponse>(ok.Value);

        Assert.Contains(response.Workflows, workflow =>
            workflow.WorkflowCode == nameof(HongdalWorkflow.GroupPurchaseImport) &&
            workflow.WorkflowName == "공동주문 수입" &&
            workflow.FlagKey == VersionFeatureFlagKeys.GroupPurchaseImportWorkflow &&
            workflow.Participants.Any(participant => participant.ActorName == "주문자 집단 대표" && participant.IsPrimary) &&
            workflow.Screens.Any(screen => screen.AppCode == "OrdererApp" && screen.Route == "/group-purchase") &&
            !string.IsNullOrWhiteSpace(workflow.BoundarySummary));
        Assert.Contains(response.WorkflowRelations, relation =>
            relation.SourceWorkflowName == "공동주문 수입" &&
            relation.TargetWorkflowName == "국내 화물 운송" &&
            relation.RelationKindName == "인계");
    }

    [Fact]
    public void Controllers_HaveHongdalProductVersionMetadata()
    {
        var missingControllers = GetControllerTypes()
            .Where(type => type.GetCustomAttribute<HongdalApiVersionAttribute>(inherit: true) is null)
            .Select(type => type.FullName)
            .Order()
            .ToArray();

        Assert.Empty(missingControllers);
    }

    [Fact]
    public void VersionFeatureGatedApis_RecordMatchingFeatureKeyInVersionMetadata()
    {
        var missingFeatureMetadata = new List<string>();

        foreach (var controllerType in GetControllerTypes())
        {
            var controllerVersion = controllerType.GetCustomAttribute<HongdalApiVersionAttribute>(inherit: true);
            foreach (var featureAttribute in controllerType.GetCustomAttributes<RequireVersionFeatureAttribute>(inherit: true))
            {
                var featureKey = GetRequiredFeatureKey(featureAttribute);
                if (!string.Equals(controllerVersion?.FeatureKey, featureKey, StringComparison.Ordinal))
                {
                    missingFeatureMetadata.Add($"{controllerType.FullName}: {featureKey}");
                }

                if (!string.Equals(controllerVersion?.WorkflowKey, featureKey, StringComparison.Ordinal))
                {
                    missingFeatureMetadata.Add($"{controllerType.FullName}: workflow {featureKey}");
                }
            }

            foreach (var action in GetActionMethods(controllerType))
            {
                var actionVersion = action.GetCustomAttribute<HongdalApiVersionAttribute>(inherit: true);
                foreach (var featureAttribute in action.GetCustomAttributes<RequireVersionFeatureAttribute>(inherit: true))
                {
                    var featureKey = GetRequiredFeatureKey(featureAttribute);
                    if (!string.Equals(actionVersion?.FeatureKey, featureKey, StringComparison.Ordinal))
                    {
                        missingFeatureMetadata.Add($"{controllerType.FullName}.{action.Name}: {featureKey}");
                    }

                    if (!string.Equals(actionVersion?.WorkflowKey, featureKey, StringComparison.Ordinal))
                    {
                        missingFeatureMetadata.Add($"{controllerType.FullName}.{action.Name}: workflow {featureKey}");
                    }
                }
            }
        }

        Assert.Empty(missingFeatureMetadata.Order().ToArray());
    }

    [Fact]
    public void WorkflowApis_AreTaggedWithWorkflowMetadata()
    {
        var missingWorkflow = new List<string>();

        AddIfMissingWorkflow(typeof(GroupPurchaseOverseasShipmentTrackingController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(GroupPurchaseCommerceFulfillmentPlanController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(GroupPurchaseLogisticsWorkflowController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(OrdererGroupOperatingEntitiesController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(GroupPurchaseOverseasShipmentTrackingAdminController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(GroupPurchaseCommerceFulfillmentPlanAdminController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(GroupPurchaseLogisticsWorkflowAdminController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(OrdererGroupOperatingEntitiesAdminController), HongdalWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(SocialInsuranceFilingsController), HongdalWorkflow.HrParticipation, missingWorkflow);
        AddIfMissingWorkflow(typeof(WarehouseOperationsController), HongdalWorkflow.WarehouseFulfillment, missingWorkflow);
        AddIfMissingWorkflow(typeof(SalesChannelsController), HongdalWorkflow.SalesChannelFulfillment, missingWorkflow);
        AddIfMissingWorkflow(
            typeof(PublicDataLookupController).GetMethod(nameof(PublicDataLookupController.FindOrdererGroupScopes)),
            "Hongdal.Controllers.Orderer.PublicDataLookupController.FindOrdererGroupScopes",
            HongdalWorkflow.GroupPurchaseImport,
            missingWorkflow);

        Assert.Empty(missingWorkflow);
    }

    [Fact]
    public void CommunityApis_AreTaggedWithCommunityGrowthTrack()
    {
        var missingCommunityTrack = new List<string>();

        AddIfMissingCommunityTrack(typeof(PlatformCommunityPostsController), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(CommunityActivitySignalsController), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(CommunityVotesController), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(인연연결Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(감사메시지Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(WorkRelationshipSnapshotsController), missingCommunityTrack);
        AddIfMissingCommunityTrack(
            typeof(인증Controller).GetMethod("가입온보딩인연후보조회"),
            "Hongdal.Controllers.Common.인증Controller.가입온보딩인연후보조회",
            missingCommunityTrack);

        Assert.Empty(missingCommunityTrack);
    }

    private static IEnumerable<Type> GetControllerTypes()
    {
        return typeof(VersionFeatureFlagsController).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type)
                && !type.IsAbstract
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .OrderBy(type => type.FullName);
    }

    private static IEnumerable<MethodInfo> GetActionMethods(Type controllerType)
    {
        return controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any()
                || method.GetCustomAttributes<RouteAttribute>(inherit: true).Any());
    }

    private static string GetRequiredFeatureKey(RequireVersionFeatureAttribute attribute)
    {
        var featureKey = attribute.Arguments?.OfType<string>().FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(featureKey));
        return featureKey!;
    }

    private static void AddIfMissingCommunityTrack(Type controllerType, List<string> missingCommunityTrack)
    {
        var hasCommunityTrack = controllerType
            .GetCustomAttributes<HongdalApiGrowthTrackAttribute>(inherit: true)
            .Any(attribute => attribute.Track == HongdalApiGrowthTrack.Community);
        if (!hasCommunityTrack)
        {
            missingCommunityTrack.Add(controllerType.FullName ?? controllerType.Name);
        }
    }

    private static void AddIfMissingCommunityTrack(MethodInfo? action, string displayName, List<string> missingCommunityTrack)
    {
        Assert.NotNull(action);

        var hasCommunityTrack = action!
            .GetCustomAttributes<HongdalApiGrowthTrackAttribute>(inherit: true)
            .Any(attribute => attribute.Track == HongdalApiGrowthTrack.Community);
        if (!hasCommunityTrack)
        {
            missingCommunityTrack.Add(displayName);
        }
    }

    private static void AddIfMissingWorkflow(Type controllerType, HongdalWorkflow workflow, List<string> missingWorkflow)
    {
        var hasWorkflow = controllerType
            .GetCustomAttributes<HongdalApiWorkflowAttribute>(inherit: true)
            .Any(attribute => attribute.Workflow == workflow);
        if (!hasWorkflow)
        {
            missingWorkflow.Add($"{controllerType.FullName ?? controllerType.Name}: {workflow}");
        }
    }

    private static void AddIfMissingWorkflow(MethodInfo? action, string displayName, HongdalWorkflow workflow, List<string> missingWorkflow)
    {
        Assert.NotNull(action);

        var hasWorkflow = action!
            .GetCustomAttributes<HongdalApiWorkflowAttribute>(inherit: true)
            .Any(attribute => attribute.Workflow == workflow);
        if (!hasWorkflow)
        {
            missingWorkflow.Add($"{displayName}: {workflow}");
        }
    }

    private sealed class FakeVersionFeatureFlagService : IVersionFeatureFlagService
    {
        public bool IsEnabled(string featureKey)
            => GetAll().TryGetValue(featureKey, out var enabled) && enabled;

        public IReadOnlyDictionary<string, bool> GetAll()
        {
            return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                [VersionFeatureFlagKeys.DomesticTransportWorkflow] = true,
                [VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow] = false,
                [VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow] = false,
                [VersionFeatureFlagKeys.GroupPurchaseImportWorkflow] = false,
                [VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow] = false,
                [VersionFeatureFlagKeys.CommunityTrustWorkflow] = true,
                [VersionFeatureFlagKeys.HrParticipationWorkflow] = false,
                [VersionFeatureFlagKeys.FoodDeliveryWorkflow] = false,
                [VersionFeatureFlagKeys.HongdalMartWorkflow] = false
            };
        }
    }
}
