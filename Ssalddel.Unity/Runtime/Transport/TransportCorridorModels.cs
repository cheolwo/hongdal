using System;
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
    }

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

            return new TransportCorridorSnapshot
            {
                StableId = "transport-corridor:" + handoff.CargoStableId,
                Revision = handoff.Revision,
                GeneratedAt = handoff.GeneratedAt,
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
