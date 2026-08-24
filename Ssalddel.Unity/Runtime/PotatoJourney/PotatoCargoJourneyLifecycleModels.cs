using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoCargoJourneyStateCodes
    {
        public const string Loaded = "Loaded";
        public const string InTransit = "InTransit";
        public const string ArrivedAtHub = "ArrivedAtHub";
        internal static bool IsKnown(string value)
            => value == Loaded || value == InTransit || value == ArrivedAtHub;
    }

    public static class PotatoCargoJourneyCommandCodes
    {
        public const string Dispatch = "Dispatch";
        public const string AdvanceRoute = "AdvanceRoute";
    }

    public sealed class PotatoCargoJourneySimulationRuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginStableId { get; set; } = string.Empty;
        public string DestinationStableId { get; set; } = string.Empty;
        public int RequiredRouteTicks { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoCargoJourneySimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public int CompletedRouteTicks { get; set; }
        public long CargoRevision { get; set; }
        public 화물LotSimulationData Cargo { get; set; } = new 화물LotSimulationData();
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public PotatoCargoJourneySimulationRuleSnapshot Rule { get; set; }
            = new PotatoCargoJourneySimulationRuleSnapshot();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoCargoJourneyPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long ExpectedCargoRevision { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class PotatoCargoJourneyCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string? PreviewStableId { get; set; }
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long ExpectedCargoRevision { get; set; }
        public long SimulationTick { get; set; }
        public int RouteTicks { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoCargoJourneySimulationValidator
    {
        public void Validate(PotatoCargoJourneySimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || snapshot.SimulationDate == default || snapshot.SimulationDate.Offset != TimeSpan.Zero
                || snapshot.GeneratedAt == default || !PotatoCargoJourneyStateCodes.IsKnown(snapshot.StateCode)
                || snapshot.CompletedRouteTicks < 0 || snapshot.CargoRevision <= 0
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || snapshot.SourceStableIds.Distinct(StringComparer.Ordinal).Count() != snapshot.SourceStableIds.Length)
                throw new InvalidOperationException("PotatoCargoJourneySnapshotInvalid");

            ValidateRule(snapshot.Rule);
            ValidateCargo(snapshot);
            if (snapshot.CompletedRouteTicks > snapshot.Rule.RequiredRouteTicks
                || (snapshot.StateCode == PotatoCargoJourneyStateCodes.Loaded && snapshot.CompletedRouteTicks != 0)
                || (snapshot.StateCode == PotatoCargoJourneyStateCodes.InTransit
                    && snapshot.CompletedRouteTicks >= snapshot.Rule.RequiredRouteTicks)
                || (snapshot.StateCode == PotatoCargoJourneyStateCodes.ArrivedAtHub
                    && snapshot.CompletedRouteTicks != snapshot.Rule.RequiredRouteTicks))
                throw new InvalidOperationException("PotatoCargoJourneyProgressInvalid");
        }

        private static void ValidateRule(PotatoCargoJourneySimulationRuleSnapshot rule)
        {
            if (rule == null || !StableDataId.IsValid(rule.StableId) || rule.Revision <= 0
                || !StableDataId.IsValid(rule.RouteStableId) || !StableDataId.IsValid(rule.OriginStableId)
                || !StableDataId.IsValid(rule.DestinationStableId) || rule.OriginStableId == rule.DestinationStableId
                || rule.RequiredRouteTicks <= 0 || rule.SourceTypeCode != "Fixture"
                || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0
                || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("PotatoCargoJourneyRuleInvalid");
        }

        private static void ValidateCargo(PotatoCargoJourneySimulationSnapshot snapshot)
        {
            var cargo = snapshot.Cargo;
            if (cargo == null || !StableDataId.IsValid(cargo.StableId)
                || cargo.CanonicalProductStableId != "product:potato"
                || cargo.HarvestLotStableId != snapshot.HarvestLotStableId
                || cargo.PackageLotStableId != snapshot.PackageLotStableId
                || !StableDataId.IsValid(snapshot.HarvestLotStableId)
                || !StableDataId.IsValid(snapshot.PackageLotStableId)
                || cargo.PackageCount != 15 || cargo.Quantity != 300m || cargo.UnitCode != "kg"
                || cargo.VehicleCapacityKg != 400m || cargo.Quantity > cargo.VehicleCapacityKg
                || cargo.OriginStableId != snapshot.Rule.OriginStableId
                || cargo.DestinationStableId != snapshot.Rule.DestinationStableId
                || cargo.SourceStableIds == null || !cargo.SourceStableIds.Contains(snapshot.HarvestLotStableId)
                || !cargo.SourceStableIds.Contains(snapshot.PackageLotStableId))
                throw new InvalidOperationException("PotatoCargoJourneyCargoInvalid");
        }
    }

    public sealed class PotatoCargoJourneySimulationEngine
    {
        private readonly PotatoCargoJourneySimulationValidator validator;
        public PotatoCargoJourneySimulationEngine(PotatoCargoJourneySimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public PotatoCargoJourneyPreview PreviewDispatch(PotatoCargoJourneySimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoCargoJourneyStateCodes.Loaded)
                throw new InvalidOperationException("PotatoCargoJourneyDispatchStateInvalid");
            return new PotatoCargoJourneyPreview
            {
                StableId = "journey-dispatch-preview:sim.potato.r" + snapshot.DataRevision,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                ExpectedCargoRevision = snapshot.CargoRevision,
                CargoStableId = snapshot.Cargo.StableId,
                RequiresExplicitConfirmation = true,
            };
        }

        public PotatoCargoJourneyCommand ConfirmDispatch(PotatoCargoJourneySimulationSnapshot snapshot,
            PotatoCargoJourneyPreview preview)
        {
            var expected = PreviewDispatch(snapshot);
            if (preview == null || preview.StableId != expected.StableId
                || preview.SnapshotStableId != expected.SnapshotStableId
                || preview.ExpectedDataRevision != expected.ExpectedDataRevision
                || preview.ExpectedCargoRevision != expected.ExpectedCargoRevision
                || preview.CargoStableId != expected.CargoStableId || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("PotatoCargoJourneyPreviewStaleOrInvalid");
            return Command(snapshot, PotatoCargoJourneyCommandCodes.Dispatch, preview.StableId, 0);
        }

        public PotatoCargoJourneyCommand CreateAdvanceRouteCommand(
            PotatoCargoJourneySimulationSnapshot snapshot, int routeTicks)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != PotatoCargoJourneyStateCodes.InTransit || routeTicks <= 0
                || snapshot.CompletedRouteTicks + routeTicks > snapshot.Rule.RequiredRouteTicks)
                throw new InvalidOperationException("PotatoCargoJourneyAdvanceInvalid");
            return Command(snapshot, PotatoCargoJourneyCommandCodes.AdvanceRoute, null, routeTicks);
        }

        public PotatoCargoJourneySimulationSnapshot Tick(PotatoCargoJourneySimulationSnapshot snapshot,
            PotatoCargoJourneyCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.ExpectedCargoRevision != snapshot.CargoRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
                throw new InvalidOperationException("PotatoCargoJourneyCommandInvalid");
            if (command.CommandCode == PotatoCargoJourneyCommandCodes.Dispatch)
            {
                if (command.PreviewStableId != PreviewDispatch(snapshot).StableId)
                    throw new InvalidOperationException("PotatoCargoJourneyCommandInvalid");
            }
            else if (command.CommandCode == PotatoCargoJourneyCommandCodes.AdvanceRoute)
            {
                if (command.PreviewStableId != null || command.RouteTicks <= 0
                    || snapshot.StateCode != PotatoCargoJourneyStateCodes.InTransit
                    || snapshot.CompletedRouteTicks + command.RouteTicks > snapshot.Rule.RequiredRouteTicks)
                    throw new InvalidOperationException("PotatoCargoJourneyCommandInvalid");
            }
            else throw new InvalidOperationException("PotatoCargoJourneyCommandInvalid");

            var next = Clone(snapshot);
            next.DataRevision++;
            next.CargoRevision++;
            next.Cargo.Revision = next.CargoRevision;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.Cargo.SourceStableIds = next.Cargo.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            if (command.CommandCode == PotatoCargoJourneyCommandCodes.Dispatch)
            {
                next.StateCode = PotatoCargoJourneyStateCodes.InTransit;
                next.Cargo.StateCode = PotatoCargoJourneyStateCodes.InTransit;
            }
            else
            {
                next.CompletedRouteTicks += command.RouteTicks;
                next.SimulationDate = next.SimulationDate.AddDays(command.RouteTicks);
                if (next.CompletedRouteTicks == next.Rule.RequiredRouteTicks)
                {
                    next.StateCode = PotatoCargoJourneyStateCodes.ArrivedAtHub;
                    next.Cargo.StateCode = PotatoCargoJourneyStateCodes.ArrivedAtHub;
                }
            }
            validator.Validate(next);
            return next;
        }

        private static PotatoCargoJourneyCommand Command(PotatoCargoJourneySimulationSnapshot snapshot,
            string code, string? previewStableId, int routeTicks)
            => new PotatoCargoJourneyCommand
            {
                StableId = "journey-" + code.ToLowerInvariant() + "-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = code, PreviewStableId = previewStableId,
                SnapshotStableId = snapshot.StableId, ExpectedDataRevision = snapshot.DataRevision,
                ExpectedCargoRevision = snapshot.CargoRevision,
                SimulationTick = snapshot.DataRevision + 1, RouteTicks = routeTicks,
            };

        private static PotatoCargoJourneySimulationSnapshot Clone(PotatoCargoJourneySimulationSnapshot source)
            => new PotatoCargoJourneySimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, SimulationDate = source.SimulationDate,
                GeneratedAt = source.GeneratedAt, StateCode = source.StateCode,
                CompletedRouteTicks = source.CompletedRouteTicks, CargoRevision = source.CargoRevision,
                HarvestLotStableId = source.HarvestLotStableId, PackageLotStableId = source.PackageLotStableId,
                SourceStableIds = source.SourceStableIds.ToArray(),
                Rule = new PotatoCargoJourneySimulationRuleSnapshot
                {
                    StableId = source.Rule.StableId, Revision = source.Rule.Revision,
                    RouteStableId = source.Rule.RouteStableId, OriginStableId = source.Rule.OriginStableId,
                    DestinationStableId = source.Rule.DestinationStableId,
                    RequiredRouteTicks = source.Rule.RequiredRouteTicks, SourceTypeCode = source.Rule.SourceTypeCode,
                    SourceStableIds = source.Rule.SourceStableIds.ToArray(), Limitations = source.Rule.Limitations.ToArray(),
                },
                Cargo = new 화물LotSimulationData
                {
                    StableId = source.Cargo.StableId, Revision = source.Cargo.Revision,
                    CanonicalProductStableId = source.Cargo.CanonicalProductStableId,
                    HarvestLotStableId = source.Cargo.HarvestLotStableId,
                    PackageLotStableId = source.Cargo.PackageLotStableId,
                    OriginStableId = source.Cargo.OriginStableId, DestinationStableId = source.Cargo.DestinationStableId,
                    StateCode = source.Cargo.StateCode, PackageCount = source.Cargo.PackageCount,
                    Quantity = source.Cargo.Quantity, UnitCode = source.Cargo.UnitCode,
                    VehicleCapacityKg = source.Cargo.VehicleCapacityKg,
                    SourceStableIds = source.Cargo.SourceStableIds.ToArray(),
                },
            };
    }

    public sealed class PotatoCargoJourneyPresentationModel
    {
        public string StateCode { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string CargoText { get; set; } = string.Empty;
        public string ProgressText { get; set; } = string.Empty;
        public string LineageText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public float NormalizedProgress { get; set; }
        public bool CanPreviewDispatch { get; set; }
        public bool CanAdvanceRoute { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoCargoJourneyProjector
    {
        private readonly PotatoCargoJourneySimulationValidator validator;
        public PotatoCargoJourneyProjector(PotatoCargoJourneySimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));
        public PotatoCargoJourneyPresentationModel Project(PotatoCargoJourneySimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            return new PotatoCargoJourneyPresentationModel
            {
                StateCode = snapshot.StateCode,
                DateText = snapshot.SimulationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CargoText = snapshot.Cargo.StableId + " · rev " + snapshot.CargoRevision + " · "
                    + snapshot.Cargo.PackageCount + " Box · " + snapshot.Cargo.Quantity + snapshot.Cargo.UnitCode,
                ProgressText = snapshot.CompletedRouteTicks + " / " + snapshot.Rule.RequiredRouteTicks + " route ticks",
                NormalizedProgress = (float)snapshot.CompletedRouteTicks / snapshot.Rule.RequiredRouteTicks,
                LineageText = snapshot.HarvestLotStableId + " → " + snapshot.PackageLotStableId + " → " + snapshot.Cargo.StableId,
                LimitationText = string.Join(" · ", snapshot.Rule.Limitations),
                CanPreviewDispatch = snapshot.StateCode == PotatoCargoJourneyStateCodes.Loaded,
                CanAdvanceRoute = snapshot.StateCode == PotatoCargoJourneyStateCodes.InTransit,
            };
        }
    }

    public static class PotatoCargoJourneySimulationFixture
    {
        public static PotatoCargoJourneySimulationSnapshot Create(감자수확CargoSimulationSnapshot loaded)
        {
            new 감자수확CargoSimulationValidator().Validate(loaded);
            if (loaded.Cargo == null || loaded.PackageLot == null)
                throw new InvalidOperationException("PotatoCargoJourneyLoadedCargoRequired");
            var cargo = loaded.Cargo;
            return new PotatoCargoJourneySimulationSnapshot
            {
                StableId = "cargo-journey:sim.potato.farm-hub", DataRevision = 1,
                ModeCode = "Simulation", ScenarioStableId = loaded.ScenarioStableId,
                SimulationDate = loaded.GeneratedAt, GeneratedAt = loaded.GeneratedAt,
                StateCode = PotatoCargoJourneyStateCodes.Loaded, CargoRevision = cargo.Revision,
                HarvestLotStableId = loaded.HarvestLot.StableId,
                PackageLotStableId = loaded.PackageLot.StableId,
                SourceStableIds = new[] { loaded.HarvestLot.StableId, loaded.PackageLot.StableId, cargo.StableId,
                    "source:fixture.potato-farm-hub-route" },
                Cargo = new 화물LotSimulationData
                {
                    StableId = cargo.StableId, Revision = cargo.Revision,
                    CanonicalProductStableId = cargo.CanonicalProductStableId,
                    HarvestLotStableId = cargo.HarvestLotStableId, PackageLotStableId = cargo.PackageLotStableId,
                    OriginStableId = cargo.OriginStableId, DestinationStableId = cargo.DestinationStableId,
                    StateCode = PotatoCargoJourneyStateCodes.Loaded, PackageCount = cargo.PackageCount,
                    Quantity = cargo.Quantity, UnitCode = cargo.UnitCode,
                    VehicleCapacityKg = cargo.VehicleCapacityKg, SourceStableIds = cargo.SourceStableIds.ToArray(),
                },
                Rule = new PotatoCargoJourneySimulationRuleSnapshot
                {
                    StableId = "journey-rule:sim.potato.farm-hub", Revision = 1,
                    RouteStableId = "route:sim.farm-hub", OriginStableId = cargo.OriginStableId,
                    DestinationStableId = cargo.DestinationStableId, RequiredRouteTicks = 3,
                    SourceTypeCode = "Fixture", SourceStableIds = new[] { "source:fixture.potato-farm-hub-route" },
                    Limitations = new[] { "3 route ticks와 날짜 진행은 Simulation 규칙이며 실제 운송 시간이나 인수를 뜻하지 않습니다." },
                },
            };
        }
    }
}
