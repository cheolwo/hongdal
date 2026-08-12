using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class DirectOnlineSaleStateCodes
    {
        public const string AwaitingPackingReview = "AwaitingPackingReview";
        public const string PackedForListing = "PackedForListing";
    }

    public sealed class ProducerDirectPackingRuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public decimal NetQuantityPerParcelKg { get; set; }
        public string PackageTypeCode { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class ProducerDirectPackingLotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string PackageTypeCode { get; set; } = string.Empty;
        public int ParcelCount { get; set; }
        public decimal NetQuantityPerParcelKg { get; set; }
        public decimal NetQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class OnlineMarketListingCandidateData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PackingLotStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string PublicationStateCode { get; set; } = string.Empty;
        public int AvailableParcelCount { get; set; }
        public decimal QuantityPerParcelKg { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class DirectOnlineSaleSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public 수확LotSimulationData HarvestLot { get; set; } = new 수확LotSimulationData();
        public HarvestDispositionDecisionData DispositionDecision { get; set; }
            = new HarvestDispositionDecisionData();
        public ProducerDirectPackingRuleSnapshot PackingRule { get; set; }
            = new ProducerDirectPackingRuleSnapshot();
        public ProducerDirectPackingLotSimulationData? PackingLot { get; set; }
        public OnlineMarketListingCandidateData? ListingCandidate { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class DirectOnlinePackingPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string DispositionDecisionStableId { get; set; } = string.Empty;
        public string PackingRuleStableId { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class DirectOnlinePackingCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
    }

    public sealed class DirectOnlineSaleSimulationValidator
    {
        public void Validate(DirectOnlineSaleSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId) || snapshot.SimulationDate == default
                || (snapshot.StateCode != DirectOnlineSaleStateCodes.AwaitingPackingReview
                    && snapshot.StateCode != DirectOnlineSaleStateCodes.PackedForListing)
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("DirectOnlineSaleSnapshotInvalid");
            ValidateHarvest(snapshot.HarvestLot);
            ValidateDisposition(snapshot.DispositionDecision, snapshot.HarvestLot);
            ValidateRule(snapshot.PackingRule, snapshot.HarvestLot);
            var packed = snapshot.StateCode == DirectOnlineSaleStateCodes.PackedForListing;
            if (packed != (snapshot.PackingLot != null && snapshot.ListingCandidate != null))
                throw new InvalidOperationException("DirectOnlineSaleStateMismatch");
            if (snapshot.PackingLot != null)
                ValidatePackingLot(snapshot.PackingLot, snapshot.HarvestLot, snapshot.PackingRule,
                    snapshot.DispositionDecision);
            if (snapshot.ListingCandidate != null)
                ValidateCandidate(snapshot.ListingCandidate, snapshot.PackingLot!);
        }

        private static void ValidateHarvest(수확LotSimulationData lot)
        {
            if (lot == null || !StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato" || lot.Quantity != 300m
                || lot.UnitCode != "kg" || lot.SourceStableIds == null || lot.SourceStableIds.Length == 0)
                throw new InvalidOperationException("DirectOnlineSaleHarvestLotInvalid");
        }

        private static void ValidateDisposition(HarvestDispositionDecisionData decision,
            수확LotSimulationData harvest)
        {
            if (decision == null || !StableDataId.IsValid(decision.StableId) || decision.Revision <= 0
                || decision.HarvestLotStableId != harvest.StableId
                || decision.ChoiceCode != HarvestDispositionChoiceCodes.DirectOnlineSale
                || decision.NextWorkflowCode != "ProducerPackingCandidate"
                || decision.Quantity != harvest.Quantity || decision.UnitCode != harvest.UnitCode
                || decision.SourceStableIds == null || !decision.SourceStableIds.Contains(harvest.StableId))
                throw new InvalidOperationException("DirectOnlineSaleDispositionInvalid");
        }

        private static void ValidateRule(ProducerDirectPackingRuleSnapshot rule, 수확LotSimulationData harvest)
        {
            if (rule == null || !StableDataId.IsValid(rule.StableId) || rule.Revision <= 0
                || rule.NetQuantityPerParcelKg != 5m || rule.PackageTypeCode != "ParcelBox"
                || rule.SourceTypeCode != "Fixture" || harvest.Quantity % rule.NetQuantityPerParcelKg != 0
                || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0
                || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("DirectOnlineSalePackingRuleInvalid");
        }

        private static void ValidatePackingLot(ProducerDirectPackingLotSimulationData lot,
            수확LotSimulationData harvest, ProducerDirectPackingRuleSnapshot rule,
            HarvestDispositionDecisionData decision)
        {
            if (!StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.HarvestLotStableId != harvest.StableId
                || lot.CanonicalProductStableId != harvest.CanonicalProductStableId
                || lot.PackageTypeCode != rule.PackageTypeCode
                || lot.ParcelCount != decimal.ToInt32(harvest.Quantity / rule.NetQuantityPerParcelKg)
                || lot.NetQuantityPerParcelKg != rule.NetQuantityPerParcelKg
                || lot.ParcelCount * lot.NetQuantityPerParcelKg != lot.NetQuantity
                || lot.NetQuantity != harvest.Quantity || lot.UnitCode != harvest.UnitCode
                || lot.SourceStableIds == null || !lot.SourceStableIds.Contains(harvest.StableId)
                || !lot.SourceStableIds.Contains(decision.StableId))
                throw new InvalidOperationException("DirectOnlineSalePackingLotInvalid");
        }

        private static void ValidateCandidate(OnlineMarketListingCandidateData candidate,
            ProducerDirectPackingLotSimulationData lot)
        {
            if (!StableDataId.IsValid(candidate.StableId) || candidate.Revision <= 0
                || candidate.PackingLotStableId != lot.StableId
                || candidate.CanonicalProductStableId != lot.CanonicalProductStableId
                || candidate.NextWorkflowCode != "OnlineMarketListingDraft"
                || candidate.PublicationStateCode != "CandidateOnly"
                || candidate.AvailableParcelCount != lot.ParcelCount
                || candidate.QuantityPerParcelKg != lot.NetQuantityPerParcelKg
                || candidate.SourceStableIds == null || !candidate.SourceStableIds.Contains(lot.StableId))
                throw new InvalidOperationException("OnlineMarketListingCandidateInvalid");
        }
    }

    public sealed class DirectOnlineSaleSimulationEngine
    {
        private readonly DirectOnlineSaleSimulationValidator validator;
        public DirectOnlineSaleSimulationEngine(DirectOnlineSaleSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public DirectOnlinePackingPreview PreviewPacking(DirectOnlineSaleSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != DirectOnlineSaleStateCodes.AwaitingPackingReview)
                throw new InvalidOperationException("DirectOnlineSaleAlreadyPacked");
            return new DirectOnlinePackingPreview
            {
                StableId = "direct-online-packing-preview:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                SnapshotStableId = snapshot.StableId, ExpectedDataRevision = snapshot.DataRevision,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                DispositionDecisionStableId = snapshot.DispositionDecision.StableId,
                PackingRuleStableId = snapshot.PackingRule.StableId,
                RequiresExplicitConfirmation = true,
            };
        }

        public DirectOnlinePackingCommand Confirm(DirectOnlineSaleSimulationSnapshot snapshot,
            DirectOnlinePackingPreview preview)
        {
            validator.Validate(snapshot);
            var expected = PreviewPacking(snapshot);
            if (preview == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.HarvestLotStableId != expected.HarvestLotStableId
                || preview.DispositionDecisionStableId != expected.DispositionDecisionStableId
                || preview.PackingRuleStableId != expected.PackingRuleStableId
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("DirectOnlineSalePreviewStaleOrInvalid");
            return new DirectOnlinePackingCommand
            {
                StableId = "direct-online-packing-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                PreviewStableId = preview.StableId, SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision, SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public DirectOnlineSaleSimulationSnapshot Tick(DirectOnlineSaleSimulationSnapshot snapshot,
            DirectOnlinePackingCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1
                || command.PreviewStableId != PreviewPacking(snapshot).StableId)
                throw new InvalidOperationException("DirectOnlineSaleCommandStaleOrInvalid");
            var next = Clone(snapshot);
            next.DataRevision++;
            next.StateCode = DirectOnlineSaleStateCodes.PackedForListing;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.PackingLot = new ProducerDirectPackingLotSimulationData
            {
                StableId = "producer-packing-lot:sim.potato.direct.20260407.r1", Revision = 1,
                HarvestLotStableId = next.HarvestLot.StableId,
                CanonicalProductStableId = next.HarvestLot.CanonicalProductStableId,
                PackageTypeCode = next.PackingRule.PackageTypeCode,
                ParcelCount = decimal.ToInt32(next.HarvestLot.Quantity / next.PackingRule.NetQuantityPerParcelKg),
                NetQuantityPerParcelKg = next.PackingRule.NetQuantityPerParcelKg,
                NetQuantity = next.HarvestLot.Quantity, UnitCode = next.HarvestLot.UnitCode,
                SourceStableIds = new[] { next.HarvestLot.StableId, next.DispositionDecision.StableId, command.StableId },
            };
            next.ListingCandidate = new OnlineMarketListingCandidateData
            {
                StableId = "online-listing-candidate:sim.potato.direct.r1", Revision = 1,
                PackingLotStableId = next.PackingLot.StableId,
                CanonicalProductStableId = next.PackingLot.CanonicalProductStableId,
                NextWorkflowCode = "OnlineMarketListingDraft", PublicationStateCode = "CandidateOnly",
                AvailableParcelCount = next.PackingLot.ParcelCount,
                QuantityPerParcelKg = next.PackingLot.NetQuantityPerParcelKg,
                SourceStableIds = new[] { next.HarvestLot.StableId, next.PackingLot.StableId, command.StableId },
            };
            validator.Validate(next);
            return next;
        }

        private static DirectOnlineSaleSimulationSnapshot Clone(DirectOnlineSaleSimulationSnapshot source)
            => new DirectOnlineSaleSimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, SimulationDate = source.SimulationDate,
                StateCode = source.StateCode, HarvestLot = source.HarvestLot,
                DispositionDecision = source.DispositionDecision, PackingRule = source.PackingRule,
                PackingLot = source.PackingLot, ListingCandidate = source.ListingCandidate,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public sealed class OnlineMarketListingDraftSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string ListingCandidateStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int AvailableParcelCount { get; set; }
        public decimal QuantityPerParcelKg { get; set; }
        public decimal? UnitPrice { get; set; }
        public bool IsPublished { get; set; }
        public int OrderCount { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class DirectOnlineListingDraftAdapter
    {
        private readonly DirectOnlineSaleSimulationValidator validator;
        public DirectOnlineListingDraftAdapter(DirectOnlineSaleSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public OnlineMarketListingDraftSnapshot Create(DirectOnlineSaleSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != DirectOnlineSaleStateCodes.PackedForListing
                || snapshot.PackingLot == null || snapshot.ListingCandidate == null)
                throw new InvalidOperationException("OnlineMarketListingCandidateRequired");
            return new OnlineMarketListingDraftSnapshot
            {
                StableId = "online-listing-draft:sim.potato.direct.r1",
                ListingCandidateStableId = snapshot.ListingCandidate.StableId,
                CanonicalProductStableId = snapshot.PackingLot.CanonicalProductStableId,
                DisplayName = "감자 5kg",
                AvailableParcelCount = snapshot.PackingLot.ParcelCount,
                QuantityPerParcelKg = snapshot.PackingLot.NetQuantityPerParcelKg,
                UnitPrice = null, IsPublished = false, OrderCount = 0,
                SourceStableIds = new[] { snapshot.HarvestLot.StableId,
                    snapshot.PackingLot.StableId, snapshot.ListingCandidate.StableId },
            };
        }
    }

    public sealed class DirectOnlineSalePresentationModel
    {
        public string StateText { get; set; } = string.Empty;
        public string PackingText { get; set; } = string.Empty;
        public string CandidateText { get; set; } = string.Empty;
        public string LineageText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class DirectOnlineSaleProjector
    {
        private readonly DirectOnlineSaleSimulationValidator validator;
        public DirectOnlineSaleProjector(DirectOnlineSaleSimulationValidator value) => validator = value;
        public DirectOnlineSalePresentationModel Project(DirectOnlineSaleSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            return new DirectOnlineSalePresentationModel
            {
                StateText = snapshot.StateCode + " · REV " + snapshot.DataRevision,
                PackingText = snapshot.PackingLot == null ? "생산자 소포장 검토 전 · 300kg"
                    : snapshot.PackingLot.StableId + " · 5kg × 60 · 300kg",
                CandidateText = snapshot.ListingCandidate == null ? "LISTING NOT READY"
                    : snapshot.ListingCandidate.NextWorkflowCode + " · CANDIDATE ONLY",
                LineageText = snapshot.PackingLot == null ? snapshot.HarvestLot.StableId
                    : snapshot.HarvestLot.StableId + " → " + snapshot.DispositionDecision.StableId
                        + " → " + snapshot.PackingLot.StableId,
                LimitationText = "5kg 소포장은 Fixture이며 상품 공개·가격·주문·결제·택배를 만들지 않습니다.",
            };
        }
    }

    public static class DirectOnlineSaleSimulationFixture
    {
        public static DirectOnlineSaleSimulationSnapshot Create(HarvestDispositionSimulationSnapshot disposition)
        {
            new HarvestDispositionSimulationValidator().Validate(disposition);
            if (disposition.StateCode != HarvestDispositionStateCodes.Decided
                || disposition.Decision?.ChoiceCode != HarvestDispositionChoiceCodes.DirectOnlineSale)
                throw new InvalidOperationException("DirectOnlineSaleDispositionRequired");
            return new DirectOnlineSaleSimulationSnapshot
            {
                StableId = "direct-online-sale:sim.potato", DataRevision = 1,
                ModeCode = "Simulation", ScenarioStableId = disposition.ScenarioStableId,
                SimulationDate = disposition.SimulationDate,
                StateCode = DirectOnlineSaleStateCodes.AwaitingPackingReview,
                HarvestLot = disposition.HarvestLot, DispositionDecision = disposition.Decision,
                SourceStableIds = new[] { disposition.HarvestLot.StableId, disposition.Decision.StableId,
                    "source:fixture.direct-online-packing" },
                PackingRule = new ProducerDirectPackingRuleSnapshot
                {
                    StableId = "packing-rule:sim.potato.direct.5kg", Revision = 1,
                    NetQuantityPerParcelKg = 5m, PackageTypeCode = "ParcelBox", SourceTypeCode = "Fixture",
                    SourceStableIds = new[] { "source:fixture.direct-online-packing" },
                    Limitations = new[] { "5kg 소포장은 Simulation Fixture이며 실제 포장·택배 기준이 아닙니다." },
                },
            };
        }
    }
}
