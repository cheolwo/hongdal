using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoHubReceivingStateCodes
    {
        public const string ArrivedAtHub = "ArrivedAtHub";
        public const string Inspection = "Inspection";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
        internal static bool IsKnown(string value) => value == ArrivedAtHub || value == Inspection
            || value == Accepted || value == Rejected;
    }

    public static class PotatoHubReceivingCommandCodes
    {
        public const string StartInspection = "StartInspection";
        public const string CompleteInspection = "CompleteInspection";
    }

    public sealed class PotatoHubInspectionSimulationRuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public decimal AcceptedQuantityKg { get; set; }
        public decimal RejectedQuantityKg { get; set; }
        public string RejectionReasonCode { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubInspectionResultData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public decimal ReceivedQuantityKg { get; set; }
        public decimal AcceptedQuantityKg { get; set; }
        public decimal RejectedQuantityKg { get; set; }
        public string RejectionReasonCode { get; set; } = string.Empty;
        public string DecisionCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubReceivingSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public 화물LotSimulationData Cargo { get; set; } = new 화물LotSimulationData();
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public PotatoHubInspectionSimulationRuleSnapshot Rule { get; set; }
            = new PotatoHubInspectionSimulationRuleSnapshot();
        public PotatoHubInspectionResultData? InspectionResult { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoHubReceivingPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long ExpectedCargoRevision { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public decimal AcceptedQuantityKg { get; set; }
        public decimal RejectedQuantityKg { get; set; }
        public string RejectionReasonCode { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class PotatoHubReceivingCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long ExpectedCargoRevision { get; set; }
        public long SimulationTick { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoHubReceivingSimulationValidator
    {
        public void Validate(PotatoHubReceivingSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || snapshot.SimulationDate == default || snapshot.GeneratedAt == default
                || !PotatoHubReceivingStateCodes.IsKnown(snapshot.StateCode) || snapshot.CargoRevision <= 0
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("PotatoHubReceivingSnapshotInvalid");
            ValidateCargo(snapshot);
            ValidateRule(snapshot.Rule, snapshot.Cargo.Quantity);
            ValidateResult(snapshot);
        }

        private static void ValidateCargo(PotatoHubReceivingSimulationSnapshot snapshot)
        {
            var cargo = snapshot.Cargo;
            if (cargo == null || !StableDataId.IsValid(cargo.StableId)
                || cargo.CanonicalProductStableId != "product:potato"
                || cargo.HarvestLotStableId != snapshot.HarvestLotStableId
                || cargo.PackageLotStableId != snapshot.PackageLotStableId
                || cargo.PackageCount != 15 || cargo.Quantity != 300m || cargo.UnitCode != "kg"
                || cargo.StateCode != PotatoCargoJourneyStateCodes.ArrivedAtHub)
                throw new InvalidOperationException("PotatoHubReceivingCargoInvalid");
        }

        private static void ValidateRule(PotatoHubInspectionSimulationRuleSnapshot rule, decimal quantity)
        {
            if (rule == null || !StableDataId.IsValid(rule.StableId) || rule.Revision <= 0
                || rule.AcceptedQuantityKg < 0 || rule.RejectedQuantityKg < 0
                || rule.AcceptedQuantityKg + rule.RejectedQuantityKg != quantity
                || (rule.RejectedQuantityKg > 0) != !string.IsNullOrWhiteSpace(rule.RejectionReasonCode)
                || rule.SourceTypeCode != "Fixture" || rule.SourceStableIds == null
                || rule.SourceStableIds.Length == 0 || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("PotatoHubInspectionRuleInvalid");
        }

        private static void ValidateResult(PotatoHubReceivingSimulationSnapshot snapshot)
        {
            var result = snapshot.InspectionResult;
            var final = snapshot.StateCode == PotatoHubReceivingStateCodes.Accepted
                || snapshot.StateCode == PotatoHubReceivingStateCodes.Rejected;
            if (final != (result != null)) throw new InvalidOperationException("PotatoHubInspectionResultStateMismatch");
            if (result == null) return;
            if (!StableDataId.IsValid(result.StableId) || result.Revision <= 0
                || result.CargoStableId != snapshot.Cargo.StableId || result.CargoRevision != snapshot.CargoRevision
                || result.ReceivedQuantityKg != snapshot.Cargo.Quantity
                || result.AcceptedQuantityKg + result.RejectedQuantityKg != result.ReceivedQuantityKg
                || result.AcceptedQuantityKg != snapshot.Rule.AcceptedQuantityKg
                || result.RejectedQuantityKg != snapshot.Rule.RejectedQuantityKg
                || result.RejectionReasonCode != snapshot.Rule.RejectionReasonCode
                || result.DecisionCode != snapshot.StateCode || result.SourceStableIds == null
                || !result.SourceStableIds.Contains(snapshot.Cargo.StableId))
                throw new InvalidOperationException("PotatoHubInspectionResultInvalid");
        }
    }

    public sealed class PotatoHubReceivingSimulationEngine
    {
        private readonly PotatoHubReceivingSimulationValidator validator;
        public PotatoHubReceivingSimulationEngine(PotatoHubReceivingSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public PotatoHubReceivingPreview PreviewReceiving(PotatoHubReceivingSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoHubReceivingStateCodes.ArrivedAtHub)
                throw new InvalidOperationException("PotatoHubReceivingStateInvalid");
            return Preview(snapshot, PotatoHubReceivingCommandCodes.StartInspection, 0, 0, string.Empty);
        }

        public PotatoHubReceivingPreview PreviewInspection(PotatoHubReceivingSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoHubReceivingStateCodes.Inspection)
                throw new InvalidOperationException("PotatoHubInspectionStateInvalid");
            return Preview(snapshot, PotatoHubReceivingCommandCodes.CompleteInspection,
                snapshot.Rule.AcceptedQuantityKg, snapshot.Rule.RejectedQuantityKg,
                snapshot.Rule.RejectionReasonCode);
        }

        public PotatoHubReceivingCommand Confirm(PotatoHubReceivingSimulationSnapshot snapshot,
            PotatoHubReceivingPreview preview)
        {
            validator.Validate(snapshot);
            var expected = preview?.CommandCode == PotatoHubReceivingCommandCodes.StartInspection
                ? PreviewReceiving(snapshot) : PreviewInspection(snapshot);
            if (preview == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.ExpectedCargoRevision != expected.ExpectedCargoRevision
                || preview.CargoStableId != expected.CargoStableId
                || preview.AcceptedQuantityKg != expected.AcceptedQuantityKg
                || preview.RejectedQuantityKg != expected.RejectedQuantityKg
                || preview.RejectionReasonCode != expected.RejectionReasonCode
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("PotatoHubReceivingPreviewStaleOrInvalid");
            return new PotatoHubReceivingCommand
            {
                StableId = "hub-" + preview.CommandCode.ToLowerInvariant() + "-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = preview.CommandCode, PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId, ExpectedDataRevision = snapshot.DataRevision,
                ExpectedCargoRevision = snapshot.CargoRevision, SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public PotatoHubReceivingSimulationSnapshot Tick(PotatoHubReceivingSimulationSnapshot snapshot,
            PotatoHubReceivingCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.ExpectedCargoRevision != snapshot.CargoRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
                throw new InvalidOperationException("PotatoHubReceivingCommandInvalid");
            var expected = command.CommandCode == PotatoHubReceivingCommandCodes.StartInspection
                ? PreviewReceiving(snapshot) : command.CommandCode == PotatoHubReceivingCommandCodes.CompleteInspection
                    ? PreviewInspection(snapshot) : throw new InvalidOperationException("PotatoHubReceivingCommandInvalid");
            if (command.PreviewStableId != expected.StableId)
                throw new InvalidOperationException("PotatoHubReceivingCommandInvalid");
            var next = Clone(snapshot); next.DataRevision++; next.CargoRevision++;
            next.Cargo.Revision = next.CargoRevision;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.Cargo.SourceStableIds = next.Cargo.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            if (command.CommandCode == PotatoHubReceivingCommandCodes.StartInspection)
                next.StateCode = PotatoHubReceivingStateCodes.Inspection;
            else
            {
                next.StateCode = next.Rule.AcceptedQuantityKg > 0
                    ? PotatoHubReceivingStateCodes.Accepted : PotatoHubReceivingStateCodes.Rejected;
                next.InspectionResult = new PotatoHubInspectionResultData
                {
                    StableId = "inspection-result:sim.potato."
                        + next.SimulationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                        + ".r" + next.DataRevision, Revision = 1,
                    CargoStableId = next.Cargo.StableId, CargoRevision = next.CargoRevision,
                    ReceivedQuantityKg = next.Cargo.Quantity,
                    AcceptedQuantityKg = next.Rule.AcceptedQuantityKg,
                    RejectedQuantityKg = next.Rule.RejectedQuantityKg,
                    RejectionReasonCode = next.Rule.RejectionReasonCode,
                    DecisionCode = next.StateCode,
                    SourceStableIds = new[] { next.Cargo.StableId, command.StableId },
                };
            }
            validator.Validate(next); return next;
        }

        private static PotatoHubReceivingPreview Preview(PotatoHubReceivingSimulationSnapshot s,
            string code, decimal accepted, decimal rejected, string reason)
            => new PotatoHubReceivingPreview
            {
                StableId = "hub-" + code.ToLowerInvariant() + "-preview:sim.potato.r" + s.DataRevision,
                CommandCode = code, SnapshotStableId = s.StableId,
                ExpectedDataRevision = s.DataRevision, ExpectedCargoRevision = s.CargoRevision,
                CargoStableId = s.Cargo.StableId, AcceptedQuantityKg = accepted,
                RejectedQuantityKg = rejected, RejectionReasonCode = reason,
                RequiresExplicitConfirmation = true,
            };

        private static PotatoHubReceivingSimulationSnapshot Clone(PotatoHubReceivingSimulationSnapshot s)
            => new PotatoHubReceivingSimulationSnapshot
            {
                StableId=s.StableId,DataRevision=s.DataRevision,ModeCode=s.ModeCode,ScenarioStableId=s.ScenarioStableId,
                SimulationDate=s.SimulationDate,GeneratedAt=s.GeneratedAt,StateCode=s.StateCode,CargoRevision=s.CargoRevision,
                HarvestLotStableId=s.HarvestLotStableId,PackageLotStableId=s.PackageLotStableId,SourceStableIds=s.SourceStableIds.ToArray(),
                Cargo=new 화물LotSimulationData{StableId=s.Cargo.StableId,Revision=s.Cargo.Revision,CanonicalProductStableId=s.Cargo.CanonicalProductStableId,HarvestLotStableId=s.Cargo.HarvestLotStableId,PackageLotStableId=s.Cargo.PackageLotStableId,OriginStableId=s.Cargo.OriginStableId,DestinationStableId=s.Cargo.DestinationStableId,StateCode=s.Cargo.StateCode,PackageCount=s.Cargo.PackageCount,Quantity=s.Cargo.Quantity,UnitCode=s.Cargo.UnitCode,VehicleCapacityKg=s.Cargo.VehicleCapacityKg,SourceStableIds=s.Cargo.SourceStableIds.ToArray()},
                Rule=new PotatoHubInspectionSimulationRuleSnapshot{StableId=s.Rule.StableId,Revision=s.Rule.Revision,AcceptedQuantityKg=s.Rule.AcceptedQuantityKg,RejectedQuantityKg=s.Rule.RejectedQuantityKg,RejectionReasonCode=s.Rule.RejectionReasonCode,SourceTypeCode=s.Rule.SourceTypeCode,SourceStableIds=s.Rule.SourceStableIds.ToArray(),Limitations=s.Rule.Limitations.ToArray()},
                InspectionResult=s.InspectionResult,
            };
    }

    public sealed class PotatoHubReceivingPresentationModel
    {
        public string StateCode { get; set; }=string.Empty;
        public string CargoText { get; set; }=string.Empty;
        public string InspectionText { get; set; }=string.Empty;
        public string LineageText { get; set; }=string.Empty;
        public string LimitationText { get; set; }=string.Empty;
        public bool CanReviewReceiving { get; set; }
        public bool CanReviewInspection { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoHubReceivingProjector
    {
        private readonly PotatoHubReceivingSimulationValidator validator;
        public PotatoHubReceivingProjector(PotatoHubReceivingSimulationValidator value)=>validator=value??throw new ArgumentNullException(nameof(value));
        public PotatoHubReceivingPresentationModel Project(PotatoHubReceivingSimulationSnapshot s)
        {validator.Validate(s);return new PotatoHubReceivingPresentationModel{StateCode=s.StateCode,CargoText=s.Cargo.StableId+" · rev "+s.CargoRevision+" · "+s.Cargo.Quantity+"kg",InspectionText=s.InspectionResult==null?"NOT DECIDED":s.InspectionResult.DecisionCode+" · accepted "+s.InspectionResult.AcceptedQuantityKg+"kg · rejected "+s.InspectionResult.RejectedQuantityKg+"kg · "+s.InspectionResult.RejectionReasonCode,LineageText=s.HarvestLotStableId+" → "+s.PackageLotStableId+" → "+s.Cargo.StableId,LimitationText=string.Join(" · ",s.Rule.Limitations),CanReviewReceiving=s.StateCode==PotatoHubReceivingStateCodes.ArrivedAtHub,CanReviewInspection=s.StateCode==PotatoHubReceivingStateCodes.Inspection};}
    }

    public static class PotatoHubReceivingSimulationFixture
    {
        public static PotatoHubReceivingSimulationSnapshot Create(PotatoCargoJourneySimulationSnapshot arrived)
        {new PotatoCargoJourneySimulationValidator().Validate(arrived);if(arrived.StateCode!=PotatoCargoJourneyStateCodes.ArrivedAtHub)throw new InvalidOperationException("PotatoHubArrivedCargoRequired");return new PotatoHubReceivingSimulationSnapshot{StableId="hub-receiving:sim.potato",DataRevision=1,ModeCode="Simulation",ScenarioStableId=arrived.ScenarioStableId,SimulationDate=arrived.SimulationDate,GeneratedAt=arrived.GeneratedAt,StateCode=PotatoHubReceivingStateCodes.ArrivedAtHub,CargoRevision=arrived.CargoRevision,HarvestLotStableId=arrived.HarvestLotStableId,PackageLotStableId=arrived.PackageLotStableId,SourceStableIds=new[]{arrived.HarvestLotStableId,arrived.PackageLotStableId,arrived.Cargo.StableId,"source:fixture.potato-hub-inspection"},Cargo=new 화물LotSimulationData{StableId=arrived.Cargo.StableId,Revision=arrived.Cargo.Revision,CanonicalProductStableId=arrived.Cargo.CanonicalProductStableId,HarvestLotStableId=arrived.Cargo.HarvestLotStableId,PackageLotStableId=arrived.Cargo.PackageLotStableId,OriginStableId=arrived.Cargo.OriginStableId,DestinationStableId=arrived.Cargo.DestinationStableId,StateCode=arrived.Cargo.StateCode,PackageCount=arrived.Cargo.PackageCount,Quantity=arrived.Cargo.Quantity,UnitCode=arrived.Cargo.UnitCode,VehicleCapacityKg=arrived.Cargo.VehicleCapacityKg,SourceStableIds=arrived.Cargo.SourceStableIds.ToArray()},Rule=new PotatoHubInspectionSimulationRuleSnapshot{StableId="inspection-rule:sim.potato.hub",Revision=1,AcceptedQuantityKg=288m,RejectedQuantityKg=12m,RejectionReasonCode="DamageFixture",SourceTypeCode="Fixture",SourceStableIds=new[]{"source:fixture.potato-hub-inspection"},Limitations=new[]{"288kg 합격과 12kg 손실은 Simulation 규칙이며 실제 품질 판정이나 재고 입고가 아닙니다."}}};}
    }
}
