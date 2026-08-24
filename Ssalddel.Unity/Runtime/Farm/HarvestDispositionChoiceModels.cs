using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class HarvestDispositionChoiceCodes
    {
        public const string CooperativeShipment = "CooperativeShipment";
        public const string DirectOnlineSale = "DirectOnlineSale";
        public const string ExportAgent = "ExportAgent";
        public const string ReserveStorage = "ReserveStorage";

        public static bool IsKnown(string value) => value == CooperativeShipment
            || value == DirectOnlineSale || value == ExportAgent || value == ReserveStorage;
    }

    public static class HarvestDispositionWorkflowCodes
    {
        public const string CooperativeIntakeCandidate = "CooperativeIntakeCandidate";
        public const string ProducerPackingCandidate = "ProducerPackingCandidate";
        public const string ExportReadinessCandidate = "ExportReadinessCandidate";
        public const string ReserveStockLotCandidate = "ReserveStockLotCandidate";

        public static string ForChoice(string choiceCode)
        {
            switch (choiceCode)
            {
                case HarvestDispositionChoiceCodes.CooperativeShipment:
                    return CooperativeIntakeCandidate;
                case HarvestDispositionChoiceCodes.DirectOnlineSale:
                    return ProducerPackingCandidate;
                case HarvestDispositionChoiceCodes.ExportAgent:
                    return ExportReadinessCandidate;
                case HarvestDispositionChoiceCodes.ReserveStorage:
                    return ReserveStockLotCandidate;
                default:
                    throw new InvalidOperationException("HarvestDispositionChoiceUnknown:" + choiceCode);
            }
        }
    }

    public static class HarvestDispositionStateCodes
    {
        public const string AwaitingChoice = "AwaitingChoice";
        public const string Decided = "Decided";
    }

    public sealed class HarvestDispositionOptionSnapshot
    {
        public string ChoiceCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class HarvestDispositionDecisionData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class HarvestDispositionSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public 수확LotSimulationData HarvestLot { get; set; } = new 수확LotSimulationData();
        public HarvestDispositionOptionSnapshot[] Options { get; set; }
            = Array.Empty<HarvestDispositionOptionSnapshot>();
        public HarvestDispositionDecisionData? Decision { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class HarvestDispositionPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class HarvestDispositionCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string ChoiceCode { get; set; } = string.Empty;
        public long SimulationTick { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class HarvestDispositionSimulationValidator
    {
        public void Validate(HarvestDispositionSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || snapshot.SimulationDate == default
                || (snapshot.StateCode != HarvestDispositionStateCodes.AwaitingChoice
                    && snapshot.StateCode != HarvestDispositionStateCodes.Decided)
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("HarvestDispositionSnapshotInvalid");
            ValidateHarvestLot(snapshot.HarvestLot);
            ValidateOptions(snapshot.Options);
            var decided = snapshot.StateCode == HarvestDispositionStateCodes.Decided;
            if (decided != (snapshot.Decision != null))
                throw new InvalidOperationException("HarvestDispositionDecisionStateMismatch");
            if (snapshot.Decision != null) ValidateDecision(snapshot, snapshot.Decision);
        }

        private static void ValidateHarvestLot(수확LotSimulationData lot)
        {
            if (lot == null || !StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato" || lot.Quantity != 300m
                || lot.UnitCode != "kg" || lot.SourceStableIds == null || lot.SourceStableIds.Length == 0)
                throw new InvalidOperationException("HarvestDispositionHarvestLotInvalid");
        }

        private static void ValidateOptions(HarvestDispositionOptionSnapshot[] options)
        {
            if (options == null || options.Length != 4
                || options.Select(value => value.ChoiceCode).Distinct(StringComparer.Ordinal).Count() != 4
                || options.Any(value => value == null || !HarvestDispositionChoiceCodes.IsKnown(value.ChoiceCode)
                    || string.IsNullOrWhiteSpace(value.DisplayName)
                    || string.IsNullOrWhiteSpace(value.NextWorkflowCode)
                    || string.IsNullOrWhiteSpace(value.Summary)
                    || value.Limitations == null || value.Limitations.Length == 0
                    || value.Limitations.Any(string.IsNullOrWhiteSpace)))
                throw new InvalidOperationException("HarvestDispositionOptionsInvalid");
        }

        private static void ValidateDecision(HarvestDispositionSimulationSnapshot snapshot,
            HarvestDispositionDecisionData decision)
        {
            var option = snapshot.Options.SingleOrDefault(value => value.ChoiceCode == decision.ChoiceCode);
            if (option == null || !StableDataId.IsValid(decision.StableId) || decision.Revision <= 0
                || decision.HarvestLotStableId != snapshot.HarvestLot.StableId
                || decision.NextWorkflowCode != option.NextWorkflowCode
                || decision.Quantity != snapshot.HarvestLot.Quantity
                || decision.UnitCode != snapshot.HarvestLot.UnitCode
                || decision.SourceStableIds == null
                || !decision.SourceStableIds.Contains(snapshot.HarvestLot.StableId))
                throw new InvalidOperationException("HarvestDispositionDecisionInvalid");
        }
    }

    public sealed class HarvestDispositionSimulationEngine
    {
        private readonly HarvestDispositionSimulationValidator validator;

        public HarvestDispositionSimulationEngine(HarvestDispositionSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public HarvestDispositionPreview Preview(HarvestDispositionSimulationSnapshot snapshot, string choiceCode)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != HarvestDispositionStateCodes.AwaitingChoice)
                throw new InvalidOperationException("HarvestDispositionAlreadyDecided");
            var option = snapshot.Options.SingleOrDefault(value => value.ChoiceCode == choiceCode)
                ?? throw new InvalidOperationException("HarvestDispositionChoiceUnknown:" + choiceCode);
            return new HarvestDispositionPreview
            {
                StableId = "harvest-disposition-preview:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture) + "." + choiceCode.ToLowerInvariant(),
                SnapshotStableId = snapshot.StableId, ExpectedDataRevision = snapshot.DataRevision,
                HarvestLotStableId = snapshot.HarvestLot.StableId, ChoiceCode = choiceCode,
                NextWorkflowCode = option.NextWorkflowCode, RequiresExplicitConfirmation = true,
            };
        }

        public HarvestDispositionCommand Confirm(HarvestDispositionSimulationSnapshot snapshot,
            HarvestDispositionPreview preview)
        {
            validator.Validate(snapshot);
            var expected = preview == null ? null : Preview(snapshot, preview.ChoiceCode);
            if (preview == null || expected == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.HarvestLotStableId != expected.HarvestLotStableId
                || preview.NextWorkflowCode != expected.NextWorkflowCode
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("HarvestDispositionPreviewStaleOrInvalid");
            return new HarvestDispositionCommand
            {
                StableId = "harvest-disposition-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture) + "."
                    + preview.ChoiceCode.ToLowerInvariant(),
                PreviewStableId = preview.StableId, SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision, ChoiceCode = preview.ChoiceCode,
                SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public HarvestDispositionSimulationSnapshot Tick(HarvestDispositionSimulationSnapshot snapshot,
            HarvestDispositionCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
                throw new InvalidOperationException("HarvestDispositionCommandStaleOrInvalid");
            var expected = Preview(snapshot, command.ChoiceCode);
            if (command.PreviewStableId != expected.StableId)
                throw new InvalidOperationException("HarvestDispositionCommandPreviewMismatch");
            var option = snapshot.Options.Single(value => value.ChoiceCode == command.ChoiceCode);
            var next = Clone(snapshot);
            next.DataRevision++;
            next.StateCode = HarvestDispositionStateCodes.Decided;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.Decision = new HarvestDispositionDecisionData
            {
                StableId = "harvest-disposition:sim.potato.20260407.r1", Revision = 1,
                HarvestLotStableId = next.HarvestLot.StableId, ChoiceCode = option.ChoiceCode,
                NextWorkflowCode = option.NextWorkflowCode, Quantity = next.HarvestLot.Quantity,
                UnitCode = next.HarvestLot.UnitCode,
                SourceStableIds = new[] { next.HarvestLot.StableId, command.StableId },
            };
            validator.Validate(next);
            return next;
        }

        private static HarvestDispositionSimulationSnapshot Clone(HarvestDispositionSimulationSnapshot source)
            => new HarvestDispositionSimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, SimulationDate = source.SimulationDate,
                StateCode = source.StateCode, HarvestLot = source.HarvestLot,
                Options = source.Options.ToArray(), Decision = source.Decision,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public sealed class HarvestDispositionCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string HarvestText { get; set; } = string.Empty;
        public string StateText { get; set; } = string.Empty;
        public string DecisionText { get; set; } = string.Empty;
        public HarvestDispositionOptionSnapshot[] Options { get; set; }
            = Array.Empty<HarvestDispositionOptionSnapshot>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class HarvestDispositionProjector
    {
        private readonly HarvestDispositionSimulationValidator validator;
        public HarvestDispositionProjector(HarvestDispositionSimulationValidator value) => validator = value;

        public HarvestDispositionCardModel Project(HarvestDispositionSimulationSnapshot source)
        {
            validator.Validate(source);
            return new HarvestDispositionCardModel
            {
                Title = "수확한 감자의 판로를 선택하세요",
                HarvestText = source.HarvestLot.StableId + " · 300kg · "
                    + source.SimulationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                StateText = source.StateCode,
                DecisionText = source.Decision == null ? "아직 판로를 결정하지 않았습니다."
                    : source.Decision.ChoiceCode + " → " + source.Decision.NextWorkflowCode,
                Options = source.Options.ToArray(),
            };
        }
    }

    public static class HarvestDispositionSimulationFixture
    {
        public static HarvestDispositionSimulationSnapshot Create(감자재배LifecycleSimulationSnapshot harvested)
        {
            new 감자재배LifecycleSimulationValidator(
                new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator()).Validate(harvested);
            if (harvested.Cultivation?.GrowthStageCode != 재배생육단계Codes.Harvested
                || harvested.HarvestLot == null)
                throw new InvalidOperationException("HarvestDispositionHarvestedLotRequired");
            return new HarvestDispositionSimulationSnapshot
            {
                StableId = "harvest-disposition-session:sim.potato", DataRevision = 1,
                ModeCode = "Simulation", ScenarioStableId = harvested.ScenarioStableId,
                SimulationDate = harvested.SimulationDate, StateCode = HarvestDispositionStateCodes.AwaitingChoice,
                HarvestLot = harvested.HarvestLot,
                SourceStableIds = new[] { harvested.HarvestLot.StableId, "source:fixture.harvest-disposition" },
                Options = new[]
                {
                    new HarvestDispositionOptionSnapshot
                    {
                        ChoiceCode = HarvestDispositionChoiceCodes.CooperativeShipment,
                        DisplayName = "생산자 조합에 출하",
                        NextWorkflowCode = HarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
                        Summary = "조합 인수 뒤 공동 선별·포장·출하를 준비합니다.",
                        Limitations = new[] { "실제 조합 인수나 정산을 확정하지 않습니다." },
                    },
                    new HarvestDispositionOptionSnapshot
                    {
                        ChoiceCode = HarvestDispositionChoiceCodes.DirectOnlineSale,
                        DisplayName = "온라인 마켓 직접 판매",
                        NextWorkflowCode = HarvestDispositionWorkflowCodes.ProducerPackingCandidate,
                        Summary = "생산자가 선별·포장하고 온라인 판매 준비를 시작합니다.",
                        Limitations = new[] { "상품 등록·주문·결제·택배를 만들지 않습니다." },
                    },
                    new HarvestDispositionOptionSnapshot
                    {
                        ChoiceCode = HarvestDispositionChoiceCodes.ExportAgent,
                        DisplayName = "수출대행 준비",
                        NextWorkflowCode = HarvestDispositionWorkflowCodes.ExportReadinessCandidate,
                        Summary = "전문 포장과 배송대행지 인계를 검토합니다.",
                        Limitations = new[] { "수출계약·검사·통관·운송을 확정하지 않습니다." },
                    },
                    new HarvestDispositionOptionSnapshot
                    {
                        ChoiceCode = HarvestDispositionChoiceCodes.ReserveStorage,
                        DisplayName = "정착지 비축 창고에 보관",
                        NextWorkflowCode = HarvestDispositionWorkflowCodes.ReserveStockLotCandidate,
                        Summary = "창고 용량과 감모를 검토하고 비축 재고 Lot 후보를 준비합니다.",
                        Limitations = new[] { "용량·감모·비용·노동·식량안보 영향은 서버 Preview가 계산합니다." },
                    },
                },
            };
        }
    }
}
