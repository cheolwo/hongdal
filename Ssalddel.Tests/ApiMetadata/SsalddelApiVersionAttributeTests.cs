using System.Reflection;
using Ssalddel.Application.Driver.Recommendation;
using Ssalddel.Application.Sales;
using Ssalddel.Application.Shipper.Request;
using Ssalddel.Application.Versioning;
using Ssalddel.Application.Warehouse;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Admin.HumanResources;
using Ssalddel.Controllers.Admin.Orderer;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Filters;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.ApiMetadata;

public sealed class SsalddelApiVersionAttributeTests
{
    [Theory]
    [InlineData(SsalddelProductVersion.V0_0, "0.0")]
    [InlineData(SsalddelProductVersion.V1_0, "1.0")]
    [InlineData(SsalddelProductVersion.V1_5, "1.5")]
    [InlineData(SsalddelProductVersion.V2_0, "2.0")]
    [InlineData(SsalddelProductVersion.V2_5, "2.5")]
    [InlineData(SsalddelProductVersion.V3_0, "3.0")]
    [InlineData(SsalddelProductVersion.V3_5, "3.5")]
    public void GetLabel_ReturnsStableProductVersionLabel(SsalddelProductVersion version, string expected)
    {
        Assert.Equal(expected, SsalddelProductVersionLabels.GetLabel(version));
    }

    [Theory]
    [InlineData(SsalddelApiGrowthTrack.Community, "Community")]
    [InlineData(SsalddelApiGrowthTrack.OrdererGroupCommerce, "Orderer Group Commerce")]
    public void GetLabel_ReturnsStableGrowthTrackLabel(SsalddelApiGrowthTrack track, string expected)
    {
        Assert.Equal(expected, SsalddelApiGrowthTrackLabels.GetLabel(track));
    }

    [Theory]
    [InlineData(SsalddelWorkflow.DomesticTransport, "국내 화물 운송")]
    [InlineData(SsalddelWorkflow.GroupPurchaseImport, "공동주문 수입")]
    [InlineData(SsalddelWorkflow.WarehouseFulfillment, "창고 입출고")]
    [InlineData(SsalddelWorkflow.SalesChannelFulfillment, "판매채널 출고")]
    [InlineData(SsalddelWorkflow.HrParticipation, "참여 인력 관리")]
    public void GetLabel_ReturnsStableWorkflowLabel(SsalddelWorkflow workflow, string expected)
    {
        Assert.Equal(expected, SsalddelWorkflowLabels.GetLabel(workflow));
    }

    [Theory]
    [InlineData(SsalddelWorkflowRelationKind.References, "참조")]
    [InlineData(SsalddelWorkflowRelationKind.Calls, "호출")]
    [InlineData(SsalddelWorkflowRelationKind.HandsOffTo, "인계")]
    [InlineData(SsalddelWorkflowRelationKind.Feeds, "공급")]
    [InlineData(SsalddelWorkflowRelationKind.PublishesSignalTo, "신호 공개")]
    [InlineData(SsalddelWorkflowRelationKind.OperatesWith, "공동 운영")]
    public void GetLabel_ReturnsStableWorkflowRelationKindLabel(SsalddelWorkflowRelationKind kind, string expected)
    {
        Assert.Equal(expected, SsalddelWorkflowRelationKindLabels.GetLabel(kind));
    }

    [Theory]
    [InlineData(SsalddelActor.Shipper, "화주")]
    [InlineData(SsalddelActor.Driver, "기사")]
    [InlineData(SsalddelActor.OrdererGroupLeader, "주문자 집단 대표")]
    [InlineData(SsalddelActor.WarehouseManager, "창고 관리자")]
    [InlineData(SsalddelActor.PlatformOperator, "플랫폼 운영자")]
    public void GetLabel_ReturnsStableActorLabel(SsalddelActor actor, string expected)
    {
        Assert.Equal(expected, SsalddelActorLabels.GetLabel(actor));
    }

    [Theory]
    [InlineData(SsalddelUseCaseActorRole.Primary, "주 액터")]
    [InlineData(SsalddelUseCaseActorRole.Supporting, "보조 액터")]
    public void GetLabel_ReturnsStableUseCaseActorRoleLabel(SsalddelUseCaseActorRole role, string expected)
    {
        Assert.Equal(expected, SsalddelUseCaseActorRoleLabels.GetLabel(role));
    }

    [Theory]
    [InlineData(SsalddelUseCaseRelationKind.Include, "포함")]
    [InlineData(SsalddelUseCaseRelationKind.Extend, "확장")]
    public void GetLabel_ReturnsStableUseCaseRelationKindLabel(SsalddelUseCaseRelationKind kind, string expected)
    {
        Assert.Equal(expected, SsalddelUseCaseRelationKindLabels.GetLabel(kind));
    }

    [Fact]
    public void WorkflowRelations_RecordCrossWorkflowIntegrationPoints()
    {
        var relations = SsalddelWorkflowRelations.GetAll();

        Assert.Contains(relations, relation =>
            relation.Source == SsalddelWorkflow.GroupPurchaseImport &&
            relation.Target == SsalddelWorkflow.DomesticTransport &&
            relation.Kind == SsalddelWorkflowRelationKind.HandsOffTo);
        Assert.Contains(relations, relation =>
            relation.Source == SsalddelWorkflow.GroupPurchaseImport &&
            relation.Target == SsalddelWorkflow.CustomsAndTradeData &&
            relation.Kind == SsalddelWorkflowRelationKind.References);
        Assert.Contains(relations, relation =>
            relation.Source == SsalddelWorkflow.SalesChannelFulfillment &&
            relation.Target == SsalddelWorkflow.WarehouseFulfillment &&
            relation.Kind == SsalddelWorkflowRelationKind.Calls);
        Assert.Contains(relations, relation =>
            relation.Source == SsalddelWorkflow.DomesticTransport &&
            relation.Target == SsalddelWorkflow.CommunityTrust &&
            relation.Kind == SsalddelWorkflowRelationKind.PublishesSignalTo);
        Assert.All(relations, relation => Assert.False(string.IsNullOrWhiteSpace(relation.Summary)));
    }

    [Fact]
    public void WorkflowRelations_CanBeQueriedByIncomingAndOutgoingWorkflow()
    {
        var groupPurchaseOutgoing = SsalddelWorkflowRelations.GetOutgoing(SsalddelWorkflow.GroupPurchaseImport);
        var domesticTransportIncoming = SsalddelWorkflowRelations.GetIncoming(SsalddelWorkflow.DomesticTransport);

        Assert.Contains(groupPurchaseOutgoing, relation => relation.Target == SsalddelWorkflow.DomesticTransport);
        Assert.Contains(groupPurchaseOutgoing, relation => relation.Target == SsalddelWorkflow.SalesChannelFulfillment);
        Assert.Contains(domesticTransportIncoming, relation => relation.Source == SsalddelWorkflow.GroupPurchaseImport);
        Assert.Contains(domesticTransportIncoming, relation => relation.Source == SsalddelWorkflow.SsalddelMart);
    }

    [Fact]
    public void WorkflowParticipants_RecordPrimaryUsersAndBoundaries()
    {
        var groupPurchaseParticipants = SsalddelWorkflowParticipants.GetByWorkflow(SsalddelWorkflow.GroupPurchaseImport);
        var domesticBoundary = SsalddelWorkflowParticipants.GetBoundarySummary(SsalddelWorkflow.DomesticTransport);

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
    public void CoreUseCases_RecordPrimaryActorMetadata()
    {
        AssertUseCaseHasPrimaryActor(typeof(화주운송의뢰UseCase), SsalddelActor.Shipper);
        AssertUseCaseHasPrimaryActor(typeof(기사배차추천UseCase), SsalddelActor.Driver);
        AssertUseCaseHasPrimaryActor(typeof(공동구매자동집단화UseCase), SsalddelActor.Orderer);
        AssertUseCaseHasPrimaryActor(typeof(공동구매커머스이행계획UseCase), SsalddelActor.OrdererGroupLeader);
        AssertUseCaseHasPrimaryActor(typeof(창고작업UseCase), SsalddelActor.WarehouseManager);
        AssertUseCaseHasPrimaryActor(typeof(판매채널UseCase), SsalddelActor.Seller);
    }

    [Fact]
    public void CoreUseCases_RecordWorkflowAndDisplayNameMetadata()
    {
        AssertUseCaseHasWorkflow(typeof(화주운송의뢰UseCase), SsalddelWorkflow.DomesticTransport);
        AssertUseCaseHasWorkflow(typeof(기사배차추천UseCase), SsalddelWorkflow.DomesticTransport);
        AssertUseCaseHasWorkflow(typeof(공동구매자동집단화UseCase), SsalddelWorkflow.GroupPurchaseImport);
        AssertUseCaseHasWorkflow(typeof(공동구매커머스이행계획UseCase), SsalddelWorkflow.GroupPurchaseImport);
        AssertUseCaseHasWorkflow(typeof(창고작업UseCase), SsalddelWorkflow.WarehouseFulfillment);
        AssertUseCaseHasWorkflow(typeof(판매채널UseCase), SsalddelWorkflow.SalesChannelFulfillment);
    }

    [Fact]
    public void CoreUseCases_RecordIncludeAndExtendRelations()
    {
        AssertUseCaseHasRelation(typeof(공동구매자동집단화UseCase), SsalddelUseCaseRelationKind.Include, "공공데이터조회UseCase");
        AssertUseCaseHasRelation(typeof(화주운송의뢰UseCase), SsalddelUseCaseRelationKind.Extend, "문서관리UseCase");
        AssertUseCaseHasRelation(typeof(공동구매커머스이행계획UseCase), SsalddelUseCaseRelationKind.Extend, "화주운송의뢰UseCase");
        AssertUseCaseHasRelation(typeof(커뮤니티게시글UseCase), SsalddelUseCaseRelationKind.Extend, "커뮤니티투표UseCase");
    }

    [Fact]
    public void WorkflowScreens_RecordAppAndScreenBoundaries()
    {
        var domesticScreens = SsalddelWorkflowScreens.GetByWorkflow(SsalddelWorkflow.DomesticTransport);
        var groupPurchaseScreens = SsalddelWorkflowScreens.GetByWorkflowAndActor(
            SsalddelWorkflow.GroupPurchaseImport,
            "Orderer");

        Assert.Contains(domesticScreens, screen =>
            screen.ActorCode == "Shipper" &&
            screen.AppCode == "SsalddelApp" &&
            screen.Route == "/shipper/request");
        Assert.Contains(domesticScreens, screen =>
            screen.ActorCode == "Driver" &&
            screen.AppCode == "DriverApp" &&
            screen.Route == "/driver/recommendations");
        Assert.Contains(groupPurchaseScreens, screen =>
            screen.AppCode == "SsalddelApp" &&
            screen.Route == "/community/group-import");
    }

    [Fact]
    public void VersionFeatureFlagsController_ReturnsWorkflowStatesAndRelations()
    {
        var controller = new VersionFeatureFlagsController(
            new 버전워크플로우UseCase(new FakeVersionFeatureFlagService()));

        var result = controller.Get();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Ssalddel.Contracts.Common.Versioning.VersionFeatureFlagsResponse>(ok.Value);

        Assert.Contains(response.Workflows, workflow =>
            workflow.WorkflowCode == nameof(SsalddelWorkflow.GroupPurchaseImport) &&
            workflow.WorkflowName == "공동주문 수입" &&
            workflow.FlagKey == VersionFeatureFlagKeys.GroupPurchaseImportWorkflow &&
            workflow.Participants.Any(participant => participant.ActorName == "주문자 집단 대표" && participant.IsPrimary) &&
            workflow.Screens.Any(screen => screen.AppCode == "SsalddelApp" && screen.Route == "/community/group-import") &&
            workflow.UseCases.Any(useCase => useCase.UseCaseCode == nameof(공동구매자동집단화UseCase) &&
                useCase.PrimaryActors.Any(actor => actor.ActorCode == nameof(SsalddelActor.Orderer)) &&
                useCase.Relations.Any(relation =>
                    relation.RelationKindCode == nameof(SsalddelUseCaseRelationKind.Include) &&
                    relation.TargetUseCaseCode == "공공데이터조회UseCase")) &&
            !string.IsNullOrWhiteSpace(workflow.BoundarySummary));
        Assert.Contains(response.Workflows, workflow =>
            workflow.WorkflowCode == nameof(SsalddelWorkflow.DomesticTransport) &&
            workflow.UseCases.Any(useCase => useCase.UseCaseCode == nameof(화주운송의뢰UseCase) &&
                useCase.Relations.Any(relation =>
                    relation.RelationKindCode == nameof(SsalddelUseCaseRelationKind.Extend) &&
                    relation.TargetUseCaseCode == "문서관리UseCase")) &&
            workflow.UseCases.Any(useCase => useCase.UseCaseCode == nameof(기사배차추천UseCase)));
        Assert.Contains(response.WorkflowRelations, relation =>
            relation.SourceWorkflowName == "공동주문 수입" &&
            relation.TargetWorkflowName == "국내 화물 운송" &&
            relation.RelationKindName == "인계");
        Assert.Contains(response.OperatingSystems, operatingSystem =>
            operatingSystem.OperatingSystemCode == nameof(SsalddelOperatingSystem.DomesticCargoTransport) &&
            operatingSystem.CanonicalOperatingSystemId == OperatingSystemIds.DomesticCargoTransport &&
            operatingSystem.OperatingSystemAliases.Contains(nameof(SsalddelOperatingSystem.DomesticCargoTransport)) &&
            operatingSystem.FeatureKey == VersionFeatureFlagKeys.DomesticTransportWorkflow &&
            operatingSystem.IsEnabled &&
            operatingSystem.Engines.Any(engine =>
                engine.EngineCode == EngineFamilyIds.TransportRequestDispatch &&
                engine.EngineFamilyId == EngineFamilyIds.TransportRequestDispatch &&
                engine.RuntimeStatus == RuntimeCapabilityStatuses.Active &&
                engine.ImplementationIds.Contains(EngineImplementationIds.CargoYongdalDispatch) &&
                engine.ImplementationIds.Contains(EngineImplementationIds.FoodDeliveryDispatch)) &&
            operatingSystem.SchedulingPolicies.Any(policy => policy.PolicyKindCode == nameof(SsalddelSchedulingPolicyKind.Mlfq)) &&
            operatingSystem.SchedulingPolicies.Any(policy => policy.PolicyKindCode == nameof(SsalddelSchedulingPolicyKind.Aging)) &&
            operatingSystem.SchedulingPolicies.All(policy => policy.RuntimeStatus == RuntimeCapabilityStatuses.Declared));
        Assert.Contains(response.OperatingSystems, operatingSystem =>
            operatingSystem.OperatingSystemCode == nameof(SsalddelOperatingSystem.WarehouseCommerceFulfillment) &&
            operatingSystem.CanonicalOperatingSystemId == OperatingSystemIds.WarehouseCommerceFulfillment &&
            !operatingSystem.IsEnabled &&
            operatingSystem.SchedulingPolicies.Any(policy => policy.PolicyKindCode == nameof(SsalddelSchedulingPolicyKind.Sjf)) &&
            operatingSystem.SchedulingPolicies.Any(policy => policy.PolicyKindCode == nameof(SsalddelSchedulingPolicyKind.Affinity)));
        Assert.Contains(response.OperatingSystems, operatingSystem =>
            operatingSystem.OperatingSystemCode == nameof(SsalddelOperatingSystem.PlatformOperations) &&
            operatingSystem.FeatureKey == string.Empty &&
            operatingSystem.IsEnabled &&
            operatingSystem.Engines.All(engine => engine.RuntimeStatus == RuntimeCapabilityStatuses.Declared));
    }

    [Fact]
    public void VersionFeatureFlagsController_ReturnsApiEndpointMetadataFromExistingRoutes()
    {
        var controller = new VersionFeatureFlagsController(
            new 버전워크플로우UseCase(new FakeVersionFeatureFlagService()));

        var result = controller.Get();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Ssalddel.Contracts.Common.Versioning.VersionFeatureFlagsResponse>(ok.Value);

        Assert.Contains(response.ApiEndpoints, endpoint =>
            endpoint.EndpointKey == "화주운송의뢰Controller.의뢰생성" &&
            endpoint.Method == "POST" &&
            endpoint.RoutePattern == "api/v1/shipper/requests" &&
            endpoint.ProductVersionName == "1.0" &&
            endpoint.FeatureKey == VersionFeatureFlagKeys.DomesticTransportWorkflow &&
            endpoint.IsEnabled);
        Assert.Contains(response.ApiEndpoints, endpoint =>
            endpoint.EndpointKey == "기사운송진행Controller.상차완료" &&
            endpoint.Method == "POST" &&
            endpoint.RoutePattern == "api/v1/driver/transports/{id:long}/pickup-complete");
        Assert.Contains(response.ApiEndpoints, endpoint =>
            endpoint.EndpointKey == "커뮤니티게시글Controller.Create" &&
            endpoint.Method == "POST" &&
            endpoint.RoutePattern == "api/v1/community/posts" &&
            endpoint.ProductVersionName == "0.0" &&
            endpoint.FeatureKey == string.Empty &&
            endpoint.IsEnabled &&
            endpoint.AllowsAnonymous);
        Assert.Contains(response.ApiEndpoints, endpoint =>
            endpoint.ControllerName == nameof(Ssalddel.Controllers.Food.음식주문Controller) &&
            endpoint.FeatureKey == VersionFeatureFlagKeys.FoodDeliveryWorkflow &&
            !endpoint.IsEnabled);
    }

    [Fact]
    public void Controllers_HaveSsalddelProductVersionMetadata()
    {
        var missingControllers = GetControllerTypes()
            .Where(type => type.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true) is null)
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
            var controllerVersion = controllerType.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
            foreach (var featureAttribute in controllerType.GetCustomAttributes<RequireVersionFeatureAttribute>(inherit: true))
            {
                var featureKey = Get필요FeatureKey(featureAttribute);
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
                var actionVersion = action.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
                foreach (var featureAttribute in action.GetCustomAttributes<RequireVersionFeatureAttribute>(inherit: true))
                {
                    var featureKey = Get필요FeatureKey(featureAttribute);
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
    public void VersionFeatureMetadata_IsResolvableForEveryControllerAction()
    {
        var unresolvedEndpoints = new List<string>();
        var featureEndpointCount = 0;

        foreach (var controllerType in GetControllerTypes())
        {
            var controllerFeatureKey = controllerType
                .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
                .FeatureKey;

            foreach (var action in GetActionMethods(controllerType))
            {
                var actionFeatureKey = action
                    .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?
                    .FeatureKey;
                var expectedFeatureKey = string.IsNullOrWhiteSpace(actionFeatureKey)
                    ? controllerFeatureKey
                    : actionFeatureKey;
                if (string.IsNullOrWhiteSpace(expectedFeatureKey))
                {
                    continue;
                }

                featureEndpointCount++;
                var descriptor = new ControllerActionDescriptor
                {
                    ControllerName = controllerType.Name,
                    ActionName = action.Name,
                    ControllerTypeInfo = controllerType.GetTypeInfo(),
                    MethodInfo = action
                };
                var resolvedFeatureKey = SsalddelApiVersionFeatureFilter.ResolveFeatureKey(descriptor);
                if (!string.Equals(expectedFeatureKey, resolvedFeatureKey, StringComparison.Ordinal))
                {
                    unresolvedEndpoints.Add($"{controllerType.FullName}.{action.Name}: {expectedFeatureKey}");
                }
            }
        }

        Assert.True(featureEndpointCount > 0);
        Assert.Empty(unresolvedEndpoints.Order().ToArray());
    }

    [Fact]
    public void CoreWorkflowEntryPoints_DeclareTheirExecutionFeatureKeys()
    {
        var cargoVersion = typeof(Ssalddel.Controllers.Shipper.Request01.화주운송의뢰Controller)
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
        var warehouseVersion = typeof(WarehouseOperationsController)
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
        var foodVersion = typeof(Ssalddel.Controllers.Food.음식주문Controller)
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);

        Assert.Equal(VersionFeatureFlagKeys.DomesticTransportWorkflow, cargoVersion?.FeatureKey);
        Assert.Equal(VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow, warehouseVersion?.FeatureKey);
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, foodVersion?.FeatureKey);
    }

    [Fact]
    public void VersionFeatureFlagsController_RemainsUngatedBootstrapEndpoint()
    {
        var controllerType = typeof(VersionFeatureFlagsController);
        var controllerVersion = controllerType
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);

        Assert.NotNull(controllerVersion);
        Assert.True(string.IsNullOrWhiteSpace(controllerVersion!.FeatureKey));

        foreach (var action in GetActionMethods(controllerType))
        {
            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = controllerType.Name,
                ActionName = action.Name,
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = action
            };

            Assert.Null(SsalddelApiVersionFeatureFilter.ResolveFeatureKey(descriptor));
        }
    }

    [Fact]
    public void WorkflowApis_AreTaggedWithWorkflowMetadata()
    {
        var missingWorkflow = new List<string>();

        AddIfMissingWorkflow(typeof(공동구매해외선적추적Controller), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매커머스이행계획Controller), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매자동집단화Controller), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매물류워크플로우Controller), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(주문자집단운영주체Controller), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매해외선적추적AdminController), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매커머스이행계획AdminController), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(공동구매물류워크플로우AdminController), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(주문자집단운영주체AdminController), SsalddelWorkflow.GroupPurchaseImport, missingWorkflow);
        AddIfMissingWorkflow(typeof(SocialInsuranceFilingsController), SsalddelWorkflow.HrParticipation, missingWorkflow);
        AddIfMissingWorkflow(typeof(WarehouseOperationsController), SsalddelWorkflow.WarehouseFulfillment, missingWorkflow);
        AddIfMissingWorkflow(typeof(SalesChannelsController), SsalddelWorkflow.SalesChannelFulfillment, missingWorkflow);
        AddIfMissingWorkflow(
            typeof(PublicDataLookupController).GetMethod(nameof(PublicDataLookupController.주문자집단배송권검색)),
            "Ssalddel.Controllers.Orderer.PublicDataLookupController.주문자집단배송권검색",
            SsalddelWorkflow.GroupPurchaseImport,
            missingWorkflow);

        Assert.Empty(missingWorkflow);
    }

    [Fact]
    public void CommunityApis_AreTaggedWithCommunityGrowthTrack()
    {
        var missingCommunityTrack = new List<string>();

        AddIfMissingCommunityTrack(typeof(커뮤니티게시글Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(커뮤니티활동신호Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(커뮤니티투표Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(인연연결Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(감사메시지Controller), missingCommunityTrack);
        AddIfMissingCommunityTrack(typeof(WorkRelationshipSnapshotsController), missingCommunityTrack);
        AddIfMissingCommunityTrack(
            typeof(인증Controller).GetMethod("가입온보딩인연후보조회"),
            "Ssalddel.Controllers.Common.인증Controller.가입온보딩인연후보조회",
            missingCommunityTrack);

        Assert.Empty(missingCommunityTrack);
    }

    [Fact]
    public void CommunityFoundationApis_AreIntroducedInVersionZero()
    {
        var communityFoundationControllers = new[]
        {
            typeof(커뮤니티게시글Controller),
            typeof(커뮤니티게시판Controller),
            typeof(커뮤니티대화Controller),
            typeof(커뮤니티투표Controller),
            typeof(커뮤니티원장공유Controller),
            typeof(주문원장Controller)
        };

        var incorrectlyVersioned = communityFoundationControllers
            .Where(type => type.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?.Version
                != SsalddelProductVersion.V0_0)
            .Select(type => type.FullName)
            .Order()
            .ToArray();

        Assert.Empty(incorrectlyVersioned);
    }

    [Fact]
    public void DomesticTransportApis_RemainVersionOne()
    {
        var version = typeof(Ssalddel.Controllers.Shipper.Request01.화주운송의뢰Controller)
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true)?.Version;

        Assert.Equal(SsalddelProductVersion.V1_0, version);
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

    private static string Get필요FeatureKey(RequireVersionFeatureAttribute attribute)
    {
        var featureKey = attribute.Arguments?.OfType<string>().FirstOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(featureKey));
        return featureKey!;
    }

    private static void AddIfMissingCommunityTrack(Type controllerType, List<string> missingCommunityTrack)
    {
        var hasCommunityTrack = controllerType
            .GetCustomAttributes<SsalddelApiGrowthTrackAttribute>(inherit: true)
            .Any(attribute => attribute.Track == SsalddelApiGrowthTrack.Community);
        if (!hasCommunityTrack)
        {
            missingCommunityTrack.Add(controllerType.FullName ?? controllerType.Name);
        }
    }

    private static void AddIfMissingCommunityTrack(MethodInfo? action, string displayName, List<string> missingCommunityTrack)
    {
        Assert.NotNull(action);

        var hasCommunityTrack = action!
            .GetCustomAttributes<SsalddelApiGrowthTrackAttribute>(inherit: true)
            .Any(attribute => attribute.Track == SsalddelApiGrowthTrack.Community);
        if (!hasCommunityTrack)
        {
            missingCommunityTrack.Add(displayName);
        }
    }

    private static void AddIfMissingWorkflow(Type controllerType, SsalddelWorkflow workflow, List<string> missingWorkflow)
    {
        var hasWorkflow = controllerType
            .GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true)
            .Any(attribute => attribute.Workflow == workflow);
        if (!hasWorkflow)
        {
            missingWorkflow.Add($"{controllerType.FullName ?? controllerType.Name}: {workflow}");
        }
    }

    private static void AddIfMissingWorkflow(MethodInfo? action, string displayName, SsalddelWorkflow workflow, List<string> missingWorkflow)
    {
        Assert.NotNull(action);

        var hasWorkflow = action!
            .GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true)
            .Any(attribute => attribute.Workflow == workflow);
        if (!hasWorkflow)
        {
            missingWorkflow.Add($"{displayName}: {workflow}");
        }
    }

    private static void AssertUseCaseHasPrimaryActor(Type useCaseType, SsalddelActor actor)
    {
        var attributes = useCaseType.GetCustomAttributes<SsalddelUseCaseActorAttribute>(inherit: true);
        Assert.Contains(attributes, attribute =>
            attribute.Actor == actor &&
            attribute.Role == SsalddelUseCaseActorRole.Primary &&
            !string.IsNullOrWhiteSpace(attribute.ActorLabel));
    }

    private static void AssertUseCaseHasWorkflow(Type useCaseType, SsalddelWorkflow workflow)
    {
        var workflowAttributes = useCaseType.GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true);
        var useCaseAttribute = useCaseType.GetCustomAttribute<SsalddelUseCaseAttribute>(inherit: true);

        Assert.Contains(workflowAttributes, attribute => attribute.Workflow == workflow);
        Assert.NotNull(useCaseAttribute);
        Assert.False(string.IsNullOrWhiteSpace(useCaseAttribute!.Name));
    }

    private static void AssertUseCaseHasRelation(Type useCaseType, SsalddelUseCaseRelationKind kind, string targetUseCaseCode)
    {
        var relationAttributes = useCaseType.GetCustomAttributes<SsalddelUseCaseRelationAttribute>(inherit: true);
        Assert.Contains(relationAttributes, attribute =>
            attribute.Kind == kind &&
            attribute.TargetUseCaseCode == targetUseCaseCode &&
            !string.IsNullOrWhiteSpace(attribute.Summary));
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
                [VersionFeatureFlagKeys.SsalddelMartWorkflow] = false
            };
        }
    }
}
