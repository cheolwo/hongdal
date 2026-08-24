using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class 수확CargoCommandCodes
    {
        public const string Pack = "Pack";
        public const string Load = "Load";

        internal static bool IsKnown(string value) => value == Pack || value == Load;
    }

    public static class 수확CargoStateCodes
    {
        public const string Harvested = "Harvested";
        public const string Packed = "Packed";
        public const string Loaded = "Loaded";
    }

    public sealed class 감자포장SimulationRuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string PackageTypeCode { get; set; } = string.Empty;
        public decimal NetQuantityPerPackageKg { get; set; }
        public decimal VehicleCapacityKg { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class 포장LotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public string PackageTypeCode { get; set; } = string.Empty;
        public int PackageCount { get; set; }
        public decimal NetQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 화물LotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string OriginStableId { get; set; } = string.Empty;
        public string DestinationStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int PackageCount { get; set; }
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal VehicleCapacityKg { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 감자수확CargoSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public 수확LotSimulationData HarvestLot { get; set; } = new 수확LotSimulationData();
        public 감자포장SimulationRuleSnapshot PackagingRule { get; set; } = new 감자포장SimulationRuleSnapshot();
        public 포장LotSimulationData? PackageLot { get; set; }
        public 화물LotSimulationData? Cargo { get; set; }
    }

    public sealed class 감자수확CargoPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class 감자수확CargoCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 감자수확CargoSimulationValidator
    {
        public void Validate(감자수확CargoSimulationSnapshot snapshot)
        {
            if (snapshot == null || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision <= 0 || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId) || snapshot.GeneratedAt == default
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || snapshot.SourceStableIds.Distinct(StringComparer.Ordinal).Count() != snapshot.SourceStableIds.Length)
                throw new InvalidOperationException("PotatoHarvestCargoSnapshotInvalid");

            ValidateHarvest(snapshot.HarvestLot);
            ValidateRule(snapshot.PackagingRule, snapshot.HarvestLot);
            ValidatePackage(snapshot.PackageLot, snapshot.HarvestLot, snapshot.PackagingRule);
            ValidateCargo(snapshot.Cargo, snapshot.PackageLot, snapshot.HarvestLot, snapshot.PackagingRule);
        }

        private static void ValidateHarvest(수확LotSimulationData lot)
        {
            if (lot == null || !StableDataId.IsValid(lot.StableId) || lot.Revision <= 0
                || lot.CanonicalProductStableId != "product:potato"
                || !StableDataId.IsValid(lot.CultivationStableId) || lot.HarvestedOn == default
                || lot.Quantity <= 0 || lot.UnitCode != "kg" || lot.SourceStableIds == null
                || lot.SourceStableIds.Length == 0 || lot.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("PotatoHarvestCargoHarvestLotInvalid");
        }

        private static void ValidateRule(감자포장SimulationRuleSnapshot rule, 수확LotSimulationData lot)
        {
            if (rule == null || !StableDataId.IsValid(rule.StableId) || rule.Revision <= 0
                || rule.CanonicalProductStableId != lot.CanonicalProductStableId
                || rule.PackageTypeCode != "Box" || rule.NetQuantityPerPackageKg <= 0
                || rule.VehicleCapacityKg <= 0 || rule.SourceTypeCode != "Fixture"
                || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0
                || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("PotatoHarvestCargoPackagingRuleInvalid");
        }

        private static void ValidatePackage(포장LotSimulationData? package, 수확LotSimulationData harvest,
            감자포장SimulationRuleSnapshot rule)
        {
            if (package == null) return;
            if (!StableDataId.IsValid(package.StableId) || package.Revision <= 0
                || package.CanonicalProductStableId != harvest.CanonicalProductStableId
                || package.HarvestLotStableId != harvest.StableId || package.HarvestLotRevision != harvest.Revision
                || package.PackageTypeCode != rule.PackageTypeCode || package.PackageCount <= 0
                || package.NetQuantity != harvest.Quantity || package.UnitCode != harvest.UnitCode
                || package.PackageCount * rule.NetQuantityPerPackageKg != package.NetQuantity
                || package.SourceStableIds == null || !package.SourceStableIds.Contains(harvest.StableId))
                throw new InvalidOperationException("PotatoHarvestCargoPackageConservationInvalid");
        }

        private static void ValidateCargo(화물LotSimulationData? cargo, 포장LotSimulationData? package,
            수확LotSimulationData harvest, 감자포장SimulationRuleSnapshot rule)
        {
            if (cargo == null) return;
            if (package == null) throw new InvalidOperationException("PotatoHarvestCargoPackageRequired");
            if (!StableDataId.IsValid(cargo.StableId) || cargo.Revision <= 0
                || cargo.CanonicalProductStableId != harvest.CanonicalProductStableId
                || cargo.HarvestLotStableId != harvest.StableId || cargo.PackageLotStableId != package.StableId
                || !StableDataId.IsValid(cargo.OriginStableId) || !StableDataId.IsValid(cargo.DestinationStableId)
                || cargo.StateCode != 수확CargoStateCodes.Loaded || cargo.PackageCount != package.PackageCount
                || cargo.Quantity != package.NetQuantity || cargo.UnitCode != package.UnitCode
                || cargo.VehicleCapacityKg != rule.VehicleCapacityKg || cargo.Quantity > cargo.VehicleCapacityKg
                || cargo.SourceStableIds == null || !cargo.SourceStableIds.Contains(harvest.StableId)
                || !cargo.SourceStableIds.Contains(package.StableId))
                throw new InvalidOperationException("PotatoHarvestCargoCargoConservationInvalid");
        }
    }

    public sealed class 감자수확CargoSimulationEngine
    {
        private readonly 감자수확CargoSimulationValidator validator;

        public 감자수확CargoSimulationEngine(감자수확CargoSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public 감자수확CargoPreview PreviewPacking(감자수확CargoSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.PackageLot != null || snapshot.Cargo != null)
                throw new InvalidOperationException("PotatoHarvestCargoAlreadyPacked");
            if (snapshot.HarvestLot.Quantity % snapshot.PackagingRule.NetQuantityPerPackageKg != 0)
                throw new InvalidOperationException("PotatoHarvestCargoPackageRemainderUnsupported");
            return Preview(snapshot, 수확CargoCommandCodes.Pack);
        }

        public 감자수확CargoPreview PreviewLoading(감자수확CargoSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            if (snapshot.PackageLot == null) throw new InvalidOperationException("PotatoHarvestCargoPackageRequired");
            if (snapshot.Cargo != null) throw new InvalidOperationException("PotatoHarvestCargoAlreadyLoaded");
            if (snapshot.PackageLot.NetQuantity > snapshot.PackagingRule.VehicleCapacityKg)
                throw new InvalidOperationException("PotatoHarvestCargoVehicleCapacityExceeded");
            return Preview(snapshot, 수확CargoCommandCodes.Load);
        }

        public 감자수확CargoCommand Confirm(감자수확CargoSimulationSnapshot snapshot, 감자수확CargoPreview preview)
        {
            validator.Validate(snapshot);
            if (preview == null || !수확CargoCommandCodes.IsKnown(preview.CommandCode)
                || preview.StableId != Preview(snapshot, preview.CommandCode).StableId
                || preview.SnapshotStableId != snapshot.StableId
                || preview.ExpectedDataRevision != snapshot.DataRevision
                || preview.HarvestLotStableId != snapshot.HarvestLot.StableId
                || preview.HarvestLotRevision != snapshot.HarvestLot.Revision
                || !preview.RequiresExplicitConfirmation)
                throw new InvalidOperationException("PotatoHarvestCargoPreviewStaleOrInvalid");
            return new 감자수확CargoCommand
            {
                StableId = "cargo-" + preview.CommandCode.ToLowerInvariant() + "-command:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = preview.CommandCode,
                PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
            };
        }

        public 감자수확CargoSimulationSnapshot Tick(감자수확CargoSimulationSnapshot snapshot,
            감자수확CargoCommand command)
        {
            validator.Validate(snapshot);
            if (command == null || !수확CargoCommandCodes.IsKnown(command.CommandCode)
                || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1
                || command.PreviewStableId != Preview(snapshot, command.CommandCode).StableId)
                throw new InvalidOperationException("PotatoHarvestCargoCommandInvalid");

            var next = Clone(snapshot);
            next.DataRevision++;
            next.SourceStableIds = next.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            if (command.CommandCode == 수확CargoCommandCodes.Pack) ApplyPacking(next, command);
            else ApplyLoading(next, command);
            validator.Validate(next);
            return next;
        }

        private static void ApplyPacking(감자수확CargoSimulationSnapshot snapshot, 감자수확CargoCommand command)
        {
            if (snapshot.PackageLot != null || snapshot.Cargo != null)
                throw new InvalidOperationException("PotatoHarvestCargoPackStateInvalid");
            snapshot.PackageLot = new 포장LotSimulationData
            {
                StableId = "package-lot:sim.potato."
                    + snapshot.HarvestLot.HarvestedOn.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                    + ".r" + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                Revision = 1,
                CanonicalProductStableId = snapshot.HarvestLot.CanonicalProductStableId,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                HarvestLotRevision = snapshot.HarvestLot.Revision,
                PackageTypeCode = snapshot.PackagingRule.PackageTypeCode,
                PackageCount = decimal.ToInt32(snapshot.HarvestLot.Quantity
                    / snapshot.PackagingRule.NetQuantityPerPackageKg),
                NetQuantity = snapshot.HarvestLot.Quantity,
                UnitCode = snapshot.HarvestLot.UnitCode,
                SourceStableIds = new[] { snapshot.HarvestLot.StableId, command.StableId },
            };
        }

        private static void ApplyLoading(감자수확CargoSimulationSnapshot snapshot, 감자수확CargoCommand command)
        {
            var package = snapshot.PackageLot
                ?? throw new InvalidOperationException("PotatoHarvestCargoPackageRequired");
            snapshot.Cargo = new 화물LotSimulationData
            {
                StableId = "cargo:sim.potato."
                    + snapshot.HarvestLot.HarvestedOn.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                    + ".r" + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                Revision = 1,
                CanonicalProductStableId = snapshot.HarvestLot.CanonicalProductStableId,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                PackageLotStableId = package.StableId,
                OriginStableId = "farm-yard:sim.potato",
                DestinationStableId = "hub:sim.inbound",
                StateCode = 수확CargoStateCodes.Loaded,
                PackageCount = package.PackageCount,
                Quantity = package.NetQuantity,
                UnitCode = package.UnitCode,
                VehicleCapacityKg = snapshot.PackagingRule.VehicleCapacityKg,
                SourceStableIds = new[] { snapshot.HarvestLot.StableId, package.StableId, command.StableId },
            };
        }

        private static 감자수확CargoPreview Preview(감자수확CargoSimulationSnapshot snapshot, string code)
            => new 감자수확CargoPreview
            {
                StableId = "cargo-" + code.ToLowerInvariant() + "-preview:sim.potato.r"
                    + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                CommandCode = code,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                HarvestLotRevision = snapshot.HarvestLot.Revision,
                RequiresExplicitConfirmation = true,
            };

        private static 감자수확CargoSimulationSnapshot Clone(감자수확CargoSimulationSnapshot source)
            => new 감자수확CargoSimulationSnapshot
            {
                StableId = source.StableId, DataRevision = source.DataRevision, ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId, GeneratedAt = source.GeneratedAt,
                SourceStableIds = source.SourceStableIds.ToArray(), HarvestLot = CloneHarvest(source.HarvestLot),
                PackagingRule = new 감자포장SimulationRuleSnapshot
                {
                    StableId = source.PackagingRule.StableId, Revision = source.PackagingRule.Revision,
                    CanonicalProductStableId = source.PackagingRule.CanonicalProductStableId,
                    PackageTypeCode = source.PackagingRule.PackageTypeCode,
                    NetQuantityPerPackageKg = source.PackagingRule.NetQuantityPerPackageKg,
                    VehicleCapacityKg = source.PackagingRule.VehicleCapacityKg,
                    SourceTypeCode = source.PackagingRule.SourceTypeCode,
                    SourceStableIds = source.PackagingRule.SourceStableIds.ToArray(),
                    Limitations = source.PackagingRule.Limitations.ToArray(),
                },
                PackageLot = source.PackageLot == null ? null : new 포장LotSimulationData
                {
                    StableId = source.PackageLot.StableId, Revision = source.PackageLot.Revision,
                    CanonicalProductStableId = source.PackageLot.CanonicalProductStableId,
                    HarvestLotStableId = source.PackageLot.HarvestLotStableId,
                    HarvestLotRevision = source.PackageLot.HarvestLotRevision,
                    PackageTypeCode = source.PackageLot.PackageTypeCode, PackageCount = source.PackageLot.PackageCount,
                    NetQuantity = source.PackageLot.NetQuantity, UnitCode = source.PackageLot.UnitCode,
                    SourceStableIds = source.PackageLot.SourceStableIds.ToArray(),
                },
                Cargo = source.Cargo == null ? null : new 화물LotSimulationData
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

        private static 수확LotSimulationData CloneHarvest(수확LotSimulationData source)
            => new 수확LotSimulationData
            {
                StableId = source.StableId, Revision = source.Revision,
                CanonicalProductStableId = source.CanonicalProductStableId,
                CultivationStableId = source.CultivationStableId, HarvestedOn = source.HarvestedOn,
                Quantity = source.Quantity, UnitCode = source.UnitCode,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }

    public sealed class 감자수확CargoPresentationModel
    {
        public string StateCode { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public string HarvestLotText { get; set; } = string.Empty;
        public string PackageLotText { get; set; } = string.Empty;
        public string CargoText { get; set; } = string.Empty;
        public string LineageText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public bool CanPreviewPacking { get; set; }
        public bool CanPreviewLoading { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 감자수확CargoProjector
    {
        private readonly 감자수확CargoSimulationValidator validator;
        public 감자수확CargoProjector(감자수확CargoSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public 감자수확CargoPresentationModel Project(감자수확CargoSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            var state = snapshot.Cargo != null ? 수확CargoStateCodes.Loaded
                : snapshot.PackageLot != null ? 수확CargoStateCodes.Packed : 수확CargoStateCodes.Harvested;
            return new 감자수확CargoPresentationModel
            {
                StateCode = state,
                SourceModeCode = "Simulation/Fixture",
                HarvestLotText = snapshot.HarvestLot.StableId + " · " + snapshot.HarvestLot.Quantity + "kg",
                PackageLotText = snapshot.PackageLot == null ? "NOT PACKED"
                    : snapshot.PackageLot.StableId + " · " + snapshot.PackageLot.PackageCount + " Box · "
                        + snapshot.PackageLot.NetQuantity + "kg",
                CargoText = snapshot.Cargo == null ? "NOT LOADED"
                    : snapshot.Cargo.StableId + " · " + snapshot.Cargo.Quantity + "kg / "
                        + snapshot.Cargo.VehicleCapacityKg + "kg",
                LineageText = snapshot.Cargo == null ? snapshot.HarvestLot.StableId
                    : snapshot.HarvestLot.StableId + " → " + snapshot.PackageLot!.StableId + " → " + snapshot.Cargo.StableId,
                LimitationText = string.Join(" · ", snapshot.PackagingRule.Limitations),
                CanPreviewPacking = snapshot.PackageLot == null,
                CanPreviewLoading = snapshot.PackageLot != null && snapshot.Cargo == null,
            };
        }
    }

    public static class 감자수확CargoSimulationFixture
    {
        public static 감자수확CargoSimulationSnapshot Create(수확LotSimulationData harvestLot)
            => new 감자수확CargoSimulationSnapshot
            {
                StableId = "cargo-lifecycle:sim.potato", DataRevision = 1, ModeCode = "Simulation",
                ScenarioStableId = "scenario:sim.potato-farm-to-market",
                GeneratedAt = new DateTimeOffset(2026, 4, 7, 0, 0, 0, TimeSpan.Zero),
                SourceStableIds = new[] { harvestLot.StableId, "source:fixture.potato-packaging" },
                HarvestLot = new 수확LotSimulationData
                {
                    StableId = harvestLot.StableId, Revision = harvestLot.Revision,
                    CanonicalProductStableId = harvestLot.CanonicalProductStableId,
                    CultivationStableId = harvestLot.CultivationStableId, HarvestedOn = harvestLot.HarvestedOn,
                    Quantity = harvestLot.Quantity, UnitCode = harvestLot.UnitCode,
                    SourceStableIds = harvestLot.SourceStableIds.ToArray(),
                },
                PackagingRule = new 감자포장SimulationRuleSnapshot
                {
                    StableId = "packaging-rule:sim.potato.box20kg", Revision = 1,
                    CanonicalProductStableId = "product:potato", PackageTypeCode = "Box",
                    NetQuantityPerPackageKg = 20m, VehicleCapacityKg = 400m, SourceTypeCode = "Fixture",
                    SourceStableIds = new[] { "source:fixture.potato-packaging" },
                    Limitations = new[] { "20kg 상자와 400kg 차량은 Simulation 규칙이며 운영 포장·운송 기준이 아닙니다." },
                },
            };
    }
}
