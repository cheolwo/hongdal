using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoHubDispositionStateCodes
    {
        public const string AcceptedAtHub = "AcceptedAtHub";
        public const string LotsSeparated = "LotsSeparated";
        public const string OutboundCandidate = "OutboundCandidate";

        internal static bool IsKnown(string value) => value == AcceptedAtHub
            || value == LotsSeparated || value == OutboundCandidate;
    }

    public static class PotatoHubDispositionCommandCodes
    {
        public const string SeparateLots = "SeparateLots";
        public const string CreateOutboundCandidate = "CreateOutboundCandidate";
    }

    public sealed class PotatoHubAcceptedLotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string InspectionResultStableId { get; set; } = string.Empty;
        public string SourceCargoStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubRejectedLossLotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string InspectionResultStableId { get; set; } = string.Empty;
        public string SourceCargoStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoCityOutboundCargoCandidateSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string AcceptedLotStableId { get; set; } = string.Empty;
        public string OriginStableId { get; set; } = string.Empty;
        public string DestinationStableId { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubDispositionSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string HubReceivingStableId { get; set; } = string.Empty;
        public string InspectionResultStableId { get; set; } = string.Empty;
        public string SourceCargoStableId { get; set; } = string.Empty;
        public decimal ReceivedQuantityKg { get; set; }
        public decimal AcceptedQuantityKg { get; set; }
        public decimal RejectedQuantityKg { get; set; }
        public string RejectionReasonCode { get; set; } = string.Empty;
        public PotatoHubAcceptedLotSimulationData? AcceptedLot { get; set; }
        public PotatoHubRejectedLossLotSimulationData? RejectedLossLot { get; set; }
        public PotatoCityOutboundCargoCandidateSimulationData? OutboundCandidate { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubDispositionPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public decimal AcceptedQuantityKg { get; set; }
        public decimal RejectedQuantityKg { get; set; }
        public string SourceLotStableId { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class PotatoHubDispositionCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
    }

    public sealed class PotatoHubDispositionSimulationValidator
    {
        public void Validate(PotatoHubDispositionSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || snapshot.SimulationDate == default || snapshot.GeneratedAt == default
                || !PotatoHubDispositionStateCodes.IsKnown(snapshot.StateCode)
                || !StableDataId.IsValid(snapshot.HubReceivingStableId)
                || !StableDataId.IsValid(snapshot.InspectionResultStableId)
                || !StableDataId.IsValid(snapshot.SourceCargoStableId)
                || snapshot.ReceivedQuantityKg <= 0 || snapshot.AcceptedQuantityKg < 0
                || snapshot.RejectedQuantityKg < 0
                || snapshot.AcceptedQuantityKg + snapshot.RejectedQuantityKg != snapshot.ReceivedQuantityKg
                || (snapshot.RejectedQuantityKg > 0) != !string.IsNullOrWhiteSpace(snapshot.RejectionReasonCode)
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("PotatoHubDispositionSnapshotInvalid");

            var separated = snapshot.StateCode != PotatoHubDispositionStateCodes.AcceptedAtHub;
            if (separated != (snapshot.AcceptedLot != null && snapshot.RejectedLossLot != null))
                throw new InvalidOperationException("PotatoHubDispositionLotsStateMismatch");
            if ((snapshot.AcceptedLot == null) != (snapshot.RejectedLossLot == null))
                throw new InvalidOperationException("PotatoHubDispositionLotsMustRemainPaired");
            if (snapshot.AcceptedLot != null) ValidateAcceptedLot(snapshot, snapshot.AcceptedLot);
            if (snapshot.RejectedLossLot != null) ValidateRejectedLot(snapshot, snapshot.RejectedLossLot);

            var candidateState = snapshot.StateCode == PotatoHubDispositionStateCodes.OutboundCandidate;
            if (candidateState != (snapshot.OutboundCandidate != null))
                throw new InvalidOperationException("PotatoHubOutboundCandidateStateMismatch");
            if (snapshot.OutboundCandidate != null) ValidateCandidate(snapshot, snapshot.OutboundCandidate);
        }

        private static void ValidateAcceptedLot(PotatoHubDispositionSimulationSnapshot snapshot,
            PotatoHubAcceptedLotSimulationData lot)
        {
            if (!StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato"
                || lot.InspectionResultStableId != snapshot.InspectionResultStableId
                || lot.SourceCargoStableId != snapshot.SourceCargoStableId
                || lot.Quantity != snapshot.AcceptedQuantityKg || lot.UnitCode != "kg"
                || lot.StateCode != "AcceptedForOutbound" || lot.SourceStableIds == null
                || !lot.SourceStableIds.Contains(snapshot.InspectionResultStableId)
                || !lot.SourceStableIds.Contains(snapshot.SourceCargoStableId))
                throw new InvalidOperationException("PotatoHubAcceptedLotInvalid");
        }

        private static void ValidateRejectedLot(PotatoHubDispositionSimulationSnapshot snapshot,
            PotatoHubRejectedLossLotSimulationData lot)
        {
            if (!StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato"
                || lot.InspectionResultStableId != snapshot.InspectionResultStableId
                || lot.SourceCargoStableId != snapshot.SourceCargoStableId
                || lot.Quantity != snapshot.RejectedQuantityKg || lot.UnitCode != "kg"
                || lot.StateCode != "LossRecorded" || lot.ReasonCode != snapshot.RejectionReasonCode
                || lot.SourceStableIds == null
                || !lot.SourceStableIds.Contains(snapshot.InspectionResultStableId)
                || !lot.SourceStableIds.Contains(snapshot.SourceCargoStableId))
                throw new InvalidOperationException("PotatoHubRejectedLossLotInvalid");
        }

        private static void ValidateCandidate(PotatoHubDispositionSimulationSnapshot snapshot,
            PotatoCityOutboundCargoCandidateSimulationData candidate)
        {
            if (snapshot.AcceptedLot == null || snapshot.RejectedLossLot == null
                || !StableDataId.IsValid(candidate.StableId) || candidate.Revision <= 0
                || candidate.AcceptedLotStableId != snapshot.AcceptedLot.StableId
                || !StableDataId.IsValid(candidate.OriginStableId)
                || !StableDataId.IsValid(candidate.DestinationStableId)
                || candidate.OriginStableId == candidate.DestinationStableId
                || !StableDataId.IsValid(candidate.RouteStableId)
                || candidate.Quantity != snapshot.AcceptedLot.Quantity || candidate.UnitCode != "kg"
                || candidate.StateCode != "CandidateOnly" || candidate.SourceStableIds == null
                || !candidate.SourceStableIds.Contains(snapshot.AcceptedLot.StableId)
                || candidate.SourceStableIds.Contains(snapshot.RejectedLossLot.StableId)
                || candidate.Limitations == null || candidate.Limitations.Length == 0
                || candidate.Limitations.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("PotatoCityOutboundCandidateInvalid");
        }
    }

    public sealed class PotatoHubDispositionSimulationEngine
    {
        private readonly PotatoHubDispositionSimulationValidator validator;

        public PotatoHubDispositionSimulationEngine(PotatoHubDispositionSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public PotatoHubDispositionPreview PreviewSeparation(PotatoHubDispositionSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoHubDispositionStateCodes.AcceptedAtHub)
                throw new InvalidOperationException("PotatoHubDispositionSeparationStateInvalid");
            return Preview(snapshot, PotatoHubDispositionCommandCodes.SeparateLots,
                snapshot.AcceptedQuantityKg, snapshot.RejectedQuantityKg, snapshot.InspectionResultStableId);
        }

        public PotatoHubDispositionPreview PreviewOutboundCandidate(PotatoHubDispositionSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoHubDispositionStateCodes.LotsSeparated || snapshot.AcceptedLot == null)
                throw new InvalidOperationException("PotatoHubOutboundCandidateStateInvalid");
            return Preview(snapshot, PotatoHubDispositionCommandCodes.CreateOutboundCandidate,
                snapshot.AcceptedLot.Quantity, 0, snapshot.AcceptedLot.StableId);
        }

        public PotatoHubDispositionCommand Confirm(PotatoHubDispositionSimulationSnapshot snapshot,
            PotatoHubDispositionPreview preview)
        {
            validator.Validate(snapshot);
            var expected = preview?.CommandCode == PotatoHubDispositionCommandCodes.SeparateLots
                ? PreviewSeparation(snapshot) : PreviewOutboundCandidate(snapshot);
            if (preview == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.AcceptedQuantityKg != expected.AcceptedQuantityKg
                || preview.RejectedQuantityKg != expected.RejectedQuantityKg
                || preview.SourceLotStableId != expected.SourceLotStableId
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("PotatoHubDispositionPreviewStaleOrInvalid");
            return new PotatoHubDispositionCommand
            {
                StableId = "hub-" + preview.CommandCode.ToLowerInvariant() + "-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = preview.CommandCode,
                PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public PotatoHubDispositionSimulationSnapshot Tick(PotatoHubDispositionSimulationSnapshot snapshot,
            PotatoHubDispositionCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
                throw new InvalidOperationException("PotatoHubDispositionCommandStaleOrInvalid");
            var expected = command.CommandCode == PotatoHubDispositionCommandCodes.SeparateLots
                ? PreviewSeparation(snapshot) : PreviewOutboundCandidate(snapshot);
            if (command.PreviewStableId != expected.StableId)
                throw new InvalidOperationException("PotatoHubDispositionCommandPreviewMismatch");

            var next = Clone(snapshot);
            next.DataRevision++;
            next.GeneratedAt = next.GeneratedAt.AddMinutes(1);
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            if (command.CommandCode == PotatoHubDispositionCommandCodes.SeparateLots)
            {
                next.StateCode = PotatoHubDispositionStateCodes.LotsSeparated;
                next.AcceptedLot = new PotatoHubAcceptedLotSimulationData
                {
                    StableId = "hub-accepted-lot:sim.potato.20260410.r1", Revision = 1,
                    CanonicalProductStableId = "product:potato",
                    InspectionResultStableId = next.InspectionResultStableId,
                    SourceCargoStableId = next.SourceCargoStableId,
                    Quantity = next.AcceptedQuantityKg, UnitCode = "kg", StateCode = "AcceptedForOutbound",
                    SourceStableIds = new[] { next.InspectionResultStableId, next.SourceCargoStableId, command.StableId },
                };
                next.RejectedLossLot = new PotatoHubRejectedLossLotSimulationData
                {
                    StableId = "hub-loss-lot:sim.potato.20260410.r1", Revision = 1,
                    CanonicalProductStableId = "product:potato",
                    InspectionResultStableId = next.InspectionResultStableId,
                    SourceCargoStableId = next.SourceCargoStableId,
                    Quantity = next.RejectedQuantityKg, UnitCode = "kg", StateCode = "LossRecorded",
                    ReasonCode = next.RejectionReasonCode,
                    SourceStableIds = new[] { next.InspectionResultStableId, next.SourceCargoStableId, command.StableId },
                };
            }
            else if (command.CommandCode == PotatoHubDispositionCommandCodes.CreateOutboundCandidate)
            {
                next.StateCode = PotatoHubDispositionStateCodes.OutboundCandidate;
                next.OutboundCandidate = new PotatoCityOutboundCargoCandidateSimulationData
                {
                    StableId = "outbound-candidate:sim.potato.hub-city.r1", Revision = 1,
                    AcceptedLotStableId = next.AcceptedLot!.StableId,
                    OriginStableId = "hub:sim.inbound", DestinationStableId = "city:sim.market-inbound",
                    RouteStableId = "route:sim.hub-city", Quantity = next.AcceptedLot.Quantity,
                    UnitCode = "kg", StateCode = "CandidateOnly",
                    SourceStableIds = new[] { next.AcceptedLot.StableId, command.StableId },
                    Limitations = new[]
                    {
                        "City outbound candidate는 Simulation 계획이며 출발 Cargo, Hub 재고, City 입고를 확정하지 않습니다.",
                    },
                };
            }
            else throw new InvalidOperationException("PotatoHubDispositionCommandUnknown");
            validator.Validate(next);
            return next;
        }

        private static PotatoHubDispositionPreview Preview(PotatoHubDispositionSimulationSnapshot snapshot,
            string commandCode, decimal accepted, decimal rejected, string sourceLot)
            => new PotatoHubDispositionPreview
            {
                StableId = "hub-" + commandCode.ToLowerInvariant() + "-preview:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = commandCode, SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                AcceptedQuantityKg = accepted, RejectedQuantityKg = rejected,
                SourceLotStableId = sourceLot, RequiresExplicitConfirmation = true,
            };

        private static PotatoHubDispositionSimulationSnapshot Clone(PotatoHubDispositionSimulationSnapshot source)
            => new PotatoHubDispositionSimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, SimulationDate = source.SimulationDate,
                GeneratedAt = source.GeneratedAt, StateCode = source.StateCode,
                HubReceivingStableId = source.HubReceivingStableId,
                InspectionResultStableId = source.InspectionResultStableId,
                SourceCargoStableId = source.SourceCargoStableId,
                ReceivedQuantityKg = source.ReceivedQuantityKg,
                AcceptedQuantityKg = source.AcceptedQuantityKg,
                RejectedQuantityKg = source.RejectedQuantityKg,
                RejectionReasonCode = source.RejectionReasonCode,
                AcceptedLot = source.AcceptedLot, RejectedLossLot = source.RejectedLossLot,
                OutboundCandidate = source.OutboundCandidate,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public sealed class PotatoHubDispositionPresentationModel
    {
        public string StateCode { get; set; } = string.Empty;
        public string LotsText { get; set; } = string.Empty;
        public string CandidateText { get; set; } = string.Empty;
        public string LineageText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public bool CanReviewSeparation { get; set; }
        public bool CanReviewOutboundCandidate { get; set; }
    }

    public sealed class PotatoHubDispositionProjector
    {
        private readonly PotatoHubDispositionSimulationValidator validator;
        public PotatoHubDispositionProjector(PotatoHubDispositionSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public PotatoHubDispositionPresentationModel Project(PotatoHubDispositionSimulationSnapshot source)
        {
            validator.Validate(source);
            return new PotatoHubDispositionPresentationModel
            {
                StateCode = source.StateCode,
                LotsText = source.AcceptedLot == null ? "NOT SEPARATED"
                    : source.AcceptedLot.StableId + " · 288kg ACCEPTED\n"
                        + source.RejectedLossLot!.StableId + " · 12kg LOSS · " + source.RejectionReasonCode,
                CandidateText = source.OutboundCandidate == null ? "NO OUTBOUND CANDIDATE"
                    : source.OutboundCandidate.StableId + " · 288kg · CANDIDATE ONLY",
                LineageText = source.SourceCargoStableId + " → " + source.InspectionResultStableId
                    + (source.AcceptedLot == null ? string.Empty : " → " + source.AcceptedLot.StableId)
                    + (source.OutboundCandidate == null ? string.Empty : " → " + source.OutboundCandidate.StableId),
                LimitationText = source.OutboundCandidate == null
                    ? "Lot 분리 전에는 City outbound 후보를 만들 수 없습니다."
                    : string.Join(" · ", source.OutboundCandidate.Limitations),
                CanReviewSeparation = source.StateCode == PotatoHubDispositionStateCodes.AcceptedAtHub,
                CanReviewOutboundCandidate = source.StateCode == PotatoHubDispositionStateCodes.LotsSeparated,
            };
        }
    }

    public static class PotatoHubDispositionSimulationFixture
    {
        public static PotatoHubDispositionSimulationSnapshot Create(PotatoHubReceivingSimulationSnapshot accepted)
        {
            new PotatoHubReceivingSimulationValidator().Validate(accepted);
            if (accepted.StateCode != PotatoHubReceivingStateCodes.Accepted || accepted.InspectionResult == null)
                throw new InvalidOperationException("PotatoHubAcceptedInspectionRequired");
            var result = accepted.InspectionResult;
            return new PotatoHubDispositionSimulationSnapshot
            {
                StableId = "hub-disposition:sim.potato", DataRevision = 1,
                ModeCode = "Simulation", ScenarioStableId = accepted.ScenarioStableId,
                SimulationDate = accepted.SimulationDate, GeneratedAt = accepted.GeneratedAt,
                StateCode = PotatoHubDispositionStateCodes.AcceptedAtHub,
                HubReceivingStableId = accepted.StableId,
                InspectionResultStableId = result.StableId,
                SourceCargoStableId = accepted.Cargo.StableId,
                ReceivedQuantityKg = result.ReceivedQuantityKg,
                AcceptedQuantityKg = result.AcceptedQuantityKg,
                RejectedQuantityKg = result.RejectedQuantityKg,
                RejectionReasonCode = result.RejectionReasonCode,
                SourceStableIds = new[] { accepted.StableId, result.StableId, accepted.Cargo.StableId },
            };
        }
    }
}
