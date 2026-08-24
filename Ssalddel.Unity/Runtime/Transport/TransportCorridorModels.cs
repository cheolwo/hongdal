using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.Transport
{
    public sealed class TruckMovementSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string CurrentNodeKey { get; set; } = string.Empty;
        public string DestinationNodeKey { get; set; } = string.Empty;
        public string MovementStateCode { get; set; } = string.Empty;
        public string ArrivalActionCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
    }

    public sealed class TransportCorridorSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public TruckMovementSnapshot Truck { get; set; } = null!;
        public DateTimeOffset GeneratedAt { get; set; }
        public InterpretationLineage Lineage { get; set; } = null!;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "권위 회랑 상태를 Unity 이동·표현 모델로 투영한다.",
        Boundary = "투영 모델은 통행 권위나 WI 발현을 확정하지 않는다.")]
    public sealed class TransportCorridorProjector
    {
        public TransportCorridorSnapshot? Project(CargoWarehouseHandoffSnapshot? handoff)
        {
            if (handoff == null || !string.Equals(handoff.HandoffStateCode, CargoHandoffStateCodes.InTransit, StringComparison.Ordinal))
                return null;

            var movement = handoff.Movements.SingleOrDefault(item =>
                string.Equals(item.WorldZoneCode, "transport-network", StringComparison.Ordinal)
                && string.Equals(item.ActorRoleCode, "Transporter", StringComparison.Ordinal));
            if (movement == null) throw new InvalidOperationException("TransportCorridorMovementMissing");
            if (!StableDataId.IsValid(handoff.CargoStableId) || !StableDataId.IsValid(movement.CanonicalTaskStableId))
                throw new InvalidOperationException("TransportCorridorCanonicalReferenceInvalid");
            if (!string.Equals(movement.MovementStateCode, NpcMovementStateCodes.Moving, StringComparison.Ordinal))
                throw new InvalidOperationException("TransportCorridorMovementStateInvalid");

            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(
                    handoff.StableId,
                    handoff.Revision.ToString(CultureInfo.InvariantCulture),
                    handoff.GeneratedAt),
                new DataRevisionReference(
                    movement.StableId,
                    movement.Revision.ToString(CultureInfo.InvariantCulture),
                    movement.GeneratedAt),
            });
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                TransportCorridorPresentationVersions.InterpreterContract,
                TransportCorridorPresentationVersions.RuleSet,
                movement.RouteCode + "|" + movement.CurrentWaypointKey + "|" + movement.DestinationWaypointKey);

            return new TransportCorridorSnapshot
            {
                StableId = "transport-corridor:" + handoff.CargoStableId,
                Revision = handoff.Revision,
                GeneratedAt = handoff.GeneratedAt,
                Lineage = new InterpretationLineage(
                    inputs,
                    TransportCorridorPresentationVersions.InterpreterContract,
                    TransportCorridorPresentationVersions.RuleSet,
                    interpretationRevision),
                Truck = new TruckMovementSnapshot
                {
                    StableId = "truck-projection:" + handoff.CargoStableId,
                    CargoStableId = handoff.CargoStableId,
                    CanonicalTaskStableId = movement.CanonicalTaskStableId,
                    RouteCode = movement.RouteCode,
                    CurrentNodeKey = movement.CurrentWaypointKey,
                    DestinationNodeKey = movement.DestinationWaypointKey,
                    MovementStateCode = movement.MovementStateCode,
                    ArrivalActionCode = movement.ArrivalActionCode,
                    Revision = movement.Revision,
                    GeneratedAt = movement.GeneratedAt,
                },
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 회랑 조회 흐름을 조율한다.",
        Boundary = "조회 UseCase는 Simulation 상태나 WorldRevision을 바꾸지 않는다.")]
    public sealed class TransportCorridorQueryUseCase
    {
        private readonly CargoWarehouseHandoffQueryUseCase handoffQuery;
        private readonly TransportCorridorProjector projector;
        public TransportCorridorQueryUseCase(CargoWarehouseHandoffQueryUseCase handoffQuery, TransportCorridorProjector projector)
        { this.handoffQuery = handoffQuery; this.projector = projector; }
        public async Task<TransportCorridorSnapshot?> 실행Async(CancellationToken cancellationToken = default)
            => projector.Project(await handoffQuery.실행Async(cancellationToken).ConfigureAwait(false));
    }

    public interface ITruckMovementTarget
    {
        string TruckStableId { get; }
        void ApplyTruckMovement(TruckMovementSnapshot movement);
    }

    public sealed class TruckMovementApplicator
    {
        private long lastRevision = -1;

        public bool Apply(
            TruckMovementPresentationModel model,
            ITruckMovementPresentationTarget target)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (target == null || !string.Equals(target.TruckStableId, model.TruckStableId, StringComparison.Ordinal))
                throw new InvalidOperationException("TruckMovementPresentationTargetMismatch");
            if (model.DataRevision < lastRevision) return false;
            target.ApplyTruckMovementPresentation(model);
            lastRevision = model.DataRevision;
            return true;
        }

        /// <summary>DIP4 이전 Snapshot 기반 target을 위한 호환 경로입니다.</summary>
        public bool Apply(TransportCorridorSnapshot snapshot, ITruckMovementTarget target)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (target == null || !string.Equals(target.TruckStableId, snapshot.Truck.StableId, StringComparison.Ordinal))
                throw new InvalidOperationException("TruckMovementTargetMismatch");
            if (snapshot.Revision < lastRevision) return false;
            target.ApplyTruckMovement(snapshot.Truck); lastRevision = snapshot.Revision; return true;
        }
    }
}
