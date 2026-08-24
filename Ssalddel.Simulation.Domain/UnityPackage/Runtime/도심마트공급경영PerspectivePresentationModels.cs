using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트공급RiskReasonCodes
    {
        public const string SupplyCoverageGap = "SupplyCoverageGap";
        public const string OrderFulfillmentGap = "OrderFulfillmentGap";
        public const string PaymentSchedulePressure = "PaymentSchedulePressure";
        public const string ShelfWorkCapacityLimited = "ShelfWorkCapacityLimited";
        public const string SupplierConcentrationHigh = "SupplierConcentrationHigh";
        public const string WasteExposure = "WasteExposure";
    }

    public static class 도심마트공급RiskQueueCodes
    {
        public const string DecisionRequired = "DecisionRequired";
        public const string RiskAttention = "RiskAttention";
        public const string ActiveContracts = "ActiveContracts";
    }

    public sealed class 도심마트공급RiskInterpretationRule
    {
        public string RuleRevision { get; set; } = string.Empty;
        public decimal SupplierConcentrationThresholdRate { get; set; }
        public decimal WasteAttentionThresholdQuantity { get; set; }
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class 도심마트공급RiskItemWorldState
    {
        public string ReasonCode { get; set; } = string.Empty;
        public string QueueCode { get; set; } = string.Empty;
        public decimal ObservedValue { get; set; }
        public decimal ThresholdValue { get; set; }
        public string EvidenceStableId { get; set; } = string.Empty;
    }

    public sealed class 마트관리자공급경영PerspectiveWorldState
    {
        public string StableId { get; set; } = string.Empty;
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string[] IntentCodes { get; set; } = Array.Empty<string>();
        public 도심마트공급RiskItemWorldState[] RiskItems { get; set; } =
            Array.Empty<도심마트공급RiskItemWorldState>();
        public string LimitationText { get; set; } = string.Empty;
        public SimulationDataLineage[] SourceLineage { get; set; } =
            Array.Empty<SimulationDataLineage>();
    }

    public sealed class 마트관리자공급경영PerspectiveInterpreter
    {
        public 마트관리자공급경영PerspectiveWorldState Interpret(
            도심마트공급경영SimulationWorldState source,
            도심마트공급RiskInterpretationRule rule)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rule == null || string.IsNullOrWhiteSpace(rule.RuleRevision)
                || rule.SupplierConcentrationThresholdRate <= 0m
                || rule.SupplierConcentrationThresholdRate > 1m
                || rule.WasteAttentionThresholdQuantity < 0m
                || string.IsNullOrWhiteSpace(rule.LimitationText))
                throw new SimulationContractException("SupplyRiskRuleInvalid");
            var risks = new System.Collections.Generic.List<도심마트공급RiskItemWorldState>();
            if (source.UnfulfilledQuantity > 0m)
            {
                risks.Add(Item(도심마트공급RiskReasonCodes.SupplyCoverageGap,
                    도심마트공급RiskQueueCodes.DecisionRequired, source.UnfulfilledQuantity, 0m, source.StableId));
                risks.Add(Item(도심마트공급RiskReasonCodes.OrderFulfillmentGap,
                    도심마트공급RiskQueueCodes.DecisionRequired, source.UnfulfilledQuantity, 0m, source.StableId));
            }
            if (source.OutstandingPaymentAmount > 0m)
                risks.Add(Item(도심마트공급RiskReasonCodes.PaymentSchedulePressure,
                    도심마트공급RiskQueueCodes.RiskAttention, source.OutstandingPaymentAmount, 0m, source.StableId));
            if (source.RejectedDeliveryQuantity > 0m)
                risks.Add(Item(도심마트공급RiskReasonCodes.ShelfWorkCapacityLimited,
                    도심마트공급RiskQueueCodes.RiskAttention, source.RejectedDeliveryQuantity, 0m, source.StableId));
            var maximumShare = source.SupplierResults.Length == 0
                ? 0m : source.SupplierResults.Max(value => value.AcceptedSupplyShareRate);
            if (maximumShare > rule.SupplierConcentrationThresholdRate)
                risks.Add(Item(도심마트공급RiskReasonCodes.SupplierConcentrationHigh,
                    도심마트공급RiskQueueCodes.RiskAttention, maximumShare,
                    rule.SupplierConcentrationThresholdRate,
                    source.SupplierResults.OrderByDescending(value => value.AcceptedSupplyShareRate)
                        .ThenBy(value => value.SupplierStableId, StringComparer.Ordinal).First().SupplierStableId));
            if (source.WasteQuantity > rule.WasteAttentionThresholdQuantity)
                risks.Add(Item(도심마트공급RiskReasonCodes.WasteExposure,
                    도심마트공급RiskQueueCodes.RiskAttention, source.WasteQuantity,
                    rule.WasteAttentionThresholdQuantity, source.StableId));

            return new 마트관리자공급경영PerspectiveWorldState
            {
                StableId = "market-manager-supply-perspective:" + source.ScenarioStableId,
                PerspectiveRevision = "supply-risk:" + source.SimulationRevision + ":" + rule.RuleRevision,
                ProductStableId = source.ProductStableId,
                IntentCodes = new[] { "ReviewDemandAndOrders", "ReviewOrderFulfillmentRisk", "CompareSupplyOffers" },
                RiskItems = risks.OrderBy(value => value.QueueCode, StringComparer.Ordinal)
                    .ThenBy(value => value.ReasonCode, StringComparer.Ordinal).ToArray(),
                LimitationText = rule.LimitationText,
                SourceLineage = source.SourceLineage,
            };
        }

        private static 도심마트공급RiskItemWorldState Item(
            string reason, string queue, decimal observed, decimal threshold, string evidence)
            => new 도심마트공급RiskItemWorldState
            {
                ReasonCode = reason,
                QueueCode = queue,
                ObservedValue = observed,
                ThresholdValue = threshold,
                EvidenceStableId = evidence,
            };
    }

    public sealed class DemandAndOrderBriefingSurface
    {
        public string StableId { get; set; } = string.Empty;
        public int AsOfTick { get; set; }
        public int TodayOrderCount { get; set; }
        public decimal TodayRequestedQuantity { get; set; }
        public decimal PendingOrderQuantity { get; set; }
        public int FulfilledOrderCount { get; set; }
        public int PartiallyFulfilledOrderCount { get; set; }
        public int UnfulfilledOrderCount { get; set; }
        public decimal CurrentAvailableInventory { get; set; }
        public decimal TodayScheduledInbound { get; set; }
        public decimal Next7DayDemand { get; set; }
        public decimal ImmediatelyFulfillableQuantity { get; set; }
        public decimal InboundAfterProcessingPotentialQuantity { get; set; }
        public decimal CannotCoverQuantity { get; set; }
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public string SimulationLabel { get; set; } = "Simulation";
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class ManagementPreviewSurface
    {
        public decimal HardDemandQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledQuantity { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal EndingCash { get; set; }
        public decimal OutstandingPaymentAmount { get; set; }
        public decimal WasteQuantity { get; set; }
        public decimal ReceivingWorkload { get; set; }
    }

    public sealed class SupplyPortfolioBoardSurfaceItem
    {
        public string SupplierStableId { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal AcceptedSupplyShareRate { get; set; }
        public decimal PurchaseCost { get; set; }
    }

    public sealed class CashScheduleSurfaceItem
    {
        public int Tick { get; set; }
        public decimal PaymentDue { get; set; }
        public decimal PaymentPaid { get; set; }
        public decimal ClosingCash { get; set; }
    }

    public sealed class DeliveryCommitmentSurfaceItem
    {
        public string DeliveryStableId { get; set; } = string.Empty;
        public string SupplierStableId { get; set; } = string.Empty;
        public int ArrivalTick { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
    }

    public sealed class 도심마트공급계약PresentationSnapshot
    {
        public string PresentationRevision { get; set; } = string.Empty;
        public DemandAndOrderBriefingSurface DemandAndOrders { get; set; } =
            new DemandAndOrderBriefingSurface();
        public ManagementPreviewSurface ManagementPreview { get; set; } =
            new ManagementPreviewSurface();
        public SupplyPortfolioBoardSurfaceItem[] SupplyPortfolio { get; set; } =
            Array.Empty<SupplyPortfolioBoardSurfaceItem>();
        public CashScheduleSurfaceItem[] CashSchedule { get; set; } =
            Array.Empty<CashScheduleSurfaceItem>();
        public DeliveryCommitmentSurfaceItem[] DeliveryCommitments { get; set; } =
            Array.Empty<DeliveryCommitmentSurfaceItem>();
        public SimulationDataLineage[] SourceLineage { get; set; } =
            Array.Empty<SimulationDataLineage>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class 도심마트공급계약PresentationProjector
    {
        public 도심마트공급계약PresentationSnapshot Project(
            도심마트공급경영SimulationWorldState source,
            마트관리자공급경영PerspectiveWorldState perspective,
            int asOfTick)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (perspective == null) throw new ArgumentNullException(nameof(perspective));
            if (asOfTick < 0 || asOfTick >= source.Ticks.Length)
                throw new SimulationContractException("SupplyPresentationAsOfTickInvalid");
            if (perspective.ProductStableId != source.ProductStableId)
                throw new SimulationContractException("SupplyPresentationPerspectiveMismatch");
            var today = source.Ticks[asOfTick];
            var cumulativeCreated = source.Orders.Where(value => value.CreatedTick <= asOfTick)
                .Sum(value => value.RequestedQuantity);
            var cumulativeFulfilled = source.Ticks.Where(value => value.Tick <= asOfTick)
                .Sum(value => value.FulfilledQuantity);
            var cumulativeClosed = source.Ticks.Where(value => value.Tick <= asOfTick)
                .Sum(value => value.UnfulfilledClosedQuantity);
            var pending = Math.Max(0m, cumulativeCreated - cumulativeFulfilled - cumulativeClosed);
            var upcomingAccepted = source.Deliveries.Where(value => value.ArrivalTick > asOfTick
                    && value.ArrivalTick <= Math.Min(source.Ticks.Length - 1, asOfTick + 7))
                .Sum(value => value.AcceptedQuantity);
            var immediate = Math.Min(pending, today.ClosingInventory);
            var afterInbound = Math.Min(Math.Max(0m, pending - immediate), upcomingAccepted);
            var reasonCodes = perspective.RiskItems.Select(value => value.ReasonCode)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();

            return new 도심마트공급계약PresentationSnapshot
            {
                PresentationRevision = "supply-presentation:" + source.SimulationRevision + ":"
                    + perspective.PerspectiveRevision + ":" + asOfTick,
                DemandAndOrders = new DemandAndOrderBriefingSurface
                {
                    StableId = "demand-order-briefing:" + source.ScenarioStableId,
                    AsOfTick = asOfTick,
                    TodayOrderCount = source.Orders.Count(value => value.CreatedTick == asOfTick),
                    TodayRequestedQuantity = source.Orders.Where(value => value.CreatedTick == asOfTick)
                        .Sum(value => value.RequestedQuantity),
                    PendingOrderQuantity = pending,
                    FulfilledOrderCount = source.Orders.Count(value => value.StateCode == SimulationOrderStateCodes.Fulfilled),
                    PartiallyFulfilledOrderCount = source.Orders.Count(value => value.StateCode == SimulationOrderStateCodes.PartiallyFulfilled),
                    UnfulfilledOrderCount = source.Orders.Count(value => value.StateCode == SimulationOrderStateCodes.Unfulfilled),
                    CurrentAvailableInventory = today.ClosingInventory,
                    TodayScheduledInbound = source.Deliveries.Where(value => value.ArrivalTick == asOfTick)
                        .Sum(value => value.PlannedQuantity),
                    Next7DayDemand = source.Orders.Where(value => value.CreatedTick > asOfTick
                            && value.CreatedTick <= Math.Min(source.Ticks.Length - 1, asOfTick + 7))
                        .Sum(value => value.RequestedQuantity),
                    ImmediatelyFulfillableQuantity = immediate,
                    InboundAfterProcessingPotentialQuantity = afterInbound,
                    CannotCoverQuantity = Math.Max(0m, pending - immediate - afterInbound),
                    ReasonCodes = reasonCodes,
                    LimitationText = perspective.LimitationText,
                },
                ManagementPreview = new ManagementPreviewSurface
                {
                    HardDemandQuantity = source.HardDemandQuantity,
                    FulfilledQuantity = source.FulfilledQuantity,
                    UnfulfilledQuantity = source.UnfulfilledQuantity,
                    PurchaseCost = source.PurchaseCost,
                    EndingCash = source.EndingCash,
                    OutstandingPaymentAmount = source.OutstandingPaymentAmount,
                    WasteQuantity = source.WasteQuantity,
                    ReceivingWorkload = source.ReceivingWorkload,
                },
                SupplyPortfolio = source.SupplierResults.Select(value => new SupplyPortfolioBoardSurfaceItem
                {
                    SupplierStableId = value.SupplierStableId,
                    PlannedQuantity = value.PlannedQuantity,
                    AcceptedQuantity = value.AcceptedQuantity,
                    AcceptedSupplyShareRate = value.AcceptedSupplyShareRate,
                    PurchaseCost = value.PurchaseCost,
                }).ToArray(),
                CashSchedule = source.Ticks.Select(value => new CashScheduleSurfaceItem
                {
                    Tick = value.Tick,
                    PaymentDue = value.PaymentDue,
                    PaymentPaid = value.PaymentPaid,
                    ClosingCash = value.ClosingCash,
                }).ToArray(),
                DeliveryCommitments = source.Deliveries.Select(value => new DeliveryCommitmentSurfaceItem
                {
                    DeliveryStableId = value.DeliveryStableId,
                    SupplierStableId = value.SupplierStableId,
                    ArrivalTick = value.ArrivalTick,
                    PlannedQuantity = value.PlannedQuantity,
                    AcceptedQuantity = value.AcceptedQuantity,
                    RejectedQuantity = value.RejectedQuantity,
                }).ToArray(),
                SourceLineage = source.SourceLineage,
            };
        }
    }

    public static class 도심마트감자공급RiskSimulationFixture
    {
        public static 도심마트공급RiskInterpretationRule Rule()
            => new 도심마트공급RiskInterpretationRule
            {
                RuleRevision = "potato-supply-risk-rule:1",
                SupplierConcentrationThresholdRate = 0.60m,
                WasteAttentionThresholdQuantity = 0m,
                LimitationText = "Simulation 결과이며 자동 발주·계약 변경을 수행하지 않습니다.",
            };

        public static 마트관리자공급경영PerspectiveWorldState Perspective()
            => new 마트관리자공급경영PerspectiveInterpreter().Interpret(
                도심마트감자공급경영SimulationEngineFixture.Run(), Rule());

        public static 도심마트공급계약PresentationSnapshot Presentation(int asOfTick = 7)
        {
            var source = 도심마트감자공급경영SimulationEngineFixture.Run();
            var perspective = new 마트관리자공급경영PerspectiveInterpreter().Interpret(source, Rule());
            return new 도심마트공급계약PresentationProjector().Project(source, perspective, asOfTick);
        }
    }
}
