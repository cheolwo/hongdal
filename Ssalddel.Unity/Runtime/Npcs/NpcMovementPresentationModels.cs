using System;
using System.Globalization;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Npcs
{
    public static class NpcMovementInterpretationVersions
    {
        public const string Contract = "npc-route-interpretation-v1";
        public const string RuleSet = "semantic-waypoint-route-v1";
        public const string VisualRule = "npc-movement-visual-v1";
        public const string PresentationContract = "npc-movement-presentation-v1";
    }

    public sealed class NpcMovementWorldState
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string NpcStableId { get; set; } = string.Empty;
        public string ActorRoleCode { get; set; } = string.Empty;
        public string WorldZoneCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string CurrentWaypointKey { get; set; } = string.Empty;
        public string DestinationWaypointKey { get; set; } = string.Empty;
        public string MovementStateCode { get; set; } = string.Empty;
        public string ArrivalActionCode { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public InterpretationLineage Lineage { get; set; } = null!;
    }

    public sealed class NpcMovementInterpreter
    {
        public NpcMovementWorldState Interpret(NpcMovementSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(
                    snapshot.StableId,
                    snapshot.Revision.ToString(CultureInfo.InvariantCulture),
                    snapshot.GeneratedAt,
                    string.Equals(snapshot.MovementStateCode, NpcMovementStateCodes.Stale, StringComparison.Ordinal)
                        ? DataQualityCodes.Stale
                        : DataQualityCodes.Observed),
            });
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                NpcMovementInterpretationVersions.Contract,
                NpcMovementInterpretationVersions.RuleSet,
                snapshot.WorldZoneCode + "|" + snapshot.RouteCode + "|"
                    + snapshot.CurrentWaypointKey + "|" + snapshot.DestinationWaypointKey);

            return new NpcMovementWorldState
            {
                StableId = snapshot.StableId,
                DataRevision = snapshot.Revision,
                NpcStableId = snapshot.NpcStableId,
                ActorRoleCode = snapshot.ActorRoleCode,
                WorldZoneCode = snapshot.WorldZoneCode,
                RouteCode = snapshot.RouteCode,
                CurrentWaypointKey = snapshot.CurrentWaypointKey,
                DestinationWaypointKey = snapshot.DestinationWaypointKey,
                MovementStateCode = snapshot.MovementStateCode,
                ArrivalActionCode = snapshot.ArrivalActionCode,
                CanonicalTaskStableId = snapshot.CanonicalTaskStableId,
                GeneratedAt = snapshot.GeneratedAt,
                Lineage = new InterpretationLineage(
                    inputs,
                    NpcMovementInterpretationVersions.Contract,
                    NpcMovementInterpretationVersions.RuleSet,
                    interpretationRevision),
            };
        }
    }

    public sealed class NpcMovementPresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string CurrentWaypointKey { get; set; } = string.Empty;
        public string DestinationWaypointKey { get; set; } = string.Empty;
        public string MovementStateCode { get; set; } = string.Empty;
        public string ArrivalAnimationCode { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E8,
        "NPC 목표·행동·기억의 생활 연속성을 표현하거나 조율한다.",
        Boundary = "NPC 표현 코드 존재만으로 E8 폐루프 완료를 주장하지 않는다.")]
    public sealed class NpcMovementPresenter
    {
        public NpcMovementPresentationModel Present(NpcMovementWorldState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Lineage == null) throw new InvalidOperationException("NpcMovementLineageMissing");

            return new NpcMovementPresentationModel
            {
                StableId = state.StableId,
                DataRevision = state.DataRevision,
                InterpretationRevision = state.Lineage.InterpretationRevision,
                PresentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                    state.Lineage.InterpretationRevision,
                    state.ActorRoleCode,
                    NpcMovementInterpretationVersions.VisualRule,
                    NpcMovementInterpretationVersions.PresentationContract),
                NpcStableId = state.NpcStableId,
                RouteCode = state.RouteCode,
                CurrentWaypointKey = state.CurrentWaypointKey,
                DestinationWaypointKey = state.DestinationWaypointKey,
                MovementStateCode = state.MovementStateCode,
                ArrivalAnimationCode = state.ArrivalActionCode,
                CanonicalTaskStableId = state.CanonicalTaskStableId,
            };
        }
    }

    public interface INpcMovementPresentationTarget
    {
        string NpcStableId { get; }
        void ApplyMovementPresentation(NpcMovementPresentationModel model);
    }
}
