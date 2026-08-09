using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.PresentationContracts.Cargo
{
    public static class CargoJourneyZoneCodes
    {
        public const string FarmYard = "farm-yard";
        public const string TransportCorridor = "transport-corridor";
        public const string UrbanLogistics = "urban-logistics";
        public const string UrbanMarket = "urban-market";
    }

    public static class CargoJourneyAnchorStateCodes
    {
        public const string Previous = "Previous";
        public const string Current = "Current";
        public const string Next = "Next";
        public const string Planned = "Planned";
    }

    /// <summary>Presentation roles are independent from vendor prefab names.</summary>
    public static class CargoJourneyVisualRoleCodes
    {
        public const string FarmPackedBox = "FarmPackedBox";
        public const string VehicleLoad = "VehicleLoad";
        public const string LogisticsPallet = "LogisticsPallet";
        public const string MarketBackroom = "MarketBackroom";
    }

    public sealed class CargoJourneyProjectionInput
    {
        public CargoWarehouseHandoffSnapshot Handoff { get; set; } = null!;
        public DataRuntimeMode Mode { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public string OriginSourceStableId { get; set; } = string.Empty;
    }

    public sealed class CargoJourneyAnchorPresentationModel
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string CargoStableId { get; set; } = string.Empty;
        public string ZoneCode { get; set; } = string.Empty;
        public string AnchorCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string VisualRoleCode { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public string LabelText { get; set; } = string.Empty;
    }

    public sealed class CargoJourneyPresentationModel
    {
        public string CargoStableId { get; set; } = string.Empty;
        public WorldIdentityLineage Identity { get; set; } = null!;
        public long SourceRevision { get; set; }
        public DataRuntimeMode Mode { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public string HandoffStableId { get; set; } = string.Empty;
        public string HandoffStateCode { get; set; } = string.Empty;
        public string CurrentZoneCode { get; set; } = string.Empty;
        public CargoJourneyAnchorPresentationModel[] Anchors { get; set; } =
            Array.Empty<CargoJourneyAnchorPresentationModel>();
    }

    /// <summary>
    /// Projects the existing cargo handoff into zone-specific presentation anchors.
    /// The handoff has no market-arrival fact, so the market anchor remains planned.
    /// </summary>
    public sealed class CargoJourneyProjector
    {
        private static readonly HashSet<string> HandoffStates = new HashSet<string>(StringComparer.Ordinal)
        {
            CargoHandoffStateCodes.InTransit,
            CargoHandoffStateCodes.ArrivedAtWarehouse,
            CargoHandoffStateCodes.ReceivingCompleted,
        };

        public CargoJourneyPresentationModel Project(CargoJourneyProjectionInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var handoff = input.Handoff ?? throw new InvalidOperationException("CargoJourneyHandoffMissing");
            ValidateStableId(handoff.StableId, "CargoJourneyHandoffStableIdInvalid");
            ValidateStableId(handoff.CargoStableId, "CargoJourneyCargoStableIdInvalid");
            ValidateStableId(handoff.TransportTaskStableId, "CargoJourneyTransportTaskStableIdInvalid");
            ValidateStableId(handoff.InboundTaskStableId, "CargoJourneyInboundTaskStableIdInvalid");
            ValidateStableId(input.ProductStableId, "CargoJourneyProductStableIdInvalid");
            ValidateStableId(input.OriginSourceStableId, "CargoJourneyOriginSourceStableIdInvalid");
            if (handoff.Revision < 0) throw new InvalidOperationException("CargoJourneyRevisionInvalid");
            if (!HandoffStates.Contains(handoff.HandoffStateCode))
                throw new InvalidOperationException("CargoJourneyHandoffStateInvalid:" + handoff.HandoffStateCode);
            if (!Enum.IsDefined(typeof(DataRuntimeMode), input.Mode))
                throw new InvalidOperationException("CargoJourneyModeInvalid");

            var cargoWorldId = new WorldStableId(handoff.CargoStableId);
            var identity = new WorldIdentityLineage(cargoWorldId, new[]
            {
                new SourceStableId(input.OriginSourceStableId),
                new SourceStableId(input.ProductStableId),
                new SourceStableId(handoff.CargoStableId),
                new SourceStableId(handoff.StableId),
                new SourceStableId(handoff.TransportTaskStableId),
                new SourceStableId(handoff.InboundTaskStableId),
            });
            var currentZone = handoff.HandoffStateCode == CargoHandoffStateCodes.InTransit
                ? CargoJourneyZoneCodes.TransportCorridor
                : CargoJourneyZoneCodes.UrbanLogistics;
            var anchors = new[]
            {
                Anchor(cargoWorldId, handoff.CargoStableId, CargoJourneyZoneCodes.FarmYard,
                    "farm-yard.cargo-handoff", CargoJourneyVisualRoleCodes.FarmPackedBox,
                    CargoJourneyAnchorStateCodes.Previous, currentZone, input.ProductStableId),
                Anchor(cargoWorldId, handoff.CargoStableId, CargoJourneyZoneCodes.TransportCorridor,
                    "transport.vehicle-load", CargoJourneyVisualRoleCodes.VehicleLoad,
                    currentZone == CargoJourneyZoneCodes.TransportCorridor
                        ? CargoJourneyAnchorStateCodes.Current
                        : CargoJourneyAnchorStateCodes.Previous,
                    currentZone, input.ProductStableId),
                Anchor(cargoWorldId, handoff.CargoStableId, CargoJourneyZoneCodes.UrbanLogistics,
                    "logistics.inbound-or-storage", CargoJourneyVisualRoleCodes.LogisticsPallet,
                    currentZone == CargoJourneyZoneCodes.UrbanLogistics
                        ? CargoJourneyAnchorStateCodes.Current
                        : CargoJourneyAnchorStateCodes.Next,
                    currentZone, input.ProductStableId),
                Anchor(cargoWorldId, handoff.CargoStableId, CargoJourneyZoneCodes.UrbanMarket,
                    "market.backroom-planned", CargoJourneyVisualRoleCodes.MarketBackroom,
                    CargoJourneyAnchorStateCodes.Planned, currentZone, input.ProductStableId),
            };

            return new CargoJourneyPresentationModel
            {
                CargoStableId = handoff.CargoStableId,
                Identity = identity,
                SourceRevision = handoff.Revision,
                Mode = input.Mode,
                ProductStableId = input.ProductStableId,
                HandoffStableId = handoff.StableId,
                HandoffStateCode = handoff.HandoffStateCode,
                CurrentZoneCode = currentZone,
                Anchors = anchors,
            };
        }

        private static CargoJourneyAnchorPresentationModel Anchor(
            WorldStableId cargoWorldId,
            string cargoStableId,
            string zoneCode,
            string anchorCode,
            string visualRoleCode,
            string stateCode,
            string currentZone,
            string productStableId)
        {
            var id = new PresentationStableId("cargo-journey:" + zoneCode + ":" + cargoStableId);
            return new CargoJourneyAnchorPresentationModel
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, new[] { cargoWorldId }),
                CargoStableId = cargoStableId,
                ZoneCode = zoneCode,
                AnchorCode = anchorCode,
                StateCode = stateCode,
                VisualRoleCode = visualRoleCode,
                IsCurrent = string.Equals(zoneCode, currentZone, StringComparison.Ordinal),
                LabelText = productStableId + "\n" + cargoStableId + "\n" + stateCode,
            };
        }

        private static void ValidateStableId(string value, string error)
        {
            if (!StableDataId.IsValid(value)) throw new InvalidOperationException(error + ":" + value);
        }
    }
}
