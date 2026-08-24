using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class CooperativeIntakeStateCodes
    {
        public const string AwaitingReview = "AwaitingReview";
        public const string AcceptedForPreparation = "AcceptedForPreparation";
    }

    public sealed class 생산자조합인수LotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CooperativeStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string CustodyStateCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class CooperativeCargoPreparationCandidateData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string IntakeLotStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class CooperativeIntakeSimulationSnapshot
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
        public 생산자조합인수LotSimulationData? IntakeLot { get; set; }
        public CooperativeCargoPreparationCandidateData? CargoPreparationCandidate { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class CooperativeIntakePreview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string DispositionDecisionStableId { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class CooperativeIntakeCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CooperativeIntakeSimulationValidator
    {
        public void Validate(CooperativeIntakeSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId) || snapshot.SimulationDate == default
                || (snapshot.StateCode != CooperativeIntakeStateCodes.AwaitingReview
                    && snapshot.StateCode != CooperativeIntakeStateCodes.AcceptedForPreparation)
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("CooperativeIntakeSnapshotInvalid");
            ValidateHarvest(snapshot.HarvestLot);
            ValidateDisposition(snapshot.DispositionDecision, snapshot.HarvestLot);
            var accepted = snapshot.StateCode == CooperativeIntakeStateCodes.AcceptedForPreparation;
            if (accepted != (snapshot.IntakeLot != null && snapshot.CargoPreparationCandidate != null))
                throw new InvalidOperationException("CooperativeIntakeStateMismatch");
            if (snapshot.IntakeLot != null) ValidateIntake(snapshot.IntakeLot, snapshot.HarvestLot,
                snapshot.DispositionDecision);
            if (snapshot.CargoPreparationCandidate != null)
                ValidateCandidate(snapshot.CargoPreparationCandidate, snapshot.IntakeLot!, snapshot.HarvestLot);
        }

        private static void ValidateHarvest(수확LotSimulationData lot)
        {
            if (lot == null || !StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato" || lot.Quantity != 300m
                || lot.UnitCode != "kg" || lot.SourceStableIds == null || lot.SourceStableIds.Length == 0)
                throw new InvalidOperationException("CooperativeIntakeHarvestLotInvalid");
        }

        private static void ValidateDisposition(HarvestDispositionDecisionData decision,
            수확LotSimulationData harvest)
        {
            if (decision == null || !StableDataId.IsValid(decision.StableId) || decision.Revision <= 0
                || decision.HarvestLotStableId != harvest.StableId
                || decision.ChoiceCode != HarvestDispositionChoiceCodes.CooperativeShipment
                || decision.NextWorkflowCode != "CooperativeIntakeCandidate"
                || decision.Quantity != harvest.Quantity || decision.UnitCode != harvest.UnitCode
                || decision.SourceStableIds == null || !decision.SourceStableIds.Contains(harvest.StableId))
                throw new InvalidOperationException("CooperativeIntakeDispositionInvalid");
        }

        private static void ValidateIntake(생산자조합인수LotSimulationData intake,
            수확LotSimulationData harvest, HarvestDispositionDecisionData decision)
        {
            if (!StableDataId.IsValid(intake.StableId) || intake.Revision <= 0
                || !StableDataId.IsValid(intake.CooperativeStableId)
                || intake.HarvestLotStableId != harvest.StableId
                || intake.CanonicalProductStableId != harvest.CanonicalProductStableId
                || intake.Quantity != harvest.Quantity || intake.UnitCode != harvest.UnitCode
                || intake.CustodyStateCode != CooperativeIntakeStateCodes.AcceptedForPreparation
                || intake.SourceStableIds == null || !intake.SourceStableIds.Contains(harvest.StableId)
                || !intake.SourceStableIds.Contains(decision.StableId))
                throw new InvalidOperationException("CooperativeIntakeLotInvalid");
        }

        private static void ValidateCandidate(CooperativeCargoPreparationCandidateData candidate,
            생산자조합인수LotSimulationData intake, 수확LotSimulationData harvest)
        {
            if (!StableDataId.IsValid(candidate.StableId) || candidate.Revision <= 0
                || candidate.IntakeLotStableId != intake.StableId
                || candidate.HarvestLotStableId != harvest.StableId
                || candidate.NextWorkflowCode != "PotatoHarvestCargoLifecycle"
                || candidate.Quantity != intake.Quantity || candidate.UnitCode != intake.UnitCode
                || candidate.SourceStableIds == null || !candidate.SourceStableIds.Contains(intake.StableId)
                || !candidate.SourceStableIds.Contains(harvest.StableId))
                throw new InvalidOperationException("CooperativeCargoPreparationCandidateInvalid");
        }
    }

    public sealed class CooperativeIntakeSimulationEngine
    {
        private readonly CooperativeIntakeSimulationValidator validator;
        public CooperativeIntakeSimulationEngine(CooperativeIntakeSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public CooperativeIntakePreview Preview(CooperativeIntakeSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != CooperativeIntakeStateCodes.AwaitingReview)
                throw new InvalidOperationException("CooperativeIntakeAlreadyAccepted");
            return new CooperativeIntakePreview
            {
                StableId = "cooperative-intake-preview:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                DispositionDecisionStableId = snapshot.DispositionDecision.StableId,
                RequiresExplicitConfirmation = true,
            };
        }

        public CooperativeIntakeCommand Confirm(CooperativeIntakeSimulationSnapshot snapshot,
            CooperativeIntakePreview preview)
        {
            validator.Validate(snapshot);
            var expected = Preview(snapshot);
            if (preview == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.HarvestLotStableId != expected.HarvestLotStableId
                || preview.DispositionDecisionStableId != expected.DispositionDecisionStableId
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("CooperativeIntakePreviewStaleOrInvalid");
            return new CooperativeIntakeCommand
            {
                StableId = "cooperative-intake-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                PreviewStableId = preview.StableId, SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public CooperativeIntakeSimulationSnapshot Tick(CooperativeIntakeSimulationSnapshot snapshot,
            CooperativeIntakeCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1
                || command.PreviewStableId != Preview(snapshot).StableId)
                throw new InvalidOperationException("CooperativeIntakeCommandStaleOrInvalid");
            var next = Clone(snapshot);
            next.DataRevision++;
            next.StateCode = CooperativeIntakeStateCodes.AcceptedForPreparation;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.IntakeLot = new 생산자조합인수LotSimulationData
            {
                StableId = "cooperative-intake-lot:sim.potato.20260407.r1", Revision = 1,
                CooperativeStableId = "producer-cooperative:sim.local-1",
                HarvestLotStableId = next.HarvestLot.StableId,
                CanonicalProductStableId = next.HarvestLot.CanonicalProductStableId,
                Quantity = next.HarvestLot.Quantity, UnitCode = next.HarvestLot.UnitCode,
                CustodyStateCode = CooperativeIntakeStateCodes.AcceptedForPreparation,
                SourceStableIds = new[] { next.HarvestLot.StableId, next.DispositionDecision.StableId, command.StableId },
            };
            next.CargoPreparationCandidate = new CooperativeCargoPreparationCandidateData
            {
                StableId = "cargo-preparation-candidate:sim.potato.cooperative.r1", Revision = 1,
                IntakeLotStableId = next.IntakeLot.StableId,
                HarvestLotStableId = next.HarvestLot.StableId,
                NextWorkflowCode = "PotatoHarvestCargoLifecycle",
                Quantity = next.IntakeLot.Quantity, UnitCode = next.IntakeLot.UnitCode,
                SourceStableIds = new[] { next.HarvestLot.StableId, next.IntakeLot.StableId, command.StableId },
            };
            validator.Validate(next);
            return next;
        }

        private static CooperativeIntakeSimulationSnapshot Clone(CooperativeIntakeSimulationSnapshot source)
            => new CooperativeIntakeSimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, SimulationDate = source.SimulationDate,
                StateCode = source.StateCode, HarvestLot = source.HarvestLot,
                DispositionDecision = source.DispositionDecision, IntakeLot = source.IntakeLot,
                CargoPreparationCandidate = source.CargoPreparationCandidate,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public sealed class CooperativeIntakePresentationModel
    {
        public string StateText { get; set; } = string.Empty;
        public string IntakeText { get; set; } = string.Empty;
        public string CandidateText { get; set; } = string.Empty;
        public string LineageText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CooperativeIntakeProjector
    {
        private readonly CooperativeIntakeSimulationValidator validator;
        public CooperativeIntakeProjector(CooperativeIntakeSimulationValidator value) => validator = value;
        public CooperativeIntakePresentationModel Project(CooperativeIntakeSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            return new CooperativeIntakePresentationModel
            {
                StateText = snapshot.StateCode + " · REV " + snapshot.DataRevision,
                IntakeText = snapshot.IntakeLot == null ? "조합 인수 검토 전 · 300kg"
                    : snapshot.IntakeLot.StableId + " · 300kg · 인수 준비 승인",
                CandidateText = snapshot.CargoPreparationCandidate == null ? "CARGO-1 NOT CONNECTED"
                    : snapshot.CargoPreparationCandidate.NextWorkflowCode + " · CANDIDATE ONLY",
                LineageText = snapshot.IntakeLot == null ? snapshot.HarvestLot.StableId
                    : snapshot.HarvestLot.StableId + " → " + snapshot.DispositionDecision.StableId
                        + " → " + snapshot.IntakeLot.StableId,
                LimitationText = "조합 정산·실제 포장·상차·운송은 발생하지 않습니다.",
            };
        }
    }

    public static class CooperativeIntakeSimulationFixture
    {
        public static CooperativeIntakeSimulationSnapshot Create(HarvestDispositionSimulationSnapshot disposition)
        {
            new HarvestDispositionSimulationValidator().Validate(disposition);
            if (disposition.StateCode != HarvestDispositionStateCodes.Decided
                || disposition.Decision?.ChoiceCode != HarvestDispositionChoiceCodes.CooperativeShipment)
                throw new InvalidOperationException("CooperativeIntakeDispositionRequired");
            return new CooperativeIntakeSimulationSnapshot
            {
                StableId = "cooperative-intake:sim.potato", DataRevision = 1,
                ModeCode = "Simulation", ScenarioStableId = disposition.ScenarioStableId,
                SimulationDate = disposition.SimulationDate,
                StateCode = CooperativeIntakeStateCodes.AwaitingReview,
                HarvestLot = disposition.HarvestLot, DispositionDecision = disposition.Decision,
                SourceStableIds = new[] { disposition.HarvestLot.StableId, disposition.Decision.StableId },
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class CooperativeHarvestCargoAdapter
    {
        private readonly CooperativeIntakeSimulationValidator validator;
        public CooperativeHarvestCargoAdapter(CooperativeIntakeSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public 감자수확CargoSimulationSnapshot Create(CooperativeIntakeSimulationSnapshot intake)
        {
            validator.Validate(intake);
            if (intake.StateCode != CooperativeIntakeStateCodes.AcceptedForPreparation
                || intake.IntakeLot == null || intake.CargoPreparationCandidate == null)
                throw new InvalidOperationException("CooperativeCargoPreparationCandidateRequired");
            var result = 감자수확CargoSimulationFixture.Create(intake.HarvestLot);
            result.SourceStableIds = result.SourceStableIds.Concat(new[]
            {
                intake.DispositionDecision.StableId,
                intake.IntakeLot.StableId,
                intake.CargoPreparationCandidate.StableId,
            }).Distinct(StringComparer.Ordinal).ToArray();
            new 감자수확CargoSimulationValidator().Validate(result);
            return result;
        }
    }
}
