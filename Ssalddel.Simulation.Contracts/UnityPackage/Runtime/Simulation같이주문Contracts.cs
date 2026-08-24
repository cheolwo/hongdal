using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation같이주문DecisionTypeCodes
    {
        public const string 모집결과확정 = "GroupOrderRecruitmentFinalization";
    }

    public sealed class Simulation같이주문의향Request
    {
        public string IntentStableId { get; set; } = string.Empty;
        public string ParticipantStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public bool ExplicitParticipationConsent { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation같이주문PreviewRequest
    {
        public string GroupOrderStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string DeliveryScopeStableId { get; set; } = string.Empty;
        public string AggregationFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public int TargetParticipantCount { get; set; }
        public decimal TargetQuantity { get; set; }
        public int FinalizationDurationTicks { get; set; } = 1;
        public Simulation같이주문의향Request[] Intents { get; set; }
            = Array.Empty<Simulation같이주문의향Request>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation같이주문ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation같이주문PreviewRequest GroupOrder { get; set; }
            = new Simulation같이주문PreviewRequest();
    }

    public sealed class Simulation같이주문PreviewSnapshot
    {
        public string GroupOrderStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string DeliveryScopeStableId { get; set; } = string.Empty;
        public string AggregationFacilityStableId { get; set; } = string.Empty;
        public string SuggestedStateCode { get; set; } = string.Empty;
        public string CompletionStateCode { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int TargetParticipantCount { get; set; }
        public decimal TargetQuantity { get; set; }
        public bool MinimumParticipantCountMet { get; set; }
        public bool TargetParticipantCountMet { get; set; }
        public bool TargetQuantityMet { get; set; }
        public bool ExplicitTargetMet { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation같이주문의향Snapshot
    {
        public string IntentStableId { get; set; } = string.Empty;
        public string ParticipantStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public bool ExplicitParticipationConsent { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation같이주문Snapshot
    {
        public string GroupOrderStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string DeliveryScopeStableId { get; set; } = string.Empty;
        public string AggregationFacilityStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public int ParticipantCount { get; set; }
        public decimal TotalQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int TargetParticipantCount { get; set; }
        public decimal TargetQuantity { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int CreatedTick { get; set; }
        public int? FinalizedTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] ExcludedOperationalEffectCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public Simulation같이주문의향Snapshot[] Intents { get; set; }
            = Array.Empty<Simulation같이주문의향Snapshot>();
    }
}
