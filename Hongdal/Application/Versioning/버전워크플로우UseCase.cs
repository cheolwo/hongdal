using System.Reflection;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Versioning;
using 홍달.Services.Versioning;

namespace Hongdal.Application.Versioning;

public interface I버전워크플로우UseCase
{
    VersionFeatureFlagsResponse 조회();
}

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("버전 워크플로우 조회", Summary = "버전 플래그, 워크플로우 관계, 참여자, 화면, 유스케이스 메타데이터를 조회합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
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
            WorkflowRelations = HongdalWorkflowRelations.GetAll().Select(ToDto).ToArray()
        };
    }

    private static IReadOnlyList<WorkflowFlagStateDto> BuildWorkflowStates(IReadOnlyDictionary<string, bool> flags)
    {
        return
        [
            ToWorkflowState(HongdalWorkflow.DomesticTransport, VersionFeatureFlagKeys.DomesticTransportWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.WarehouseFulfillment, VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.CustomsAndTradeData, VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.GroupPurchaseImport, VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.SalesChannelFulfillment, VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.CommunityTrust, VersionFeatureFlagKeys.CommunityTrustWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.HrParticipation, VersionFeatureFlagKeys.HrParticipationWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.FoodDelivery, VersionFeatureFlagKeys.FoodDeliveryWorkflow, flags),
            ToWorkflowState(HongdalWorkflow.HongdalMart, VersionFeatureFlagKeys.HongdalMartWorkflow, flags)
        ];
    }

    private static WorkflowFlagStateDto ToWorkflowState(
        HongdalWorkflow workflow,
        string flagKey,
        IReadOnlyDictionary<string, bool> flags)
    {
        return new WorkflowFlagStateDto
        {
            WorkflowCode = workflow.ToString(),
            WorkflowName = HongdalWorkflowLabels.GetLabel(workflow),
            FlagKey = flagKey,
            IsEnabled = flags.TryGetValue(flagKey, out var enabled) && enabled,
            BoundarySummary = HongdalWorkflowParticipants.GetBoundarySummary(workflow),
            Participants = HongdalWorkflowParticipants.GetByWorkflow(workflow).Select(ToParticipantDto).ToArray(),
            Screens = HongdalWorkflowScreens.GetByWorkflow(workflow).Select(ToScreenDto).ToArray(),
            UseCases = BuildUseCases(workflow)
        };
    }

    private static IReadOnlyList<WorkflowUseCaseDto> BuildUseCases(HongdalWorkflow workflow)
    {
        return typeof(버전워크플로우UseCase).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.Name.EndsWith("UseCase", StringComparison.Ordinal))
            .Where(type => type
                .GetCustomAttributes<HongdalApiWorkflowAttribute>(inherit: true)
                .Any(attribute => attribute.Workflow == workflow))
            .Select(ToUseCaseDto)
            .OrderBy(useCase => useCase.UseCaseCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static WorkflowUseCaseDto ToUseCaseDto(Type useCaseType)
    {
        var useCase = useCaseType.GetCustomAttribute<HongdalUseCaseAttribute>(inherit: true);
        var actors = useCaseType
            .GetCustomAttributes<HongdalUseCaseActorAttribute>(inherit: true)
            .Select(ToUseCaseActorDto)
            .ToArray();
        var relations = useCaseType
            .GetCustomAttributes<HongdalUseCaseRelationAttribute>(inherit: true)
            .Select(ToUseCaseRelationDto)
            .ToArray();

        return new WorkflowUseCaseDto
        {
            UseCaseCode = useCaseType.Name,
            UseCaseName = useCase?.Name ?? useCaseType.Name,
            Summary = useCase?.Summary ?? string.Empty,
            IsRequired = useCase?.IsRequired ?? true,
            PrimaryActors = actors
                .Where(actor => actor.RoleCode == HongdalUseCaseActorRole.Primary.ToString())
                .ToArray(),
            SupportingActors = actors
                .Where(actor => actor.RoleCode == HongdalUseCaseActorRole.Supporting.ToString())
                .ToArray(),
            Relations = relations
        };
    }

    private static WorkflowUseCaseActorDto ToUseCaseActorDto(HongdalUseCaseActorAttribute actor)
    {
        return new WorkflowUseCaseActorDto
        {
            ActorCode = actor.ActorCode,
            ActorName = actor.ActorLabel,
            RoleCode = actor.Role.ToString(),
            RoleName = actor.RoleLabel
        };
    }

    private static WorkflowUseCaseRelationDto ToUseCaseRelationDto(HongdalUseCaseRelationAttribute relation)
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

    private static WorkflowParticipantDto ToParticipantDto(HongdalWorkflowParticipant participant)
    {
        return new WorkflowParticipantDto
        {
            ActorCode = participant.ActorCode,
            ActorName = participant.ActorName,
            IsPrimary = participant.IsPrimary,
            Responsibility = participant.Responsibility
        };
    }

    private static WorkflowScreenDto ToScreenDto(HongdalWorkflowScreen screen)
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

    private static WorkflowRelationDto ToDto(HongdalWorkflowRelation relation)
    {
        return new WorkflowRelationDto
        {
            SourceWorkflowCode = relation.Source.ToString(),
            SourceWorkflowName = HongdalWorkflowLabels.GetLabel(relation.Source),
            TargetWorkflowCode = relation.Target.ToString(),
            TargetWorkflowName = HongdalWorkflowLabels.GetLabel(relation.Target),
            RelationKindCode = relation.Kind.ToString(),
            RelationKindName = HongdalWorkflowRelationKindLabels.GetLabel(relation.Kind),
            Summary = relation.Summary
        };
    }
}
