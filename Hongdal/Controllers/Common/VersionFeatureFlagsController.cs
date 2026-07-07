using Hongdal.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/version-feature-flags")]
public sealed class VersionFeatureFlagsController : ControllerBase
{
    private readonly IVersionFeatureFlagService _featureFlagService;

    public VersionFeatureFlagsController(IVersionFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    [HttpGet]
    public ActionResult<VersionFeatureFlagsResponse> Get()
    {
        var flags = _featureFlagService.GetAll();
        return Ok(new VersionFeatureFlagsResponse
        {
            Flags = flags,
            Workflows = BuildWorkflowStates(flags),
            WorkflowRelations = HongdalWorkflowRelations.GetAll().Select(ToDto).ToArray()
        });
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
            Screens = HongdalWorkflowScreens.GetByWorkflow(workflow).Select(ToScreenDto).ToArray()
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
