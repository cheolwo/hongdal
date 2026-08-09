using System;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Transport
{
    public static class TransportCorridorPresentationVersions
    {
        public const string InterpreterContract = "transport-corridor-interpretation-v1";
        public const string RuleSet = "canonical-handoff-route-v1";
        public const string VisualRule = "transport-corridor-visual-v1";
        public const string PresentationContract = "transport-corridor-presentation-v1";
        public const string Perspective = "Transporter";
    }

    public sealed class TruckMovementPresentationModel
    {
        public string CorridorStableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public string TruckStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string CurrentNodeKey { get; set; } = string.Empty;
        public string DestinationNodeKey { get; set; } = string.Empty;
        public string MovementStateCode { get; set; } = string.Empty;
        public string ArrivalAnimationCode { get; set; } = string.Empty;
        public string StatusLabelText { get; set; } = string.Empty;
    }

    public sealed class TransportCorridorPresenter
    {
        public TruckMovementPresentationModel? Present(TransportCorridorSnapshot? snapshot)
        {
            if (snapshot == null) return null;
            if (snapshot.Lineage == null) throw new InvalidOperationException("TransportCorridorLineageMissing");
            if (snapshot.Truck == null) throw new InvalidOperationException("TransportCorridorTruckMissing");

            return new TruckMovementPresentationModel
            {
                CorridorStableId = snapshot.StableId,
                DataRevision = snapshot.Revision,
                InterpretationRevision = snapshot.Lineage.InterpretationRevision,
                PresentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                    snapshot.Lineage.InterpretationRevision,
                    TransportCorridorPresentationVersions.Perspective,
                    TransportCorridorPresentationVersions.VisualRule,
                    TransportCorridorPresentationVersions.PresentationContract),
                TruckStableId = snapshot.Truck.StableId,
                CargoStableId = snapshot.Truck.CargoStableId,
                CanonicalTaskStableId = snapshot.Truck.CanonicalTaskStableId,
                RouteCode = snapshot.Truck.RouteCode,
                CurrentNodeKey = snapshot.Truck.CurrentNodeKey,
                DestinationNodeKey = snapshot.Truck.DestinationNodeKey,
                MovementStateCode = snapshot.Truck.MovementStateCode,
                ArrivalAnimationCode = snapshot.Truck.ArrivalActionCode,
                StatusLabelText = snapshot.Truck.CargoStableId + "\n"
                    + snapshot.Truck.CurrentNodeKey + " → " + snapshot.Truck.DestinationNodeKey,
            };
        }
    }

    public interface ITruckMovementPresentationTarget
    {
        string TruckStableId { get; }
        void ApplyTruckMovementPresentation(TruckMovementPresentationModel model);
    }
}
