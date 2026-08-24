using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.Survival
{
    public static class FarmSurvivalVisualKeys
    {
        public const string PlayerSurvivor = "character.survivor.player";
        public const string FarmWorker = "character.farm-worker";
        public const string TilledSoil = "agriculture.soil.tilled";
        public const string PreparedDefense = "survival.defense.prepared";
        public const string DamagedDefense = "survival.defense.damaged";
        public const string StylizedZombie = "threat.zombie.stylized";
        public const string StylizedRaider = "threat.raider.stylized";
        public const string DamageMarker = "survival.damage.recoverable";

        public const string GenericCharacterFallback = "character.generic.lowpoly";
        public const string SkeletonThreatFallback = "character.threat.skeleton";
        public const string FarmPropFallback = "prop.farm.generic";
    }

    public static class FarmSurvivalExperienceCodes
    {
        public const string ScenicSeasonRuleRevision =
            "farm-survival.scenic-season.r1";
        public const string AwaitingCombat = "AwaitingCombat";
        public const string Peaceful = "Peaceful";
        public const string SeasonalPreparation = "SeasonalPreparation";
        public const string Combat = "Combat";
        public const string ScenicPresentation = "survival.scenic-exploration";
    }

    public sealed class FarmSurvivalVisualCatalogEntry
    {
        public string VisualKey { get; set; } = string.Empty;
        public string CurrentFallbackVisualKey { get; set; } = string.Empty;
        public string PreferredSourcePack { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class FarmSurvivalVisualCatalog
    {
        private readonly Dictionary<string, FarmSurvivalVisualCatalogEntry> entries;

        public FarmSurvivalVisualCatalog(
            IEnumerable<FarmSurvivalVisualCatalogEntry> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            entries = values.ToDictionary(value => value.VisualKey,
                StringComparer.Ordinal);
            if (entries.Count == 0)
                throw new ArgumentException("FarmSurvivalVisualCatalogEmpty",
                    nameof(values));
            if (entries.Values.Any(value => string.IsNullOrWhiteSpace(value.VisualKey)
                || string.IsNullOrWhiteSpace(value.CurrentFallbackVisualKey)
                || string.IsNullOrWhiteSpace(value.PreferredSourcePack)
                || !value.PresentationOnly))
                throw new ArgumentException("FarmSurvivalVisualCatalogInvalid",
                    nameof(values));
        }

        public FarmSurvivalVisualCatalogEntry Resolve(string visualKey)
            => entries.TryGetValue(visualKey, out var value)
                ? Clone(value)
                : throw new InvalidOperationException(
                    "FarmSurvivalVisualKeyNotFound:" + visualKey);

        public static FarmSurvivalVisualCatalog CreateDefault()
            => new FarmSurvivalVisualCatalog(new[]
            {
                Entry(FarmSurvivalVisualKeys.PlayerSurvivor,
                    FarmSurvivalVisualKeys.GenericCharacterFallback,
                    "POLYGON Farm", "HumanoidLocomotion"),
                Entry(FarmSurvivalVisualKeys.FarmWorker,
                    FarmSurvivalVisualKeys.GenericCharacterFallback,
                    "POLYGON Farm", "HumanoidFarmWork"),
                Entry(FarmSurvivalVisualKeys.TilledSoil,
                    FarmSurvivalVisualKeys.FarmPropFallback,
                    "POLYGON Farm", "FarmSoilState"),
                Entry(FarmSurvivalVisualKeys.PreparedDefense,
                    FarmSurvivalVisualKeys.FarmPropFallback,
                    "POLYGON Apocalypse", "DefensiveStructure"),
                Entry(FarmSurvivalVisualKeys.DamagedDefense,
                    FarmSurvivalVisualKeys.FarmPropFallback,
                    "POLYGON Apocalypse", "RecoverableDamage"),
                Entry(FarmSurvivalVisualKeys.StylizedZombie,
                    FarmSurvivalVisualKeys.SkeletonThreatFallback,
                    "POLYGON Apocalypse", "StylizedZombieThreat"),
                Entry(FarmSurvivalVisualKeys.StylizedRaider,
                    FarmSurvivalVisualKeys.GenericCharacterFallback,
                    "POLYGON Apocalypse", "StylizedRaiderThreat"),
                Entry(FarmSurvivalVisualKeys.DamageMarker,
                    FarmSurvivalVisualKeys.FarmPropFallback,
                    "POLYGON Apocalypse", "RecoverableDamage"),
            });

        private static FarmSurvivalVisualCatalogEntry Entry(
            string visualKey,
            string fallback,
            string pack,
            string capability)
            => new FarmSurvivalVisualCatalogEntry
            {
                VisualKey = visualKey,
                CurrentFallbackVisualKey = fallback,
                PreferredSourcePack = pack,
                CapabilityCode = capability,
                PresentationOnly = true,
            };

        private static FarmSurvivalVisualCatalogEntry Clone(
            FarmSurvivalVisualCatalogEntry source)
            => new FarmSurvivalVisualCatalogEntry
            {
                VisualKey = source.VisualKey,
                CurrentFallbackVisualKey = source.CurrentFallbackVisualKey,
                PreferredSourcePack = source.PreferredSourcePack,
                CapabilityCode = source.CapabilityCode,
                PresentationOnly = source.PresentationOnly,
            };
    }

    public sealed class FarmSurvivalActorApiModel
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ActorKindCode { get; set; } = string.Empty;
        public bool Injured { get; set; }
    }

    public sealed class FarmSurvivalSoilTileApiModel
    {
        public string SoilTileStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class FarmSurvivalDefenseApiModel
    {
        public string DefenseStableId { get; set; } = string.Empty;
        public decimal Durability { get; set; }
        public bool Prepared { get; set; }
    }

    public sealed class FarmSurvivalEncounterApiModel
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int ThreatUnitCount { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public string[] AvailableChoiceStableIds { get; set; }
            = Array.Empty<string>();
        public int? DecisionDeadlineWorldTick { get; set; }
        public decimal DamageUnits { get; set; }
    }

    public sealed class FarmSurvivalStateApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public int ChapterDayNumber { get; set; } = 1;
        public string RuleRevision { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string FarmBuildingStableId { get; set; } = string.Empty;
        public FarmSurvivalActorApiModel[] Actors { get; set; }
            = Array.Empty<FarmSurvivalActorApiModel>();
        public FarmSurvivalSoilTileApiModel[] SoilTiles { get; set; }
            = Array.Empty<FarmSurvivalSoilTileApiModel>();
        public FarmSurvivalDefenseApiModel[] Defenses { get; set; }
            = Array.Empty<FarmSurvivalDefenseApiModel>();
        public FarmSurvivalEncounterApiModel[] Encounters { get; set; }
            = Array.Empty<FarmSurvivalEncounterApiModel>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class FarmSurvivalExperienceIntent
    {
        public string MoodCode { get; set; } = string.Empty;
        public string PrimaryPresentationKey { get; set; } = string.Empty;
        public bool ShowScenicHud { get; set; }
        public bool ShowCombatHud { get; set; }
        public bool ShowThreatVisuals { get; set; }
        public bool DirectCombatOptional { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    /// <summary>
    /// 경관 중심 규칙에서는 직접 전투를 선택하기 전까지 평온한 HUD와 경관을 유지한다.
    /// 서버 상태를 숨기지 않으며 화면 노출 우선순위만 결정한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public static class FarmSurvivalExperienceIntentMapper
    {
        public static FarmSurvivalExperienceIntent Map(
            FarmSurvivalStateApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.SimulationOnly || source.IsOperationalState)
                throw new InvalidOperationException("FarmSurvivalBoundaryInvalid");
            var encounters = source.Encounters
                ?? Array.Empty<FarmSurvivalEncounterApiModel>();
            var combat = encounters.FirstOrDefault(value => value.StateCode ==
                FarmSurvivalExperienceCodes.AwaitingCombat);
            if (combat != null)
                return new FarmSurvivalExperienceIntent
                {
                    MoodCode = FarmSurvivalExperienceCodes.Combat,
                    PrimaryPresentationKey = combat.PresentationKey,
                    ShowCombatHud = true,
                    ShowThreatVisuals = true,
                    DirectCombatOptional = source.RuleRevision ==
                        FarmSurvivalExperienceCodes.ScenicSeasonRuleRevision,
                };

            var seasonal = encounters.LastOrDefault(value =>
                value.StateCode != "Resolved");
            return new FarmSurvivalExperienceIntent
            {
                MoodCode = seasonal == null
                    ? FarmSurvivalExperienceCodes.Peaceful
                    : FarmSurvivalExperienceCodes.SeasonalPreparation,
                PrimaryPresentationKey = seasonal?.PresentationKey
                    ?? FarmSurvivalExperienceCodes.ScenicPresentation,
                ShowScenicHud = true,
                ShowCombatHud = false,
                ShowThreatVisuals = false,
                DirectCombatOptional = source.RuleRevision ==
                    FarmSurvivalExperienceCodes.ScenicSeasonRuleRevision,
            };
        }
    }

    public sealed class FarmSurvivalVisualIntent
    {
        public string InstanceStableId { get; set; } = string.Empty;
        public string AnchorStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string PreferredSourcePack { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
    }

    /// <summary>
    /// 서버가 확정한 상태를 시각 의도로만 바꾼다. Prefab 생성과 업무 상태 변경은 하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class FarmSurvivalVisualIntentMapper
    {
        private readonly FarmSurvivalVisualCatalog catalog;

        public FarmSurvivalVisualIntentMapper(FarmSurvivalVisualCatalog catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public FarmSurvivalVisualIntent[] Map(FarmSurvivalStateApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Require(source.SessionStableId, "FarmSurvivalSessionMissing");
            Require(source.TileKey, "FarmSurvivalTileMissing");
            Require(source.FarmBuildingStableId, "FarmSurvivalBuildingMissing");
            if (source.WorldRevision < 0 || source.WorldTick < 0)
                throw new InvalidOperationException("FarmSurvivalRevisionInvalid");
            if (!source.SimulationOnly || source.IsOperationalState)
                throw new InvalidOperationException("FarmSurvivalBoundaryInvalid");

            var result = new List<FarmSurvivalVisualIntent>();
            foreach (var actor in source.Actors ?? Array.Empty<FarmSurvivalActorApiModel>())
            {
                Require(actor.ActorStableId, "FarmSurvivalActorMissing");
                var visualKey = actor.ActorKindCode == "Player"
                    ? FarmSurvivalVisualKeys.PlayerSurvivor
                    : FarmSurvivalVisualKeys.FarmWorker;
                result.Add(Intent(actor.ActorStableId, source.FarmBuildingStableId,
                    source.TileKey, visualKey,
                    actor.Injured ? "survival.actor.injured" : "survival.actor.ready"));
            }
            foreach (var soil in source.SoilTiles ?? Array.Empty<FarmSurvivalSoilTileApiModel>())
            {
                if (soil.StateCode != "Tilled") continue;
                result.Add(Intent(soil.SoilTileStableId, soil.SoilTileStableId,
                    source.TileKey, FarmSurvivalVisualKeys.TilledSoil,
                    "survival.farm-soil.tilled"));
            }
            foreach (var defense in source.Defenses ?? Array.Empty<FarmSurvivalDefenseApiModel>())
            {
                var visualKey = defense.Prepared && defense.Durability > 0m
                    ? FarmSurvivalVisualKeys.PreparedDefense
                    : FarmSurvivalVisualKeys.DamagedDefense;
                result.Add(Intent(defense.DefenseStableId, defense.DefenseStableId,
                    source.TileKey, visualKey, "survival.defense.state"));
            }
            foreach (var encounter in source.Encounters
                ?? Array.Empty<FarmSurvivalEncounterApiModel>())
            {
                Require(encounter.EncounterStableId, "FarmSurvivalEncounterMissing");
                var showThreatVisual = source.RuleRevision !=
                        FarmSurvivalExperienceCodes.ScenicSeasonRuleRevision
                    || encounter.StateCode ==
                        FarmSurvivalExperienceCodes.AwaitingCombat;
                if (encounter.StateCode != "Resolved" && showThreatVisual)
                {
                    if (encounter.ThreatUnitCount <= 0 || encounter.ThreatUnitCount > 12)
                        throw new InvalidOperationException("FarmSurvivalThreatCountInvalid");
                    var visualKey = encounter.ThreatTypeCode == "ZombiePressure"
                        ? FarmSurvivalVisualKeys.StylizedZombie
                        : FarmSurvivalVisualKeys.StylizedRaider;
                    for (var index = 0; index < encounter.ThreatUnitCount; index++)
                        result.Add(Intent(encounter.EncounterStableId + ":unit:" + index,
                            source.FarmBuildingStableId, source.TileKey, visualKey,
                            encounter.PresentationKey));
                }
                if (encounter.DamageUnits > 0m)
                    result.Add(Intent(encounter.EncounterStableId + ":damage",
                        source.FarmBuildingStableId, source.TileKey,
                        FarmSurvivalVisualKeys.DamageMarker,
                        "survival.damage-assessment"));
            }
            return result.OrderBy(value => value.InstanceStableId,
                StringComparer.Ordinal).ToArray();
        }

        private FarmSurvivalVisualIntent Intent(
            string instanceStableId,
            string anchorStableId,
            string tileKey,
            string visualKey,
            string presentationKey)
        {
            var entry = catalog.Resolve(visualKey);
            return new FarmSurvivalVisualIntent
            {
                InstanceStableId = instanceStableId,
                AnchorStableId = anchorStableId,
                TileKey = tileKey,
                VisualKey = entry.VisualKey,
                FallbackVisualKey = entry.CurrentFallbackVisualKey,
                PreferredSourcePack = entry.PreferredSourcePack,
                PresentationKey = presentationKey ?? string.Empty,
                PresentationOnly = true,
            };
        }

        private static void Require(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorCode);
        }
    }
}
