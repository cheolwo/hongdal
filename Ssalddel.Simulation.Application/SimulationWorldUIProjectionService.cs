using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorldUIProjectionService
{
    public const string 진부면물류거점입출고SurfaceStableId =
        "ui-surface:sim:pyeongchang:hub-operations";
    public const string SurfaceNotFoundCode = "SimulationWorldUiSurfaceNotFound";
    public const string InboundFreightNotReadyCode =
        "SimulationWorldUiInboundFreightNotReady";
    public const string InboundInspectorNotAvailableCode =
        "SimulationWorldUiInboundInspectorNotAvailable";
    public const string PutAwayInventoryNotReadyCode =
        "SimulationWorldUiPutAwayInventoryNotReady";
    public const string PutAwayOperatorNotAvailableCode =
        "SimulationWorldUiPutAwayOperatorNotAvailable";

    private const string ProjectionRoute =
        "/api/simulation/v1/sessions/{sessionStableId}/world-ui/surfaces/{surfaceStableId}";
    private const string PreviewRoute =
        "/api/simulation/v1/sessions/{sessionStableId}/freight-receipt-previews";
    private const string ConfirmRoute =
        "/api/simulation/v1/sessions/{sessionStableId}/freight-receipts/confirm";
    private const string PutAwayPreviewRoute =
        "/api/simulation/v1/sessions/{sessionStableId}/warehouse-put-away-previews";
    private const string PutAwayConfirmRoute =
        "/api/simulation/v1/sessions/{sessionStableId}/warehouse-put-aways/confirm";

    private readonly 경영SimulationSessionService _sessionService;
    private readonly SimulationWorld업무규칙집결원장 _businessRules;
    private readonly SimulationWorldUI기획원장 _uiPlan;

    public SimulationWorldUIProjectionService(경영SimulationSessionService sessionService)
    {
        _sessionService = sessionService;
        _businessRules = PyeongchangSimulationWorld업무규칙CatalogFactory.Create(
            "world-build:runtime:pyeongchang",
            new string('0', 64),
            "area-set:sim:pyeongchang:farm-hub-town.v1");
        _uiPlan = PyeongchangSimulationWorldUI기획Factory.Create(_businessRules);
    }

    public SimulationWorldUIProjection Get(string sessionStableId, string surfaceStableId)
    {
        var surface = _uiPlan.Surfaces.SingleOrDefault(value =>
            string.Equals(value.StableId, surfaceStableId, StringComparison.Ordinal));
        if (surface == null || !string.Equals(
                surface.StableId,
                진부면물류거점입출고SurfaceStableId,
                StringComparison.Ordinal))
        {
            throw new SimulationNotFoundException(SurfaceNotFoundCode);
        }

        var session = _sessionService.Get(sessionStableId);
        var freightAtFacility = (
            from freight in session.FreightTransports
            join movement in session.LogisticsMovements
                on freight.LogisticsTaskStableId equals movement.TaskStableId
            where string.Equals(
                movement.DestinationFacilityStableId,
                surface.FacilityStableId,
                StringComparison.Ordinal)
            select freight).ToArray();
        var receivableFreight = freightAtFacility
            .Where(value => value.StateCode == 화물운송상태코드.하차지도착
                && string.IsNullOrWhiteSpace(value.ReceiptTaskStableId))
            .OrderBy(value => value.TransportRequestStableId, StringComparer.Ordinal)
            .FirstOrDefault();
        var inspectorAvailable = session.NpcActors.Any(value => string.Equals(
            value.ActorStableId,
            PyeongchangSimulationNpcStableIds.진부입고검수담당,
            StringComparison.Ordinal));
        var assignments = session.NpcTaskAssignments
            .Where(value => string.Equals(
                value.FacilityStableId,
                surface.FacilityStableId,
                StringComparison.Ordinal))
            .ToArray();
        var inventories = session.NpcFacilityInventories
            .Where(value => string.Equals(
                value.FacilityStableId,
                surface.FacilityStableId,
                StringComparison.Ordinal))
            .ToArray();
        var activePutAwayInventoryIds = ActivePutAwayInventoryIds(session);
        var putAwayCandidate = inventories
            .Where(value => value.StateCode == SimulationNpcInventoryStateCodes.StorageEligible
                && !activePutAwayInventoryIds.Contains(value.InventoryStableId))
            .OrderBy(value => value.InventoryStableId, StringComparer.Ordinal)
            .FirstOrDefault();
        var putAwayInProgress = assignments.Any(value =>
            value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove
            && IsActivePhase(value.PhaseCode));
        var putAwayFlow = putAwayCandidate != null
            || putAwayInProgress
            || (receivableFreight == null && inventories.Any(value =>
                value.StateCode == SimulationNpcInventoryStateCodes.PutAwayCompleted));
        var putAwayOperatorAvailable = session.NpcCapabilityGrants.Any(value =>
            value.Active
            && value.ActorStableId == PyeongchangSimulationNpcStableIds.진부적재담당
            && value.FacilityStableId == surface.FacilityStableId
            && value.CapabilityCode == SimulationNpcCapabilityCodes.WarehouseStorageMove);
        var workflowStageCode = ResolveWorkflowStageCode(
            assignments,
            inventories,
            receivableFreight);
        var stateCode = ResolveStateCode(
            assignments,
            inventories,
            receivableFreight,
            putAwayCandidate);
        var statePlan = _uiPlan.StatePresentations.Single(value =>
            value.SurfaceStableId == surface.StableId && value.StateCode == stateCode);
        var actionBlockReason = putAwayFlow
            ? putAwayCandidate == null
                ? PutAwayInventoryNotReadyCode
                : !putAwayOperatorAvailable
                    ? PutAwayOperatorNotAvailableCode
                    : null
            : receivableFreight == null
                ? InboundFreightNotReadyCode
                : !inspectorAvailable
                    ? InboundInspectorNotAvailableCode
                    : null;

        return new SimulationWorldUIProjection
        {
            UI기획개정번호 = _uiPlan.CatalogRevision,
            업무규칙대장개정번호 = _businessRules.CatalogRevision,
            DesignProfileRevision = SimulationWorldUIDesignProfileCodes.FigmaMauiWarehouseV1,
            SessionStableId = session.SessionStableId,
            StateRevision = session.Revision,
            WorldTick = session.WorldContext.WorldTick,
            SurfaceStableId = surface.StableId,
            FacilityStableId = surface.FacilityStableId,
            SurfaceKindCode = surface.SurfaceKindCode,
            LayoutProfileCode = SimulationWorldUILayoutProfileCodes.WorldSidePanel,
            RoleCode = surface.RoleCode,
            RoleStyleSemanticKey = SimulationWorldUIStyleSemanticKeys.Warehouse,
            WorkflowCode = 업무흐름코드.창고입고,
            WorkflowStageCode = workflowStageCode,
            ExecutionModeCode = SimulationWorldUIExecutionModeCodes.Simulation,
            StateCode = stateCode,
            KoreanTitle = surface.KoreanTitle,
            StateKoreanLabel = statePlan.KoreanLabel,
            PresentationIntentCode = statePlan.PresentationIntentCode,
            StateStyleSemanticKey = "State." + statePlan.PresentationIntentCode,
            ProjectedAtUtc = DateTimeOffset.UtcNow,
            InformationItems = ProjectInformation(
                surface,
                session,
                statePlan.KoreanLabel,
                freightAtFacility,
                receivableFreight,
                assignments,
                inventories,
                putAwayCandidate),
            Actions = ProjectActions(
                surface,
                session,
                receivableFreight,
                putAwayCandidate,
                putAwayFlow,
                actionBlockReason),
            RuleEvidence = _uiPlan.RuleBindings
                .Where(value => value.SurfaceStableId == surface.StableId)
                .OrderByDescending(value => value.Priority)
                .Select(value => new SimulationWorldUIProjectionRuleEvidence
                {
                    UI규칙연결StableId = value.StableId,
                    BusinessRuleBindingStableId = value.BusinessRuleBindingStableId,
                    FacilityCapabilityCode = value.FacilityCapabilityCode,
                    RuleStableId = value.RuleStableId,
                    RuleRevision = value.RuleRevision,
                })
                .ToArray(),
        };
    }

    private IReadOnlyList<SimulationWorldUIProjectionItem> ProjectInformation(
        SimulationWorldUI화면영역기획 surface,
        경영SimulationSessionSnapshot session,
        string stateKoreanLabel,
        IReadOnlyCollection<SimulationFreightTransportSnapshot> freightAtFacility,
        SimulationFreightTransportSnapshot? receivableFreight,
        IReadOnlyCollection<SimulationNpcTaskAssignmentSnapshot> assignments,
        IReadOnlyCollection<SimulationNpcFacilityInventorySnapshot> inventories,
        SimulationNpcFacilityInventorySnapshot? putAwayCandidate)
    {
        var active = assignments.FirstOrDefault(value =>
            IsActivePhase(value.PhaseCode));
        var pendingInspection = inventories.Count(value =>
            value.StateCode == SimulationNpcInventoryStateCodes.PendingInspection);
        var putAwayPending = inventories.Count(value =>
            value.StateCode == SimulationNpcInventoryStateCodes.StorageEligible);
        var putAwayCompleted = inventories.Count(value =>
            value.StateCode == SimulationNpcInventoryStateCodes.PutAwayCompleted);
        var activePutAway = active?.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove;
        var evidenceRule = putAwayCandidate != null || activePutAway || putAwayCompleted > 0
            ? PyeongchangSimulationWorldStableIds.창고적재규칙
            : PyeongchangSimulationWorldStableIds.창고입고검수규칙;
        var nextStep = putAwayCandidate != null
            ? "적재 미리보기를 확인한 뒤 같은 입고 재고를 확정"
            : receivableFreight != null
                ? "입고 검수 미리보기를 확인한 뒤 확정"
                : active != null
                    ? activePutAway
                        ? "NPC 적재 작업의 WorldTick 진행을 기다림"
                        : "NPC 검수 작업의 WorldTick 진행을 기다림"
                    : putAwayCompleted > 0
                        ? "적재 완료된 재고를 확인"
                        : "도착 화물 대기";
        var values = new Dictionary<string, (string Value, string Status, string? Source, string? Limitation)>(StringComparer.Ordinal)
        {
            ["Summary"] = ($"도착 화물 {freightAtFacility.Count}건 · 검수 대기 {pendingInspection}건 · 적재 대기 {putAwayPending}건 · 적재 완료 {putAwayCompleted}건", "Derived", surface.FacilityStableId, null),
            ["Status"] = (active == null ? stateKoreanLabel : stateKoreanLabel + " · " + PhaseKorean(active.ActionCode, active.PhaseCode), "Derived", active?.AssignmentStableId ?? surface.FacilityStableId, null),
            ["NextStep"] = (nextStep, "Derived", putAwayCandidate?.InventoryStableId ?? receivableFreight?.TransportRequestStableId, null),
            ["Evidence"] = ($"{evidenceRule} · {_businessRules.CatalogRevision}", "Derived", evidenceRule, null),
            ["Limitation"] = ("Simulation 상태 표현이며 실제 입고·검수·재고 업무를 변경하지 않음", "Scenario", session.ScenarioStableId, "SimulationOnly"),
            ["Refresh"] = ($"상태 사본 r{session.Revision} · WorldTick {session.WorldContext.WorldTick}", "Observed", session.SessionStableId, null),
        };
        return _uiPlan.InformationItems
            .Where(value => value.SurfaceStableId == surface.StableId)
            .OrderByDescending(value => value.Priority)
            .Select(value =>
            {
                var projected = values[value.InformationKindCode];
                return new SimulationWorldUIProjectionItem
                {
                    StableId = value.StableId,
                    InformationKindCode = value.InformationKindCode,
                    KoreanLabel = value.KoreanLabel,
                    StyleSemanticKey = InformationStyle(value.InformationKindCode),
                    ValueText = projected.Value,
                    UnitCode = value.UnitCode,
                    DataStatusCode = projected.Status,
                    SourceStableId = projected.Source,
                    ObservedAtUtc = session.WorldContext.GameDate,
                    LimitationCode = projected.Limitation,
                };
            })
            .ToArray();
    }

    private IReadOnlyList<SimulationWorldUIProjectionAction> ProjectActions(
        SimulationWorldUI화면영역기획 surface,
        경영SimulationSessionSnapshot session,
        SimulationFreightTransportSnapshot? receivableFreight,
        SimulationNpcFacilityInventorySnapshot? putAwayCandidate,
        bool putAwayFlow,
        string? actionBlockReason)
        => _uiPlan.ActionCandidates
            .Where(value => value.SurfaceStableId == surface.StableId)
            .OrderBy(value => value.DisplayOrder)
            .Select(value =>
            {
                var inspect = value.ActionKindCode == SimulationWorldUI행동종류Codes.조회;
                var confirm = value.ActionKindCode == SimulationWorldUI행동종류Codes.확정;
                var subject = putAwayFlow ? "적재" : "입고 검수";
                var canonicalActionCode = putAwayFlow
                    ? 창고입고행동코드.적재완료
                    : 창고입고행동코드.검수완료;
                return new SimulationWorldUIProjectionAction
                {
                    StableId = value.StableId,
                    ActionKindCode = value.ActionKindCode,
                    KoreanLabel = inspect ? "입출고 상세 보기" : subject + (confirm ? " 확정" : " 미리보기"),
                    StyleSemanticKey = ActionStyle(value.ActionKindCode),
                    CapabilityKey = inspect
                        ? value.CapabilityKey
                        : putAwayFlow
                            ? "WarehousePutAway." + (confirm ? "Confirm" : "Preview")
                            : value.CapabilityKey,
                    CanonicalActionCode = inspect ? string.Empty : canonicalActionCode,
                    ServerCommandKey = inspect
                        ? null
                        : putAwayFlow && confirm
                            ? "WarehousePutAway.Confirm"
                            : value.ServerCommandKey,
                    Enabled = inspect || actionBlockReason == null,
                    BlockReasonCode = inspect ? null : actionBlockReason,
                    RequiresPreview = value.RequiresPreview,
                    RequiresExplicitConfirmation = value.RequiresExplicitConfirmation,
                    RequiresExpectedRevision = value.RequiresExpectedRevision,
                    HttpMethod = inspect ? "GET" : "POST",
                    RouteTemplate = inspect
                        ? ProjectionRoute
                        : putAwayFlow
                            ? confirm ? PutAwayConfirmRoute : PutAwayPreviewRoute
                            : confirm ? ConfirmRoute : PreviewRoute,
                    RequestContractKey = inspect ? null : confirm
                        ? putAwayFlow
                            ? nameof(SimulationWarehousePutAwayConfirmRequest)
                            : nameof(SimulationFreightReceiptConfirmRequest)
                        : putAwayFlow
                            ? nameof(SimulationWarehousePutAwayPreviewRequest)
                            : nameof(SimulationFreightReceiptPreviewRequest),
                    ResponseContractKey = inspect
                        ? nameof(SimulationWorldUIProjection)
                        : confirm
                            ? nameof(경영SimulationSessionSnapshot)
                            : nameof(SimulationDecisionPreviewSnapshot),
                    CanonicalRequeryRouteTemplate = ProjectionRoute,
                    Invocation = inspect || (putAwayFlow ? putAwayCandidate == null : receivableFreight == null)
                        ? null
                        : new SimulationWorldUIActionInvocation
                    {
                        TargetStableId = putAwayFlow
                            ? putAwayCandidate!.InventoryStableId
                            : receivableFreight!.TransportRequestStableId,
                        TargetRevision = putAwayFlow
                            ? putAwayCandidate!.Revision
                            : receivableFreight!.Revision,
                        ActorStableId = putAwayFlow
                            ? PyeongchangSimulationNpcStableIds.진부적재담당
                            : PyeongchangSimulationNpcStableIds.진부입고검수담당,
                        ExpectedStateRevision = confirm ? session.Revision : null,
                        DurationTicks = 2,
                        SourceStableIds = new[]
                        {
                            putAwayFlow
                                ? putAwayCandidate!.InventoryStableId
                                : receivableFreight!.TransportRequestStableId,
                            putAwayFlow
                                ? PyeongchangSimulationWorldStableIds.창고적재규칙
                                : PyeongchangSimulationWorldStableIds.창고입고검수규칙,
                            surface.StableId,
                        },
                    },
                };
            })
            .ToArray();

    private static string InformationStyle(string informationKindCode)
        => informationKindCode == "Evidence"
            ? SimulationWorldUIStyleSemanticKeys.Evidence
            : informationKindCode == "Limitation"
                ? SimulationWorldUIStyleSemanticKeys.Limitation
                : SimulationWorldUIStyleSemanticKeys.Information;

    private static string ActionStyle(string actionKindCode)
        => actionKindCode == SimulationWorldUI행동종류Codes.확정
            ? SimulationWorldUIStyleSemanticKeys.ConfirmAction
            : actionKindCode == SimulationWorldUI행동종류Codes.미리보기
                ? SimulationWorldUIStyleSemanticKeys.PreviewAction
                : SimulationWorldUIStyleSemanticKeys.InspectAction;

    private static string ResolveWorkflowStageCode(
        IReadOnlyCollection<SimulationNpcTaskAssignmentSnapshot> assignments,
        IReadOnlyCollection<SimulationNpcFacilityInventorySnapshot> inventories,
        SimulationFreightTransportSnapshot? receivableFreight)
    {
        if (assignments.Any(value =>
                value.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove
                && IsActivePhase(value.PhaseCode))
            || inventories.Any(value => value.StateCode == SimulationNpcInventoryStateCodes.StorageEligible))
        {
            return 창고입고상태코드.적재대기;
        }
        if (assignments.Any(value =>
                value.ActionCode == SimulationNpcActionCodes.WarehouseInboundInspection
                && IsActivePhase(value.PhaseCode))
            || inventories.Any(value => value.StateCode == SimulationNpcInventoryStateCodes.PendingInspection))
        {
            return 창고입고상태코드.검수대기;
        }
        if (receivableFreight != null)
            return 창고입고상태코드.입고예정;
        if (inventories.Any(value => value.StateCode == SimulationNpcInventoryStateCodes.PutAwayCompleted))
            return 창고입고상태코드.적재완료;
        return 창고입고상태코드.입고예정;
    }

    private static string ResolveStateCode(
        IReadOnlyCollection<SimulationNpcTaskAssignmentSnapshot> assignments,
        IReadOnlyCollection<SimulationNpcFacilityInventorySnapshot> inventories,
        SimulationFreightTransportSnapshot? receivableFreight,
        SimulationNpcFacilityInventorySnapshot? putAwayCandidate)
    {
        if (assignments.Any(value => value.PhaseCode == SimulationNpcActionPhaseCodes.Blocked))
            return SimulationWorldUI상태Codes.차단;
        if (assignments.Any(value =>
                IsActivePhase(value.PhaseCode)))
            return SimulationWorldUI상태Codes.진행중;
        if (putAwayCandidate != null || receivableFreight != null)
            return SimulationWorldUI상태Codes.준비;
        if (inventories.Count > 0 && inventories.All(value =>
                value.StateCode == SimulationNpcInventoryStateCodes.PutAwayCompleted))
            return SimulationWorldUI상태Codes.완료;
        return SimulationWorldUI상태Codes.대기;
    }

    private static HashSet<string> ActivePutAwayInventoryIds(경영SimulationSessionSnapshot session)
        => (from task in session.Tasks
            join decision in session.Decisions
                on task.CausedByDecisionStableId equals decision.DecisionStableId
            where task.ActionCode == SimulationNpcActionCodes.WarehouseStorageMove
                && task.StateCode != SimulationTaskStateCodes.Completed
                && task.StateCode != SimulationTaskStateCodes.Cancelled
            from target in decision.TargetStableIds
            where target.StartsWith("npc-inventory:", StringComparison.Ordinal)
            select target).ToHashSet(StringComparer.Ordinal);

    private static bool IsActivePhase(string phaseCode)
        => phaseCode == SimulationNpcActionPhaseCodes.Scheduled
            || phaseCode == SimulationNpcActionPhaseCodes.Navigating
            || phaseCode == SimulationNpcActionPhaseCodes.Working;

    private static string PhaseKorean(string actionCode, string phaseCode)
        => phaseCode switch
        {
            SimulationNpcActionPhaseCodes.Scheduled => "작업 배정",
            SimulationNpcActionPhaseCodes.Navigating => actionCode == SimulationNpcActionCodes.WarehouseStorageMove
                ? "적재 위치로 이동"
                : "검수 위치로 이동",
            SimulationNpcActionPhaseCodes.Working => actionCode == SimulationNpcActionCodes.WarehouseStorageMove
                ? "재고 적재 중"
                : "입고 검수 중",
            SimulationNpcActionPhaseCodes.Completed => actionCode == SimulationNpcActionCodes.WarehouseStorageMove
                ? "적재 완료"
                : "검수 완료",
            SimulationNpcActionPhaseCodes.Blocked => "작업 차단",
            _ => phaseCode,
        };
}
}
