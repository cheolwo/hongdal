using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarmChoicePlayableCodes
    {
        public const string SituationStableId =
            "farm-situation:potato-harvest-complete.v1";
        public const string AreaSetStableId =
            "area-set:sim:pyeongchang:farm-production.v1";
        public const string ProductStableId = "product:potato";

        public const string AwaitingHarvest = "AwaitingHarvest";
        public const string AwaitingChoice = "AwaitingChoice";
        public const string ChoiceConfirmed = "ChoiceConfirmed";

        public const string ReserveStorageChoice =
            "farm-choice:potato.reserve-storage.v1";
        public const string HubShipmentChoice =
            "farm-choice:potato.hub-shipment.v1";
        public const string TownDirectSaleChoice =
            "farm-choice:potato.town-direct-sale.v1";
    }

    public sealed class SimulationFarmChoiceFactSnapshot
    {
        public string FactStableId { get; set; } = string.Empty;
        public string FactCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ValueCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarmChoiceCandidateReasonSnapshot
    {
        public string ReasonCode { get; set; } = string.Empty;
        public string[] SourceFactStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarmChoiceCandidateSnapshot
    {
        public string ChoiceStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string CardFunctionCode { get; set; } = string.Empty;
        public string KoreanDisplayName { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationFarmChoiceCandidateReasonSnapshot[] CandidateReasons { get; set; }
            = Array.Empty<SimulationFarmChoiceCandidateReasonSnapshot>();
    }

    public sealed class SimulationFarmChoiceContextSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string SituationStableId { get; set; } = string.Empty;
        public int SituationRevision { get; set; }
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public string AreaSetStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string SituationStateCode { get; set; } = string.Empty;
        public string AppliedChoiceStableId { get; set; } = string.Empty;
        public bool IsSimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
        public SimulationFarmChoiceFactSnapshot[] Facts { get; set; }
            = Array.Empty<SimulationFarmChoiceFactSnapshot>();
        public SimulationFarmChoiceCandidateSnapshot[] Candidates { get; set; }
            = Array.Empty<SimulationFarmChoiceCandidateSnapshot>();
    }

    public sealed class SimulationFarmChoicePreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string ChoiceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmChoicePreviewSnapshot
    {
        public string SituationStableId { get; set; } = string.Empty;
        public string ChoiceStableId { get; set; } = string.Empty;
        public long BaseRevision { get; set; }
        public bool IsCandidateOnly { get; set; }
        public bool RequiresExplicitConfirm { get; set; }
        public SimulationHarvestDispositionImpactPreviewSnapshot Impact { get; set; }
            = new SimulationHarvestDispositionImpactPreviewSnapshot();
    }

    public sealed class SimulationFarmChoiceConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ChoiceStableId { get; set; } = string.Empty;
    }
}
