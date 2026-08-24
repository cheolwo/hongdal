using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class SimulationFreightDispatchCandidateRequest
    {
        public string CarrierCandidateStableId { get; set; } = string.Empty;
        public string VehicleStableId { get; set; } = string.Empty;
        public bool IsFreightApp { get; set; }
        public bool IsVehicleActive { get; set; }
        public bool IsDriverOperating { get; set; }
        public bool WasPreviouslyRejected { get; set; }
        public decimal? LocationAgeMinutes { get; set; }
        public decimal? PickupDistanceKm { get; set; }
        public decimal? PickupAllowedRadiusKm { get; set; }
        public decimal VehicleCapacity { get; set; }
        public string VehicleCapacityUnitCode { get; set; } = string.Empty;
        public bool IsVehicleCompatible { get; set; }
        public string[] VehicleBlockReasonCodes { get; set; } = Array.Empty<string>();
        public decimal DriverWaitingMinutes { get; set; }
        public bool? CanCompleteSchedule { get; set; }
        public bool? CanInsertSchedule { get; set; }
        public bool HasRouteChangeBenefit { get; set; }
        public decimal? EstimatedExtraProfit { get; set; }
        public decimal? AdditionalDelayMinutes { get; set; }
        public string RecommendationTypeCode { get; set; } = "single";
        public bool IsCargoSensitive { get; set; }
        public decimal? ReturnDetourDistanceKm { get; set; }
        public bool UsesReturnDestination { get; set; }
        public string BaseReason { get; set; } = string.Empty;
    }

    public sealed class SimulationFreightDispatchRequest
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public decimal LocationFreshnessMinutes { get; set; } = 10m;
        public decimal BasePickupRadiusKm { get; set; } = 5m;
        public decimal MaximumRemotePickupRadiusKm { get; set; } = 30m;
        public decimal RemotePickupAverageSpeedKmH { get; set; } = 40m;
        public decimal RemotePickupArrivalBufferMinutes { get; set; } = 10m;
        public decimal? PickupWindowRemainingMinutes { get; set; }
        public string? ExcludedCarrierCandidateStableId { get; set; }
        public SimulationFreightDispatchCandidateRequest[] Candidates { get; set; }
            = Array.Empty<SimulationFreightDispatchCandidateRequest>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFreightDispatchPreviewRequest
    {
        public SimulationFreightDispatchRequest Dispatch { get; set; }
            = new SimulationFreightDispatchRequest();
        public SimulationLogisticsMovementPreviewRequest Movement { get; set; }
            = new SimulationLogisticsMovementPreviewRequest();
    }

    public sealed class SimulationFreightDispatchConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string SelectedCarrierCandidateStableId { get; set; } = string.Empty;
        public SimulationFreightDispatchPreviewRequest FreightDispatch { get; set; }
            = new SimulationFreightDispatchPreviewRequest();
    }

    public sealed class SimulationFreightDispatchScoreBreakdownSnapshot
    {
        public decimal ScheduleScore { get; set; }
        public decimal ProfitScore { get; set; }
        public decimal DelayScore { get; set; }
        public decimal DistanceScore { get; set; }
        public decimal RecommendationTypeScore { get; set; }
        public decimal CargoSensitivityScore { get; set; }
        public decimal ReturnBurdenScore { get; set; }
        public decimal BaseScore { get; set; }
        public decimal DriverWaitingScore { get; set; }
        public decimal TotalScore { get; set; }
    }

    public sealed class SimulationFreightDispatchCandidateEvaluationSnapshot
    {
        public string CarrierCandidateStableId { get; set; } = string.Empty;
        public string VehicleStableId { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public bool IsRecommended { get; set; }
        public bool IsSelected { get; set; }
        public int Rank { get; set; }
        public decimal? PickupDistanceKm { get; set; }
        public decimal VehicleCapacity { get; set; }
        public string VehicleCapacityUnitCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationFreightDispatchScoreBreakdownSnapshot Score { get; set; }
            = new SimulationFreightDispatchScoreBreakdownSnapshot();
    }

    public sealed class SimulationFreightDispatchDecisionSnapshot
    {
        public string DispatchOfferStableId { get; set; } = string.Empty;
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string? RecommendedCarrierCandidateStableId { get; set; }
        public string? SelectedCarrierCandidateStableId { get; set; }
        public string? SelectedVehicleStableId { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationFreightDispatchCandidateEvaluationSnapshot[] CandidateEvaluations { get; set; }
            = Array.Empty<SimulationFreightDispatchCandidateEvaluationSnapshot>();
    }

    public sealed class SimulationFreightDispatchPreviewSnapshot
    {
        public long ObservedRevision { get; set; }
        public int ObservedWorldTick { get; set; }
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string DispatchOfferStableId { get; set; } = string.Empty;
        public string? RecommendedCarrierCandidateStableId { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationFreightDispatchCandidateEvaluationSnapshot[] CandidateEvaluations { get; set; }
            = Array.Empty<SimulationFreightDispatchCandidateEvaluationSnapshot>();
        public SimulationLogisticsMovementPreviewSnapshot LogisticsMovement { get; set; }
            = new SimulationLogisticsMovementPreviewSnapshot();
    }
}
