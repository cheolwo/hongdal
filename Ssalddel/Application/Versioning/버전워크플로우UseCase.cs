using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using 살뜰.Services.Versioning;

namespace Ssalddel.Application.Versioning;

public interface I버전워크플로우UseCase
{
    VersionFeatureFlagsResponse 조회();
}

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase("버전 워크플로우 조회", Summary = "버전 플래그, 워크플로우 관계, 참여자, 화면, 유스케이스 메타데이터를 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator)]
public sealed class 버전워크플로우UseCase : I버전워크플로우UseCase
{
    private readonly IVersionFeatureFlagService _featureFlagService;

    public 버전워크플로우UseCase(IVersionFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public VersionFeatureFlagsResponse 조회()
    {
        var flags = _featureFlagService.GetAll();
        return new VersionFeatureFlagsResponse
        {
            Flags = flags,
            Workflows = BuildWorkflowStates(flags),
            WorkflowRelations = SsalddelWorkflowRelations.GetAll().Select(ToDto).ToArray(),
            OperatingSystems = SsalddelOperatingSystems.GetAll().Select(item => ToDto(item, flags)).ToArray(),
            ApiEndpoints = BuildApiEndpoints(flags),
            PageCapabilities = BuildPageCapabilities(flags)
        };
    }

    private static IReadOnlyList<PageCapabilityDto> BuildPageCapabilities(
        IReadOnlyDictionary<string, bool> flags)
    {
        return SsalddelPageCapabilityCatalog.GetAll()
            .Select(capability => new PageCapabilityDto
            {
                PageKey = capability.PageKey,
                AppCode = capability.AppCode,
                RoutePattern = capability.RoutePattern,
                MatchKindCode = capability.MatchKind.ToString(),
                StageCode = capability.Stage.ToString(),
                StageName = PageCapabilityLabels.StageName(capability.Stage),
                BoundaryCode = capability.Boundary.ToString(),
                BoundaryName = PageCapabilityLabels.BoundaryName(capability.Boundary),
                RequiresAuthentication = capability.RequiresAuthentication,
                HasExternalEffects = capability.HasExternalEffects,
                IntroducedVersion = capability.IntroducedVersion,
                FeatureKeys = capability.FeatureKeys,
                IsFeatureEnabled = capability.FeatureKeys.Count == 0
                    || capability.FeatureKeys.Any(featureKey =>
                        flags.TryGetValue(featureKey, out var enabled) && enabled),
                WorkflowCodes = capability.WorkflowCodes,
                Notice = capability.Notice
            })
            .OrderBy(capability => capability.AppCode, StringComparer.Ordinal)
            .ThenBy(capability => capability.RoutePattern, StringComparer.Ordinal)
            .ThenBy(capability => capability.PageKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<WorkflowFlagStateDto> BuildWorkflowStates(IReadOnlyDictionary<string, bool> flags)
    {
        return
        [
            ToWorkflowState(SsalddelWorkflow.DomesticTransport, VersionFeatureFlagKeys.DomesticTransportWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.WarehouseFulfillment, VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.CustomsAndTradeData, VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.GroupPurchaseDemand, VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.GroupPurchaseImport, VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.SalesChannelFulfillment, VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.CommunityTrust, VersionFeatureFlagKeys.CommunityTrustWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.HrParticipation, VersionFeatureFlagKeys.HrParticipationWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.FoodDelivery, VersionFeatureFlagKeys.FoodDeliveryWorkflow, flags),
            ToWorkflowState(SsalddelWorkflow.SsalddelMart, VersionFeatureFlagKeys.SsalddelMartWorkflow, flags)
        ];
    }

    private static WorkflowFlagStateDto ToWorkflowState(
        SsalddelWorkflow workflow,
        string flagKey,
        IReadOnlyDictionary<string, bool> flags)
    {
        return new WorkflowFlagStateDto
        {
            WorkflowCode = workflow.ToString(),
            WorkflowName = SsalddelWorkflowLabels.GetLabel(workflow),
            FlagKey = flagKey,
            IsEnabled = flags.TryGetValue(flagKey, out var enabled) && enabled,
            BoundarySummary = SsalddelWorkflowParticipants.GetBoundarySummary(workflow),
            Participants = SsalddelWorkflowParticipants.GetByWorkflow(workflow).Select(ToParticipantDto).ToArray(),
            Screens = SsalddelWorkflowScreens.GetByWorkflow(workflow).Select(ToScreenDto).ToArray(),
            UseCases = BuildUseCases(workflow)
        };
    }

    private static IReadOnlyList<WorkflowUseCaseDto> BuildUseCases(SsalddelWorkflow workflow)
    {
        return typeof(버전워크플로우UseCase).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.Name.EndsWith("UseCase", StringComparison.Ordinal))
            .Where(type => type
                .GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true)
                .Any(attribute => attribute.Workflow == workflow))
            .Select(ToUseCaseDto)
            .OrderBy(useCase => useCase.UseCaseCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static WorkflowUseCaseDto ToUseCaseDto(Type useCaseType)
    {
        var useCase = useCaseType.GetCustomAttribute<SsalddelUseCaseAttribute>(inherit: true);
        var actors = useCaseType
            .GetCustomAttributes<SsalddelUseCaseActorAttribute>(inherit: true)
            .Select(ToUseCaseActorDto)
            .ToArray();
        var relations = useCaseType
            .GetCustomAttributes<SsalddelUseCaseRelationAttribute>(inherit: true)
            .Select(ToUseCaseRelationDto)
            .ToArray();

        return new WorkflowUseCaseDto
        {
            UseCaseCode = useCaseType.Name,
            UseCaseName = useCase?.Name ?? useCaseType.Name,
            Summary = useCase?.Summary ?? string.Empty,
            IsRequired = useCase?.IsRequired ?? true,
            PrimaryActors = actors
                .Where(actor => actor.RoleCode == SsalddelUseCaseActorRole.Primary.ToString())
                .ToArray(),
            SupportingActors = actors
                .Where(actor => actor.RoleCode == SsalddelUseCaseActorRole.Supporting.ToString())
                .ToArray(),
            Relations = relations
        };
    }

    private static WorkflowUseCaseActorDto ToUseCaseActorDto(SsalddelUseCaseActorAttribute actor)
    {
        return new WorkflowUseCaseActorDto
        {
            ActorCode = actor.ActorCode,
            ActorName = actor.ActorLabel,
            RoleCode = actor.Role.ToString(),
            RoleName = actor.RoleLabel
        };
    }

    private static WorkflowUseCaseRelationDto ToUseCaseRelationDto(SsalddelUseCaseRelationAttribute relation)
    {
        return new WorkflowUseCaseRelationDto
        {
            RelationKindCode = relation.Kind.ToString(),
            RelationKindName = relation.KindLabel,
            TargetUseCaseCode = relation.TargetUseCaseCode,
            Condition = relation.Condition,
            Summary = relation.Summary
        };
    }

    private static IReadOnlyList<WorkflowApiEndpointDto> BuildApiEndpoints(
        IReadOnlyDictionary<string, bool> flags)
    {
        return typeof(버전워크플로우UseCase).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type)
                && !type.IsAbstract
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .SelectMany(type => BuildControllerEndpoints(type, flags))
            .OrderBy(endpoint => endpoint.RoutePattern, StringComparer.Ordinal)
            .ThenBy(endpoint => endpoint.Method, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<WorkflowApiEndpointDto> BuildControllerEndpoints(
        Type controllerType,
        IReadOnlyDictionary<string, bool> flags)
    {
        var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>(inherit: true)?.Template ?? string.Empty;
        var controllerVersion = controllerType.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
        var controllerWorkflows = controllerType.GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true).ToArray();
        var controllerGrowthTracks = controllerType.GetCustomAttributes<SsalddelApiGrowthTrackAttribute>(inherit: true).ToArray();
        var controllerAuthorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToArray();
        var controllerAllowsAnonymous = controllerType.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any();

        foreach (var action in GetActionMethods(controllerType))
        {
            var declaredActionVersion = action.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
            var actionVersion = declaredActionVersion ?? controllerVersion;
            var featureKey = !string.IsNullOrWhiteSpace(declaredActionVersion?.FeatureKey)
                ? declaredActionVersion.FeatureKey
                : controllerVersion?.FeatureKey;
            var actionWorkflows = action.GetCustomAttributes<SsalddelApiWorkflowAttribute>(inherit: true).DefaultIfEmpty().Where(x => x is not null).Cast<SsalddelApiWorkflowAttribute>().ToArray();
            if (actionWorkflows.Length == 0)
            {
                actionWorkflows = controllerWorkflows;
            }

            var actionGrowthTracks = action.GetCustomAttributes<SsalddelApiGrowthTrackAttribute>(inherit: true).DefaultIfEmpty().Where(x => x is not null).Cast<SsalddelApiGrowthTrackAttribute>().ToArray();
            if (actionGrowthTracks.Length == 0)
            {
                actionGrowthTracks = controllerGrowthTracks;
            }

            var authorizeAttributes = controllerAuthorize
                .Concat(action.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
                .ToArray();
            var allowsAnonymous = controllerAllowsAnonymous ||
                action.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any();

            foreach (var httpAttribute in action.GetCustomAttributes<HttpMethodAttribute>(inherit: true))
            {
                foreach (var method in httpAttribute.HttpMethods)
                {
                    yield return new WorkflowApiEndpointDto
                    {
                        EndpointKey = $"{controllerType.Name}.{action.Name}",
                        ControllerName = controllerType.Name,
                        ActionName = action.Name,
                        Method = method,
                        RoutePattern = CombineRoutes(controllerRoute, httpAttribute.Template),
                        ProductVersionCode = actionVersion?.Version.ToString() ?? string.Empty,
                        ProductVersionName = actionVersion?.VersionLabel ?? string.Empty,
                        ProductName = actionVersion?.ProductName ?? string.Empty,
                        ProductVersionDisplayName = actionVersion?.VersionDisplayName ?? string.Empty,
                        FeatureKey = featureKey ?? string.Empty,
                        IsEnabled = string.IsNullOrWhiteSpace(featureKey)
                            || flags.TryGetValue(featureKey, out var enabled) && enabled,
                        WorkflowCodes = actionWorkflows.Select(attribute => attribute.Workflow.ToString()).Distinct(StringComparer.Ordinal).ToArray(),
                        WorkflowNames = actionWorkflows.Select(attribute => attribute.WorkflowLabel).Distinct(StringComparer.Ordinal).ToArray(),
                        GrowthTrackCodes = actionGrowthTracks.Select(attribute => attribute.Track.ToString()).Distinct(StringComparer.Ordinal).ToArray(),
                        GrowthTrackNames = actionGrowthTracks.Select(attribute => attribute.TrackLabel).Distinct(StringComparer.Ordinal).ToArray(),
                        AuthorizationPolicy = string.Join(", ", authorizeAttributes.Select(x => x.Policy).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal)),
                        AuthorizationRoles = string.Join(", ", authorizeAttributes.Select(x => x.Roles).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal)),
                        AllowsAnonymous = allowsAnonymous
                    };
                }
            }
        }
    }

    private static IEnumerable<MethodInfo> GetActionMethods(Type controllerType)
    {
        return controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any());
    }

    private static string CombineRoutes(string controllerRoute, string? actionRoute)
    {
        var left = (controllerRoute ?? string.Empty).Trim('/');
        var right = (actionRoute ?? string.Empty).Trim('/');

        if (string.IsNullOrWhiteSpace(left))
        {
            return right;
        }

        return string.IsNullOrWhiteSpace(right) ? left : $"{left}/{right}";
    }

    private static WorkflowParticipantDto ToParticipantDto(SsalddelWorkflowParticipant participant)
    {
        return new WorkflowParticipantDto
        {
            ActorCode = participant.ActorCode,
            ActorName = participant.ActorName,
            IsPrimary = participant.IsPrimary,
            Responsibility = participant.Responsibility
        };
    }

    private static WorkflowScreenDto ToScreenDto(SsalddelWorkflowScreen screen)
    {
        return new WorkflowScreenDto
        {
            ActorCode = screen.ActorCode,
            AppCode = screen.AppCode,
            AppName = screen.AppName,
            ScreenName = screen.ScreenName,
            Route = screen.Route,
            Purpose = screen.Purpose
        };
    }

    private static WorkflowRelationDto ToDto(SsalddelWorkflowRelation relation)
    {
        return new WorkflowRelationDto
        {
            SourceWorkflowCode = relation.Source.ToString(),
            SourceWorkflowName = SsalddelWorkflowLabels.GetLabel(relation.Source),
            TargetWorkflowCode = relation.Target.ToString(),
            TargetWorkflowName = SsalddelWorkflowLabels.GetLabel(relation.Target),
            RelationKindCode = relation.Kind.ToString(),
            RelationKindName = SsalddelWorkflowRelationKindLabels.GetLabel(relation.Kind),
            Summary = relation.Summary
        };
    }

    private static OperatingSystemDto ToDto(
        SsalddelOperatingSystemDefinition operatingSystem,
        IReadOnlyDictionary<string, bool> flags)
    {
        var canonicalId = SsalddelOperatingSystems.GetCanonicalId(operatingSystem.OperatingSystem);
        var featureKey = GetOperatingSystemFeatureKey(operatingSystem.OperatingSystem);
        return new OperatingSystemDto
        {
            OperatingSystemCode = operatingSystem.OperatingSystem.ToString(),
            CanonicalOperatingSystemId = canonicalId,
            OperatingSystemAliases = OperatingSystemIds.GetAliases(canonicalId),
            OperatingSystemName = operatingSystem.Name,
            Purpose = operatingSystem.Purpose,
            FeatureKey = featureKey ?? string.Empty,
            IsEnabled = featureKey is null || flags.TryGetValue(featureKey, out var enabled) && enabled,
            Workflows = operatingSystem.Workflows.Select(ToOperatingSystemWorkflowDto).ToArray(),
            Engines = operatingSystem.Engines.Select(ToOperatingSystemEngineDto).ToArray(),
            SchedulingPolicies = operatingSystem.SchedulingPolicies.Select(ToOperatingSystemSchedulingPolicyDto).ToArray()
        };
    }

    private static string? GetOperatingSystemFeatureKey(SsalddelOperatingSystem operatingSystem)
        => operatingSystem switch
        {
            SsalddelOperatingSystem.DomesticCargoTransport => VersionFeatureFlagKeys.DomesticTransportWorkflow,
            SsalddelOperatingSystem.WarehouseCommerceFulfillment => VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
            SsalddelOperatingSystem.GroupPurchaseDemand => VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
            SsalddelOperatingSystem.GroupPurchaseImport => VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
            SsalddelOperatingSystem.FoodDelivery => VersionFeatureFlagKeys.FoodDeliveryWorkflow,
            SsalddelOperatingSystem.SsalddelMartUrbanLogistics => VersionFeatureFlagKeys.SsalddelMartWorkflow,
            SsalddelOperatingSystem.CommunityTrust => VersionFeatureFlagKeys.CommunityTrustWorkflow,
            SsalddelOperatingSystem.PlatformOperations => null,
            _ => throw new ArgumentOutOfRangeException(nameof(operatingSystem), operatingSystem, "Unknown Ssalddel operating system.")
        };

    private static OperatingSystemWorkflowDto ToOperatingSystemWorkflowDto(SsalddelWorkflow workflow)
    {
        return new OperatingSystemWorkflowDto
        {
            WorkflowCode = workflow.ToString(),
            WorkflowName = SsalddelWorkflowLabels.GetLabel(workflow)
        };
    }

    private static OperatingSystemEngineDto ToOperatingSystemEngineDto(SsalddelOperatingSystemEngine engine)
    {
        var implementationIds = EngineImplementationCatalog.GetAll()
            .Where(binding => string.Equals(binding.EngineFamilyId, engine.EngineCode, StringComparison.Ordinal))
            .Select(binding => binding.ImplementationId)
            .ToArray();

        return new OperatingSystemEngineDto
        {
            EngineCode = engine.EngineCode,
            EngineFamilyId = engine.EngineCode,
            ImplementationIds = implementationIds,
            RuntimeStatus = implementationIds.Length > 0
                ? RuntimeCapabilityStatuses.Active
                : RuntimeCapabilityStatuses.Declared,
            EngineName = engine.EngineName,
            AdjustmentPolicy = engine.AdjustmentPolicy
        };
    }

    private static OperatingSystemSchedulingPolicyDto ToOperatingSystemSchedulingPolicyDto(SsalddelSchedulingPolicy policy)
    {
        return new OperatingSystemSchedulingPolicyDto
        {
            RuntimeStatus = SchedulingPolicyImplementationCatalog.IsActive(policy.PolicyCode)
                ? RuntimeCapabilityStatuses.Active
                : RuntimeCapabilityStatuses.Declared,
            PolicyKindCode = policy.Kind.ToString(),
            PolicyKindName = SsalddelSchedulingPolicyKindLabels.GetLabel(policy.Kind),
            PolicyCode = policy.PolicyCode,
            PolicyName = policy.PolicyName,
            TargetQueue = policy.TargetQueue,
            AppliedEngineCode = policy.AppliedEngineCode,
            Rule = policy.Rule,
            StarvationGuard = policy.StarvationGuard
        };
    }
}
