using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.Warehouse
{
    /// <summary>
    /// 서버가 명시한 inbound-task stable ID를 기준으로 차량·화물·운송자·입고작업자를
    /// Warehouse World의 동일한 canonical relation으로 결합합니다.
    /// </summary>
    public sealed class WarehouseInboundHandoffInterpreter
    {
        public WarehouseWorldSnapshot Compose(
            WarehouseWorldSnapshot warehouse,
            IReadOnlyList<CargoWarehouseHandoffSnapshot> handoffs)
        {
            if (warehouse == null) throw new ArgumentNullException(nameof(warehouse));
            if (handoffs == null) throw new ArgumentNullException(nameof(handoffs));

            var objects = warehouse.Objects.ToList();
            foreach (var handoff in handoffs.OrderBy(item => item.StableId, StringComparer.Ordinal))
            {
                Apply(warehouse.StableId, objects, handoff);
            }

            warehouse.Objects = objects.ToArray();
            return warehouse;
        }

        private static void Apply(
            string warehouseStableId,
            ICollection<WarehouseWorldObject> objects,
            CargoWarehouseHandoffSnapshot handoff)
        {
            var relation = handoff.InboundTaskStableId;
            if (string.IsNullOrWhiteSpace(relation))
                throw new InvalidOperationException("WarehouseInboundHandoffRelationMissing:" + handoff.StableId);

            var destination = Destination(handoff.HandoffStateCode);
            foreach (var existing in objects.Where(item =>
                string.Equals(item.CanonicalTaskStableId, relation, StringComparison.Ordinal)))
            {
                existing.CanonicalRelationStableId = relation;
            }

            AddUnique(objects, Object(
                handoff.StableId,
                "Handoff",
                warehouseStableId,
                "입고 인계",
                handoff.HandoffStateCode,
                destination.Cargo,
                relation,
                relation,
                handoff.GeneratedAt));
            AddUnique(objects, Object(
                relation,
                "InboundTask",
                warehouseStableId,
                "입고 작업",
                handoff.HandoffStateCode,
                destination.Cargo,
                handoff.StableId,
                relation,
                handoff.GeneratedAt));
            AddUnique(objects, Object(
                handoff.CargoStableId,
                "Cargo",
                warehouseStableId,
                "운송 화물",
                handoff.HandoffStateCode,
                destination.Cargo,
                handoff.StableId,
                relation,
                handoff.GeneratedAt));
            AddUnique(objects, Object(
                "vehicle-projection:" + handoff.CargoStableId,
                "Vehicle",
                warehouseStableId,
                "운송 차량",
                handoff.HandoffStateCode,
                destination.Vehicle,
                handoff.TransportTaskStableId,
                relation,
                handoff.GeneratedAt));

            foreach (var movement in handoff.Movements.Where(item =>
                string.Equals(item.WorldZoneCode, "warehouse", StringComparison.Ordinal)))
            {
                var existingWorker = objects.FirstOrDefault(item =>
                    item.Kind == "Npc"
                    && string.Equals(item.CanonicalTaskStableId, movement.CanonicalTaskStableId, StringComparison.Ordinal));
                if (existingWorker != null)
                {
                    existingWorker.CurrentLocationCode = movement.CurrentWaypointKey;
                    existingWorker.LocationCode = movement.DestinationWaypointKey;
                    existingWorker.Status = movement.ArrivalActionCode;
                    existingWorker.CanonicalRelationStableId = relation;
                    continue;
                }

                AddUnique(objects, new WarehouseWorldObject
                {
                    StableId = movement.NpcStableId,
                    Kind = "Npc",
                    WarehouseStableId = warehouseStableId,
                    Title = movement.ActorRoleCode,
                    Status = movement.ArrivalActionCode,
                    CurrentLocationCode = movement.CurrentWaypointKey,
                    LocationCode = movement.DestinationWaypointKey,
                    SourceStableId = movement.StableId,
                    CanonicalTaskStableId = movement.CanonicalTaskStableId,
                    CanonicalRelationStableId = relation,
                    UpdatedAtUtc = movement.GeneratedAt,
                });
            }
        }

        private static void AddUnique(
            ICollection<WarehouseWorldObject> objects,
            WarehouseWorldObject candidate)
        {
            if (objects.Any(item => string.Equals(item.StableId, candidate.StableId, StringComparison.Ordinal)))
                throw new InvalidOperationException("DuplicateWarehouseHandoffObject:" + candidate.StableId);
            objects.Add(candidate);
        }

        private static WarehouseWorldObject Object(
            string stableId,
            string kind,
            string warehouseStableId,
            string title,
            string status,
            string location,
            string sourceStableId,
            string relation,
            DateTimeOffset updatedAt)
            => new()
            {
                StableId = stableId,
                Kind = kind,
                WarehouseStableId = warehouseStableId,
                Title = title,
                Status = status,
                LocationCode = location,
                SourceStableId = sourceStableId,
                CanonicalTaskStableId = relation,
                CanonicalRelationStableId = relation,
                UpdatedAtUtc = updatedAt,
            };

        private static HandoffDestination Destination(string stateCode)
            => stateCode switch
            {
                CargoHandoffStateCodes.InTransit => new(
                    WarehouseLocationSocketKeys.Approach,
                    WarehouseLocationSocketKeys.Approach),
                CargoHandoffStateCodes.ArrivedAtWarehouse => new(
                    WarehouseLocationSocketKeys.InboundDock,
                    WarehouseLocationSocketKeys.InboundDock),
                CargoHandoffStateCodes.ReceivingCompleted => new(
                    WarehouseLocationSocketKeys.StorageZone,
                    WarehouseLocationSocketKeys.VehicleExit),
                _ => throw new InvalidOperationException("WarehouseInboundHandoffStateInvalid:" + stateCode),
            };

        private readonly struct HandoffDestination
        {
            public HandoffDestination(string cargo, string vehicle)
            {
                Cargo = cargo;
                Vehicle = vehicle;
            }

            public string Cargo { get; }
            public string Vehicle { get; }
        }
    }
}
