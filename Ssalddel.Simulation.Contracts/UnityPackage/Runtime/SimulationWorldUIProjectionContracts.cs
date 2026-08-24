using System;
using System.Collections.Generic;

namespace Ssalddel.Simulation.Contracts
{
public static class SimulationWorldUIExecutionModeCodes
{
    public const string Simulation = "Simulation";
}

public static class SimulationWorldUIDesignProfileCodes
{
    public const string FigmaMauiWarehouseV1 = "figma-maui-warehouse.v1";
}

public static class SimulationWorldUILayoutProfileCodes
{
    public const string WorldSidePanel = "WorldSidePanel";
}

public static class SimulationWorldUIStyleSemanticKeys
{
    public const string Warehouse = "Role.Warehouse";
    public const string NeutralState = "State.Neutral";
    public const string Information = "Information.Default";
    public const string Evidence = "Information.Evidence";
    public const string Limitation = "Information.Limitation";
    public const string InspectAction = "Action.Secondary";
    public const string PreviewAction = "Action.Preview";
    public const string ConfirmAction = "Action.Confirm";
}

public sealed class SimulationWorldUIProjection
{
    public string UI기획개정번호 { get; set; } = string.Empty;
    public string 업무규칙대장개정번호 { get; set; } = string.Empty;
    public string DesignProfileRevision { get; set; } = string.Empty;
    public string SessionStableId { get; set; } = string.Empty;
    public long StateRevision { get; set; }
    public long WorldTick { get; set; }
    public string SurfaceStableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string SurfaceKindCode { get; set; } = string.Empty;
    public string LayoutProfileCode { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleStyleSemanticKey { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowStageCode { get; set; } = string.Empty;
    public string ExecutionModeCode { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string KoreanTitle { get; set; } = string.Empty;
    public string StateKoreanLabel { get; set; } = string.Empty;
    public string PresentationIntentCode { get; set; } = string.Empty;
    public string StateStyleSemanticKey { get; set; } = string.Empty;
    public DateTimeOffset ProjectedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorldUIProjectionItem> InformationItems { get; set; } =
        Array.Empty<SimulationWorldUIProjectionItem>();
    public IReadOnlyList<SimulationWorldUIProjectionAction> Actions { get; set; } =
        Array.Empty<SimulationWorldUIProjectionAction>();
    public IReadOnlyList<SimulationWorldUIProjectionRuleEvidence> RuleEvidence { get; set; } =
        Array.Empty<SimulationWorldUIProjectionRuleEvidence>();
}

public sealed class SimulationWorldUIProjectionItem
{
    public string StableId { get; set; } = string.Empty;
    public string InformationKindCode { get; set; } = string.Empty;
    public string KoreanLabel { get; set; } = string.Empty;
    public string StyleSemanticKey { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public string? UnitCode { get; set; }
    public string DataStatusCode { get; set; } = string.Empty;
    public string? SourceStableId { get; set; }
    public DateTimeOffset? ObservedAtUtc { get; set; }
    public string? LimitationCode { get; set; }
}

public sealed class SimulationWorldUIProjectionAction
{
    public string StableId { get; set; } = string.Empty;
    public string ActionKindCode { get; set; } = string.Empty;
    public string KoreanLabel { get; set; } = string.Empty;
    public string StyleSemanticKey { get; set; } = string.Empty;
    public string CapabilityKey { get; set; } = string.Empty;
    public string CanonicalActionCode { get; set; } = string.Empty;
    public string? ServerCommandKey { get; set; }
    public bool Enabled { get; set; }
    public string? BlockReasonCode { get; set; }
    public bool RequiresPreview { get; set; }
    public bool RequiresExplicitConfirmation { get; set; }
    public bool RequiresExpectedRevision { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string RouteTemplate { get; set; } = string.Empty;
    public string? RequestContractKey { get; set; }
    public string ResponseContractKey { get; set; } = string.Empty;
    public string CanonicalRequeryRouteTemplate { get; set; } = string.Empty;
    public SimulationWorldUIActionInvocation? Invocation { get; set; }
}

public sealed class SimulationWorldUIActionInvocation
{
    public string? TargetStableId { get; set; }
    public long? TargetRevision { get; set; }
    public string? ActorStableId { get; set; }
    public long? ExpectedStateRevision { get; set; }
    public int? DurationTicks { get; set; }
    public IReadOnlyList<string> SourceStableIds { get; set; } = Array.Empty<string>();
}

public sealed class SimulationWorldUIProjectionRuleEvidence
{
    public string UI규칙연결StableId { get; set; } = string.Empty;
    public string BusinessRuleBindingStableId { get; set; } = string.Empty;
    public string FacilityCapabilityCode { get; set; } = string.Empty;
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
}
}
