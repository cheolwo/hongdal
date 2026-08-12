using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class FarmSoilProfileCodes
    {
        public const string Loam = "Loam";
        public const string DryLoam = "DryLoam";
        public const string WetLoam = "WetLoam";

        internal static bool IsKnown(string value)
            => value == Loam || value == DryLoam || value == WetLoam;
    }

    public static class FarmSoilTileCultivationStateCodes
    {
        public const string Untilled = "Untilled";
        public const string Tilled = "Tilled";
        public const string Sown = "Sown";
        public const string Harvested = "Harvested";

        internal static bool IsKnown(string value)
            => value == Untilled || value == Tilled || value == Sown || value == Harvested;
    }

    public static class FarmSoilTileWorkStateCodes
    {
        public const string None = "None";
        public const string Planned = "Planned";
        public const string InProgress = "InProgress";

        internal static bool IsKnown(string value)
            => value == None || value == Planned || value == InProgress;
    }

    public static class FarmSoilTileColorTokens
    {
        public const string Untilled = "Soil.Untilled";
        public const string Tilled = "Soil.Tilled";
        public const string Sown = "Soil.Sown";
        public const string Harvested = "Soil.Harvested";
        public const string Selected = "Soil.Selected";
    }

    public sealed class FarmSoilTileSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public int GridX { get; set; }
        public int GridZ { get; set; }
        public string SoilProfileCode { get; set; } = string.Empty;
        public string MoistureConditionCode { get; set; } = string.Empty;
        public string CultivationStateCode { get; set; } = string.Empty;
        public string WorkStateCode { get; set; } = string.Empty;
        public string? ActiveCultivationStableId { get; set; }
        public string? ActiveWorkStableId { get; set; }
    }

    public sealed class FarmSoilTileSimulationDataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public int ScenarioSeed { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string PlotStableId { get; set; } = string.Empty;
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public FarmSoilTileSimulationData[] Tiles { get; set; } =
            Array.Empty<FarmSoilTileSimulationData>();
    }

    public sealed class FarmSoilTileTillingPreview
    {
        public string StableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class FarmSoilTileTillingCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
        public string ScenarioStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simulation의 밭갈이 상태 전이를 소유합니다. Preview와 Confirm은 snapshot을 바꾸지 않고,
    /// 명시적으로 확인된 command만 Tick에서 새 snapshot으로 반영합니다.
    /// </summary>
    public sealed class FarmSoilTileTillingSimulationEngine
    {
        private readonly FarmSoilTileSimulationValidator validator;

        public FarmSoilTileTillingSimulationEngine(FarmSoilTileSimulationValidator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public FarmSoilTileTillingPreview Preview(
            FarmSoilTileSimulationDataSnapshot snapshot,
            string tileStableId)
        {
            var tile = EligibleTile(snapshot, tileStableId);
            return new FarmSoilTileTillingPreview
            {
                StableId = TillingId("farm-tilling-preview", snapshot, tile),
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                ScenarioStableId = snapshot.ScenarioStableId,
                RuleRevision = snapshot.RuleRevision,
                TileStableId = tile.StableId,
                RequiresExplicitConfirmation = true,
            };
        }

        public FarmSoilTileTillingCommand Confirm(
            FarmSoilTileSimulationDataSnapshot snapshot,
            FarmSoilTileTillingPreview preview)
        {
            if (preview == null
                || !preview.RequiresExplicitConfirmation
                || preview.SnapshotStableId != snapshot?.StableId
                || preview.ExpectedDataRevision != snapshot.DataRevision
                || preview.ScenarioStableId != snapshot.ScenarioStableId
                || preview.RuleRevision != snapshot.RuleRevision)
            {
                throw new InvalidOperationException("FarmSoilTileTillingPreviewStale");
            }

            var tile = EligibleTile(snapshot, preview.TileStableId);
            var expectedPreviewId = TillingId("farm-tilling-preview", snapshot, tile);
            if (preview.StableId != expectedPreviewId)
                throw new InvalidOperationException("FarmSoilTileTillingPreviewInvalid");

            return new FarmSoilTileTillingCommand
            {
                StableId = TillingId("farm-tilling-command", snapshot, tile),
                PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
                ScenarioStableId = snapshot.ScenarioStableId,
                RuleRevision = snapshot.RuleRevision,
                TileStableId = tile.StableId,
            };
        }

        public FarmSoilTileSimulationDataSnapshot Tick(
            FarmSoilTileSimulationDataSnapshot snapshot,
            FarmSoilTileTillingCommand command)
        {
            if (command == null
                || command.SnapshotStableId != snapshot?.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1
                || command.ScenarioStableId != snapshot.ScenarioStableId
                || command.RuleRevision != snapshot.RuleRevision)
            {
                throw new InvalidOperationException("FarmSoilTileTillingCommandStale");
            }

            var tile = EligibleTile(snapshot, command.TileStableId);
            if (command.StableId != TillingId("farm-tilling-command", snapshot, tile)
                || command.PreviewStableId != TillingId("farm-tilling-preview", snapshot, tile))
            {
                throw new InvalidOperationException("FarmSoilTileTillingCommandInvalid");
            }

            var next = Clone(snapshot);
            next.DataRevision = command.SimulationTick;
            next.GeneratedAt = snapshot.GeneratedAt.AddSeconds(1);
            next.SourceStableIds = snapshot.SourceStableIds
                .Concat(new[] { command.StableId })
                .ToArray();
            var target = next.Tiles.Single(value => value.StableId == command.TileStableId);
            target.Revision++;
            target.CultivationStateCode = FarmSoilTileCultivationStateCodes.Tilled;
            target.WorkStateCode = FarmSoilTileWorkStateCodes.None;
            target.ActiveWorkStableId = null;
            validator.Validate(next);
            return next;
        }

        private FarmSoilTileSimulationData EligibleTile(
            FarmSoilTileSimulationDataSnapshot snapshot,
            string tileStableId)
        {
            validator.Validate(snapshot);
            var tile = snapshot.Tiles.SingleOrDefault(value => value.StableId == tileStableId)
                ?? throw new InvalidOperationException("FarmSoilTileTillingTargetMissing:" + tileStableId);
            if (tile.CultivationStateCode != FarmSoilTileCultivationStateCodes.Untilled
                || tile.WorkStateCode != FarmSoilTileWorkStateCodes.None)
            {
                throw new InvalidOperationException("FarmSoilTileTillingNotAllowed:" + tileStableId);
            }

            return tile;
        }

        private static string TillingId(
            string prefix,
            FarmSoilTileSimulationDataSnapshot snapshot,
            FarmSoilTileSimulationData tile)
            => prefix + ":sim.potato."
                + tile.GridX + "." + tile.GridZ + ".r" + snapshot.DataRevision;

        private static FarmSoilTileSimulationDataSnapshot Clone(
            FarmSoilTileSimulationDataSnapshot snapshot)
            => new FarmSoilTileSimulationDataSnapshot
            {
                StableId = snapshot.StableId,
                DataRevision = snapshot.DataRevision,
                ModeCode = snapshot.ModeCode,
                ScenarioStableId = snapshot.ScenarioStableId,
                ScenarioSeed = snapshot.ScenarioSeed,
                RuleRevision = snapshot.RuleRevision,
                PlotStableId = snapshot.PlotStableId,
                GridWidth = snapshot.GridWidth,
                GridHeight = snapshot.GridHeight,
                GeneratedAt = snapshot.GeneratedAt,
                SourceStableIds = snapshot.SourceStableIds.ToArray(),
                Tiles = snapshot.Tiles.Select(value => new FarmSoilTileSimulationData
                {
                    StableId = value.StableId,
                    Revision = value.Revision,
                    GridX = value.GridX,
                    GridZ = value.GridZ,
                    SoilProfileCode = value.SoilProfileCode,
                    MoistureConditionCode = value.MoistureConditionCode,
                    CultivationStateCode = value.CultivationStateCode,
                    WorkStateCode = value.WorkStateCode,
                    ActiveCultivationStableId = value.ActiveCultivationStableId,
                    ActiveWorkStableId = value.ActiveWorkStableId,
                }).ToArray(),
            };
    }

    public sealed class FarmSoilTileSimulationValidator
    {
        public void Validate(FarmSoilTileSimulationDataSnapshot snapshot)
        {
            if (snapshot == null
                || !StableDataId.IsValid(snapshot.StableId)
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || !StableDataId.IsValid(snapshot.PlotStableId)
                || snapshot.DataRevision < 0
                || snapshot.ModeCode != "Simulation"
                || string.IsNullOrWhiteSpace(snapshot.RuleRevision)
                || snapshot.GridWidth <= 0 || snapshot.GridHeight <= 0
                || snapshot.GeneratedAt == default
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || snapshot.SourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != snapshot.SourceStableIds.Length
                || snapshot.Tiles == null)
            {
                throw new InvalidOperationException("FarmSoilTileSnapshotInvalid");
            }

            var stableIds = new HashSet<string>(StringComparer.Ordinal);
            var coordinates = new HashSet<string>(StringComparer.Ordinal);
            foreach (var tile in snapshot.Tiles)
            {
                if (tile == null || !StableDataId.IsValid(tile.StableId)
                    || tile.Revision < 0
                    || tile.GridX < 0 || tile.GridX >= snapshot.GridWidth
                    || tile.GridZ < 0 || tile.GridZ >= snapshot.GridHeight
                    || !FarmSoilProfileCodes.IsKnown(tile.SoilProfileCode)
                    || !tile.MoistureConditionCode.IsKnownForSoilTile()
                    || !FarmSoilTileCultivationStateCodes.IsKnown(tile.CultivationStateCode)
                    || !FarmSoilTileWorkStateCodes.IsKnown(tile.WorkStateCode))
                {
                    throw new InvalidOperationException("FarmSoilTileInvalid");
                }

                if (!stableIds.Add(tile.StableId))
                    throw new InvalidOperationException("FarmSoilTileStableIdDuplicate");
                if (!coordinates.Add(tile.GridX + ":" + tile.GridZ))
                    throw new InvalidOperationException("FarmSoilTileCoordinateDuplicate");
                ValidateReferences(tile);
            }

            if (snapshot.Tiles.Length != snapshot.GridWidth * snapshot.GridHeight
                || coordinates.Count != snapshot.GridWidth * snapshot.GridHeight)
            {
                throw new InvalidOperationException("FarmSoilTileGridIncomplete");
            }
        }

        private static void ValidateReferences(FarmSoilTileSimulationData tile)
        {
            var hasCultivation = !string.IsNullOrWhiteSpace(tile.ActiveCultivationStableId);
            var hasWork = !string.IsNullOrWhiteSpace(tile.ActiveWorkStableId);
            if ((hasCultivation && !StableDataId.IsValid(tile.ActiveCultivationStableId))
                || (hasWork && !StableDataId.IsValid(tile.ActiveWorkStableId))
                || (tile.CultivationStateCode == FarmSoilTileCultivationStateCodes.Sown) != hasCultivation
                || (tile.WorkStateCode == FarmSoilTileWorkStateCodes.None) == hasWork)
            {
                throw new InvalidOperationException("FarmSoilTileStateReferenceMismatch");
            }
        }
    }

    internal static class FarmSensorConditionCodeExtensions
    {
        public static bool IsKnownForSoilTile(this string value)
            => value == FarmSensorConditionCodes.Normal
                || value == FarmSensorConditionCodes.Dry
                || value == FarmSensorConditionCodes.Waterlogged
                || value == FarmSensorConditionCodes.Unknown;
    }

    public sealed class FarmSoilTilePresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridZ { get; set; }
        public string SoilProfileCode { get; set; } = string.Empty;
        public string MoistureConditionCode { get; set; } = string.Empty;
        public string CultivationStateCode { get; set; } = string.Empty;
        public string WorkStateCode { get; set; } = string.Empty;
        public string ColorToken { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public sealed class FarmSoilTileMapPresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public long SourceRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public string? SelectedTileStableId { get; set; }
        public string SelectedTileTitleText { get; set; } = string.Empty;
        public string SelectedTileDetailText { get; set; } = string.Empty;
        public bool CanPreviewTilling { get; set; }
        public bool RequiresExplicitTillingConfirmation { get; set; }
        public bool HasConfirmedTillingCommand { get; set; }
        public FarmSoilTilePresentationModel[] Tiles { get; set; } =
            Array.Empty<FarmSoilTilePresentationModel>();
    }

    public sealed class FarmSoilTileMapProjector
    {
        private readonly FarmSoilTileSimulationValidator validator;

        public FarmSoilTileMapProjector(FarmSoilTileSimulationValidator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public FarmSoilTileMapPresentationModel Project(
            FarmSoilTileSimulationDataSnapshot snapshot,
            string? selectedTileStableId = null,
            FarmSoilTileTillingPreview? tillingPreview = null,
            FarmSoilTileTillingCommand? confirmedCommand = null)
        {
            validator.Validate(snapshot);
            if (selectedTileStableId != null
                && !snapshot.Tiles.Any(value => value.StableId == selectedTileStableId))
            {
                throw new InvalidOperationException("FarmSoilTileSelectionMissing:" + selectedTileStableId);
            }
            if (tillingPreview != null
                && (selectedTileStableId == null
                    || tillingPreview.TileStableId != selectedTileStableId
                    || tillingPreview.SnapshotStableId != snapshot.StableId
                    || tillingPreview.ExpectedDataRevision != snapshot.DataRevision))
            {
                throw new InvalidOperationException("FarmSoilTileTillingPreviewPresentationMismatch");
            }
            if (confirmedCommand != null
                && (tillingPreview == null
                    || confirmedCommand.PreviewStableId != tillingPreview.StableId
                    || confirmedCommand.TileStableId != selectedTileStableId
                    || confirmedCommand.ExpectedDataRevision != snapshot.DataRevision))
            {
                throw new InvalidOperationException("FarmSoilTileTillingCommandPresentationMismatch");
            }

            var selected = selectedTileStableId == null
                ? null
                : snapshot.Tiles.Single(value => value.StableId == selectedTileStableId);
            return new FarmSoilTileMapPresentationModel
            {
                StableId = "farm-soil-tile-map:" + snapshot.PlotStableId,
                SourceRevision = snapshot.DataRevision,
                ModeCode = snapshot.ModeCode,
                RuleRevision = snapshot.RuleRevision,
                GridWidth = snapshot.GridWidth,
                GridHeight = snapshot.GridHeight,
                SelectedTileStableId = selectedTileStableId,
                SelectedTileTitleText = selected == null
                    ? "토양 타일을 선택하세요"
                    : "타일 " + selected.GridX + "," + selected.GridZ,
                SelectedTileDetailText = selected == null
                    ? "토양·경작 상태를 확인한 뒤 작업을 검토합니다."
                    : Detail(selected, tillingPreview, confirmedCommand),
                CanPreviewTilling = selected?.CultivationStateCode
                    == FarmSoilTileCultivationStateCodes.Untilled
                    && selected.WorkStateCode == FarmSoilTileWorkStateCodes.None
                    && tillingPreview == null,
                RequiresExplicitTillingConfirmation = tillingPreview != null
                    && confirmedCommand == null,
                HasConfirmedTillingCommand = confirmedCommand != null,
                Tiles = snapshot.Tiles
                    .OrderBy(value => value.GridZ)
                    .ThenBy(value => value.GridX)
                    .Select(value => Present(value, value.StableId == selectedTileStableId))
                    .ToArray(),
            };
        }

        private static FarmSoilTilePresentationModel Present(
            FarmSoilTileSimulationData tile,
            bool selected)
            => new FarmSoilTilePresentationModel
            {
                StableId = tile.StableId,
                GridX = tile.GridX,
                GridZ = tile.GridZ,
                SoilProfileCode = tile.SoilProfileCode,
                MoistureConditionCode = tile.MoistureConditionCode,
                CultivationStateCode = tile.CultivationStateCode,
                WorkStateCode = tile.WorkStateCode,
                ColorToken = selected
                    ? FarmSoilTileColorTokens.Selected
                    : ColorToken(tile.CultivationStateCode),
                IsSelected = selected,
            };

        private static string ColorToken(string state)
            => state switch
            {
                FarmSoilTileCultivationStateCodes.Untilled => FarmSoilTileColorTokens.Untilled,
                FarmSoilTileCultivationStateCodes.Tilled => FarmSoilTileColorTokens.Tilled,
                FarmSoilTileCultivationStateCodes.Sown => FarmSoilTileColorTokens.Sown,
                FarmSoilTileCultivationStateCodes.Harvested => FarmSoilTileColorTokens.Harvested,
                _ => throw new InvalidOperationException("FarmSoilTileCultivationStateInvalid:" + state),
            };

        private static string Detail(
            FarmSoilTileSimulationData tile,
            FarmSoilTileTillingPreview? preview,
            FarmSoilTileTillingCommand? command)
            => "토양 " + tile.SoilProfileCode
                + "\n수분 " + tile.MoistureConditionCode
                + "\n경작 " + tile.CultivationStateCode
                + "\n작업 " + tile.WorkStateCode
                + "\n" + (command != null
                    ? "Confirm 완료 · Simulation Tick 대기"
                    : preview != null
                        ? "밭갈이 Preview · 명시적 Confirm 필요"
                        : tile.CultivationStateCode == FarmSoilTileCultivationStateCodes.Untilled
                            ? "밭갈이 Preview 가능"
                            : tile.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled
                                ? "밭갈이 Tick 반영 완료"
                                : "현재 단계 확인 필요");
    }

    public static class FarmPotatoSoilTileSimulationFixture
    {
        public static FarmSoilTileSimulationDataSnapshot Create()
        {
            const int size = 6;
            var tiles = new List<FarmSoilTileSimulationData>(size * size);
            for (var z = 0; z < size; z++)
            for (var x = 0; x < size; x++)
            {
                var sown = x == 2 && z == 2;
                var tilled = !sown && z == 2 && x >= 1 && x <= 4;
                tiles.Add(new FarmSoilTileSimulationData
                {
                    StableId = $"farm-soil-tile:sim.potato.{x}.{z}",
                    Revision = 1,
                    GridX = x,
                    GridZ = z,
                    SoilProfileCode = x == 0
                        ? FarmSoilProfileCodes.DryLoam
                        : x == 5
                            ? FarmSoilProfileCodes.WetLoam
                            : FarmSoilProfileCodes.Loam,
                    MoistureConditionCode = x == 0
                        ? FarmSensorConditionCodes.Dry
                        : x == 5
                            ? FarmSensorConditionCodes.Waterlogged
                            : FarmSensorConditionCodes.Normal,
                    CultivationStateCode = sown
                        ? FarmSoilTileCultivationStateCodes.Sown
                        : tilled
                            ? FarmSoilTileCultivationStateCodes.Tilled
                            : FarmSoilTileCultivationStateCodes.Untilled,
                    WorkStateCode = FarmSoilTileWorkStateCodes.None,
                    ActiveCultivationStableId = sown
                        ? "cultivation:sim.potato.2026.tile-2-2"
                        : null,
                });
            }

            return new FarmSoilTileSimulationDataSnapshot
            {
                StableId = "farm-soil-tile-snapshot:sim.potato.1",
                DataRevision = 1,
                ModeCode = "Simulation",
                ScenarioStableId = "scenario:farm.potato.first-playable",
                ScenarioSeed = 260809,
                RuleRevision = "farm-soil-tile-rule:1",
                PlotStableId = "farm-plot:sim.potato.1",
                GridWidth = size,
                GridHeight = size,
                GeneratedAt = new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
                SourceStableIds = new[]
                {
                    "simulation-assumption:farm.potato.soil-layout.1",
                },
                Tiles = tiles.ToArray(),
            };
        }
    }
}
