using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class 재배활동Codes
    {
        public const string Sowing = "Sowing";
        public const string Harvest = "Harvest";

        internal static bool IsKnown(string value)
            => value == Sowing || value == Harvest;
    }

    public static class 재배생육단계Codes
    {
        public const string Sown = "Sown";
        public const string Emerged = "Emerged";
        public const string Vegetative = "Vegetative";
        public const string Bulking = "Bulking";
        public const string HarvestReady = "HarvestReady";
        public const string Harvested = "Harvested";

        internal static bool IsKnown(string value)
            => value == Sown || value == Emerged || value == Vegetative
                || value == Bulking || value == HarvestReady || value == Harvested;
    }

    public static class 재배LifecycleCommandCodes
    {
        public const string Sow = "Sow";
        public const string AdvanceDays = "AdvanceDays";
        public const string Harvest = "Harvest";

        internal static bool IsKnown(string value)
            => value == Sow || value == AdvanceDays || value == Harvest;
    }

    public static class 재배달력SourceTypeCodes
    {
        public const string Fixture = "Fixture";
        public const string OfficialReference = "OfficialReference";

        internal static bool IsKnown(string value)
            => value == Fixture || value == OfficialReference;
    }

    public sealed class 재배활동Window
    {
        public string ActivityCode { get; set; } = string.Empty;
        public int StartMonth { get; set; }
        public int StartDay { get; set; }
        public int EndMonth { get; set; }
        public int EndDay { get; set; }

        public bool Contains(DateTimeOffset value)
        {
            var point = value.Month * 100 + value.Day;
            var start = StartMonth * 100 + StartDay;
            var end = EndMonth * 100 + EndDay;
            return start <= end
                ? point >= start && point <= end
                : point >= start || point <= end;
        }
    }

    public sealed class 재배생육단계Definition
    {
        public string StageCode { get; set; } = string.Empty;
        public int MinimumDaysAfterSowing { get; set; }
    }

    public sealed class 재배달력ProfileSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public string RegionCode { get; set; } = string.Empty;
        public string CultivationMethodCode { get; set; } = string.Empty;
        public DateTimeOffset EffectiveOn { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string QualityCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
        public 재배활동Window[] ActivityWindows { get; set; } = Array.Empty<재배활동Window>();
    }

    public sealed class 재배SimulationRuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
        public 재배생육단계Definition[] GrowthStages { get; set; } = Array.Empty<재배생육단계Definition>();
        public decimal BaseHarvestQuantityKg { get; set; }
    }

    public sealed class 재배달력ProfileValidator
    {
        public void Validate(재배달력ProfileSnapshot profile)
        {
            if (profile == null
                || !StableDataId.IsValid(profile.StableId)
                || profile.Revision <= 0
                || !StableDataId.IsValid(profile.CanonicalProductStableId)
                || !StableDataId.IsValid(profile.CropVariantStableId)
                || string.IsNullOrWhiteSpace(profile.RegionCode)
                || string.IsNullOrWhiteSpace(profile.CultivationMethodCode)
                || profile.EffectiveOn == default
                || profile.EffectiveOn.TimeOfDay != TimeSpan.Zero
                || profile.EffectiveOn.Offset != TimeSpan.Zero
                || profile.SourceStableIds == null || profile.SourceStableIds.Length == 0
                || profile.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || profile.SourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != profile.SourceStableIds.Length
                || profile.Limitations == null || profile.Limitations.Length == 0
                || profile.Limitations.Any(string.IsNullOrWhiteSpace)
                || profile.ActivityWindows == null)
            {
                throw new InvalidOperationException("CultivationCalendarProfileInvalid");
            }

            if ((profile.SourceTypeCode == 재배달력SourceTypeCodes.Fixture)
                != (profile.QualityCode == 데이터품질Codes.Fixture))
            {
                throw new InvalidOperationException("CultivationCalendarProfileSourceQualityMismatch");
            }

            if (!재배달력SourceTypeCodes.IsKnown(profile.SourceTypeCode)
                || (profile.SourceTypeCode == 재배달력SourceTypeCodes.OfficialReference
                    && profile.QualityCode != 데이터품질Codes.Valid
                    && profile.QualityCode != 데이터품질Codes.Stale))
            {
                throw new InvalidOperationException("CultivationCalendarProfileSourceTypeInvalid");
            }

            ValidateWindows(profile.ActivityWindows);
        }

        private static void ValidateWindows(IReadOnlyCollection<재배활동Window> windows)
        {
            if (windows.Count != 2
                || windows.Select(value => value?.ActivityCode)
                    .Distinct(StringComparer.Ordinal).Count() != windows.Count
                || !windows.Any(value => value?.ActivityCode == 재배활동Codes.Sowing)
                || !windows.Any(value => value?.ActivityCode == 재배활동Codes.Harvest))
            {
                throw new InvalidOperationException("CultivationCalendarActivityWindowsInvalid");
            }

            foreach (var window in windows)
            {
                if (window == null || !재배활동Codes.IsKnown(window.ActivityCode)
                    || !IsMonthDay(window.StartMonth, window.StartDay)
                    || !IsMonthDay(window.EndMonth, window.EndDay))
                {
                    throw new InvalidOperationException("CultivationCalendarActivityWindowInvalid");
                }
            }
        }

        internal static void ValidateGrowthStages(IReadOnlyCollection<재배생육단계Definition> stages)
        {
            var ordered = stages
                .OrderBy(value => value?.MinimumDaysAfterSowing ?? int.MaxValue)
                .ThenBy(value => value?.StageCode, StringComparer.Ordinal)
                .ToArray();
            if (ordered.Length < 5
                || ordered.Any(value => value == null
                    || !재배생육단계Codes.IsKnown(value.StageCode)
                    || value.StageCode == 재배생육단계Codes.Harvested
                    || value.MinimumDaysAfterSowing < 0)
                || ordered.Select(value => value.StageCode).Distinct(StringComparer.Ordinal).Count()
                    != ordered.Length
                || ordered.Select(value => value.MinimumDaysAfterSowing).Distinct().Count()
                    != ordered.Length
                || ordered[0].StageCode != 재배생육단계Codes.Sown
                || ordered[0].MinimumDaysAfterSowing != 0
                || ordered[ordered.Length - 1].StageCode != 재배생육단계Codes.HarvestReady)
            {
                throw new InvalidOperationException("CultivationCalendarGrowthStagesInvalid");
            }
        }

        private static bool IsMonthDay(int month, int day)
        {
            try
            {
                _ = new DateTime(2000, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }

    public sealed class 재배작기SimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public string CalendarProfileStableId { get; set; } = string.Empty;
        public long CalendarProfileRevision { get; set; }
        public string PlotStableId { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
        public DateTimeOffset SownOn { get; set; }
        public int DaysAfterSowing { get; set; }
        public string GrowthStageCode { get; set; } = string.Empty;
        public DateTimeOffset? HarvestedOn { get; set; }
    }

    public sealed class 수확LotSimulationData
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string CultivationStableId { get; set; } = string.Empty;
        public DateTimeOffset HarvestedOn { get; set; }
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 감자재배LifecycleSimulationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long DataRevision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public 재배달력ProfileSnapshot CalendarProfile { get; set; } = new 재배달력ProfileSnapshot();
        public 재배SimulationRuleSnapshot SimulationRule { get; set; } = new 재배SimulationRuleSnapshot();
        public FarmSoilTileSimulationDataSnapshot Soil { get; set; } = new FarmSoilTileSimulationDataSnapshot();
        public 재배작기SimulationData? Cultivation { get; set; }
        public 수확LotSimulationData? HarvestLot { get; set; }
    }

    public sealed class 감자재배LifecyclePreview
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public DateTimeOffset SimulationDate { get; set; }
        public string TileStableId { get; set; } = string.Empty;
        public string CalendarProfileStableId { get; set; } = string.Empty;
        public long CalendarProfileRevision { get; set; }
        public bool RequiresExplicitConfirmation { get; set; }
    }

    public sealed class 감자재배LifecycleCommand
    {
        public string StableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public string? PreviewStableId { get; set; }
        public string SnapshotStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public long SimulationTick { get; set; }
        public string TileStableId { get; set; } = string.Empty;
        public int Days { get; set; }
    }

    public sealed class 감자재배LifecycleSimulationValidator
    {
        private readonly FarmSoilTileSimulationValidator soilValidator;
        private readonly 재배달력ProfileValidator calendarValidator;

        public 감자재배LifecycleSimulationValidator(
            FarmSoilTileSimulationValidator soil,
            재배달력ProfileValidator calendar)
        {
            soilValidator = soil ?? throw new ArgumentNullException(nameof(soil));
            calendarValidator = calendar ?? throw new ArgumentNullException(nameof(calendar));
        }

        public void Validate(감자재배LifecycleSimulationSnapshot snapshot)
        {
            if (snapshot == null
                || !StableDataId.IsValid(snapshot.StableId)
                || snapshot.DataRevision < 0
                || snapshot.ModeCode != "Simulation"
                || !StableDataId.IsValid(snapshot.ScenarioStableId)
                || string.IsNullOrWhiteSpace(snapshot.RuleRevision)
                || !IsUtcDate(snapshot.SimulationDate)
                || snapshot.GeneratedAt == default
                || snapshot.SourceStableIds == null || snapshot.SourceStableIds.Length == 0
                || snapshot.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || snapshot.SourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != snapshot.SourceStableIds.Length)
            {
                throw new InvalidOperationException("PotatoCultivationLifecycleSnapshotInvalid");
            }

            calendarValidator.Validate(snapshot.CalendarProfile);
            ValidateSimulationRule(snapshot.SimulationRule, snapshot.CalendarProfile);
            soilValidator.Validate(snapshot.Soil);
            if (snapshot.Soil.ScenarioStableId != snapshot.ScenarioStableId
                || snapshot.Soil.DataRevision != snapshot.DataRevision)
            {
                throw new InvalidOperationException("PotatoCultivationLifecycleSoilRevisionMismatch");
            }

            ValidateCultivation(snapshot);
            ValidateHarvestLot(snapshot);
        }

        private static void ValidateSimulationRule(
            재배SimulationRuleSnapshot rule,
            재배달력ProfileSnapshot calendar)
        {
            if (rule == null
                || !StableDataId.IsValid(rule.StableId)
                || rule.Revision <= 0
                || rule.CanonicalProductStableId != "product:potato"
                || rule.CanonicalProductStableId != calendar.CanonicalProductStableId
                || rule.CropVariantStableId != calendar.CropVariantStableId
                || rule.SourceTypeCode != 데이터SourceTypes.Fixture
                || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0
                || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.SourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != rule.SourceStableIds.Length
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace)
                || rule.GrowthStages == null
                || rule.BaseHarvestQuantityKg <= 0)
            {
                throw new InvalidOperationException("PotatoCultivationSimulationRuleInvalid");
            }

            재배달력ProfileValidator.ValidateGrowthStages(rule.GrowthStages);
        }

        private static void ValidateCultivation(감자재배LifecycleSimulationSnapshot snapshot)
        {
            var cultivation = snapshot.Cultivation;
            if (cultivation == null)
            {
                if (snapshot.HarvestLot != null)
                    throw new InvalidOperationException("PotatoHarvestLotWithoutCultivation");
                return;
            }

            if (!StableDataId.IsValid(cultivation.StableId)
                || cultivation.Revision <= 0
                || cultivation.CanonicalProductStableId != snapshot.CalendarProfile.CanonicalProductStableId
                || cultivation.CropVariantStableId != snapshot.CalendarProfile.CropVariantStableId
                || cultivation.CalendarProfileStableId != snapshot.CalendarProfile.StableId
                || cultivation.CalendarProfileRevision != snapshot.CalendarProfile.Revision
                || cultivation.PlotStableId != snapshot.Soil.PlotStableId
                || !snapshot.Soil.Tiles.Any(value => value.StableId == cultivation.TileStableId)
                || !IsUtcDate(cultivation.SownOn)
                || cultivation.DaysAfterSowing < 0
                || !재배생육단계Codes.IsKnown(cultivation.GrowthStageCode))
            {
                throw new InvalidOperationException("PotatoCultivationInvalid");
            }

            var tile = snapshot.Soil.Tiles.Single(value => value.StableId == cultivation.TileStableId);
            var harvested = cultivation.GrowthStageCode == 재배생육단계Codes.Harvested;
            if (harvested != cultivation.HarvestedOn.HasValue
                || (!harvested && tile.CultivationStateCode != FarmSoilTileCultivationStateCodes.Sown)
                || (!harvested && tile.ActiveCultivationStableId != cultivation.StableId)
                || (harvested && tile.CultivationStateCode != FarmSoilTileCultivationStateCodes.Harvested)
                || (harvested && tile.ActiveCultivationStableId != null))
            {
                throw new InvalidOperationException("PotatoCultivationTileStateMismatch");
            }
        }

        private static void ValidateHarvestLot(감자재배LifecycleSimulationSnapshot snapshot)
        {
            var lot = snapshot.HarvestLot;
            if (lot == null)
                return;

            if (snapshot.Cultivation == null
                || !StableDataId.IsValid(lot.StableId)
                || lot.Revision <= 0
                || lot.CanonicalProductStableId != snapshot.Cultivation.CanonicalProductStableId
                || lot.CultivationStableId != snapshot.Cultivation.StableId
                || !IsUtcDate(lot.HarvestedOn)
                || lot.Quantity <= 0
                || lot.UnitCode != "kg"
                || lot.SourceStableIds == null || lot.SourceStableIds.Length == 0
                || lot.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || lot.SourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != lot.SourceStableIds.Length)
            {
                throw new InvalidOperationException("PotatoHarvestLotInvalid");
            }
        }

        private static bool IsUtcDate(DateTimeOffset value)
            => value != default && value.Offset == TimeSpan.Zero && value.TimeOfDay == TimeSpan.Zero;
    }

    public sealed class 감자재배LifecycleSimulationEngine
    {
        private readonly 감자재배LifecycleSimulationValidator validator;

        public 감자재배LifecycleSimulationEngine(감자재배LifecycleSimulationValidator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public 감자재배LifecyclePreview PreviewSowing(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId)
        {
            validator.Validate(snapshot);
            if (snapshot.Cultivation != null || snapshot.HarvestLot != null)
                throw new InvalidOperationException("PotatoSowingAlreadyStarted");
            var tile = FindTile(snapshot, tileStableId);
            if (tile.CultivationStateCode != FarmSoilTileCultivationStateCodes.Tilled
                || tile.WorkStateCode != FarmSoilTileWorkStateCodes.None)
            {
                throw new InvalidOperationException("PotatoSowingNotAllowed:" + tileStableId);
            }
            if (!Window(snapshot.CalendarProfile, 재배활동Codes.Sowing).Contains(snapshot.SimulationDate))
                throw new InvalidOperationException("PotatoSowingOutsideCalendarWindow");

            return Preview(snapshot, tileStableId, 재배LifecycleCommandCodes.Sow);
        }

        public 감자재배LifecyclePreview PreviewHarvest(감자재배LifecycleSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            var cultivation = snapshot.Cultivation
                ?? throw new InvalidOperationException("PotatoHarvestCultivationMissing");
            if (cultivation.GrowthStageCode != 재배생육단계Codes.HarvestReady
                || snapshot.HarvestLot != null)
            {
                throw new InvalidOperationException("PotatoHarvestNotReady");
            }
            if (!Window(snapshot.CalendarProfile, 재배활동Codes.Harvest).Contains(snapshot.SimulationDate))
                throw new InvalidOperationException("PotatoHarvestOutsideCalendarWindow");

            return Preview(snapshot, cultivation.TileStableId, 재배LifecycleCommandCodes.Harvest);
        }

        public 감자재배LifecycleCommand Confirm(
            감자재배LifecycleSimulationSnapshot snapshot,
            감자재배LifecyclePreview preview)
        {
            validator.Validate(snapshot);
            if (preview == null || !preview.RequiresExplicitConfirmation
                || preview.SnapshotStableId != snapshot.StableId
                || preview.ExpectedDataRevision != snapshot.DataRevision
                || preview.SimulationDate != snapshot.SimulationDate
                || preview.CalendarProfileStableId != snapshot.CalendarProfile.StableId
                || preview.CalendarProfileRevision != snapshot.CalendarProfile.Revision
                || (preview.CommandCode != 재배LifecycleCommandCodes.Sow
                    && preview.CommandCode != 재배LifecycleCommandCodes.Harvest)
                || preview.StableId != PreviewId(snapshot, preview.TileStableId, preview.CommandCode))
            {
                throw new InvalidOperationException("PotatoCultivationPreviewStaleOrInvalid");
            }

            if (preview.CommandCode == 재배LifecycleCommandCodes.Sow)
                _ = PreviewSowing(snapshot, preview.TileStableId);
            else
                _ = PreviewHarvest(snapshot);

            return new 감자재배LifecycleCommand
            {
                StableId = CommandId(snapshot, preview.TileStableId, preview.CommandCode),
                CommandCode = preview.CommandCode,
                PreviewStableId = preview.StableId,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
                TileStableId = preview.TileStableId,
            };
        }

        public 감자재배LifecycleCommand CreateAdvanceDaysCommand(
            감자재배LifecycleSimulationSnapshot snapshot,
            int days)
        {
            validator.Validate(snapshot);
            if (snapshot.Cultivation == null
                || snapshot.Cultivation.GrowthStageCode == 재배생육단계Codes.Harvested)
            {
                throw new InvalidOperationException("PotatoAdvanceDaysCultivationMissing");
            }
            if (days <= 0 || days > 31)
                throw new InvalidOperationException("PotatoAdvanceDaysInvalid");

            return new 감자재배LifecycleCommand
            {
                StableId = CommandId(snapshot, snapshot.Cultivation.TileStableId,
                    재배LifecycleCommandCodes.AdvanceDays) + ".d"
                    + days.ToString(CultureInfo.InvariantCulture),
                CommandCode = 재배LifecycleCommandCodes.AdvanceDays,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationTick = snapshot.DataRevision + 1,
                TileStableId = snapshot.Cultivation.TileStableId,
                Days = days,
            };
        }

        public 감자재배LifecycleSimulationSnapshot Tick(
            감자재배LifecycleSimulationSnapshot snapshot,
            감자재배LifecycleCommand command)
        {
            validator.Validate(snapshot);
            if (command == null
                || !재배LifecycleCommandCodes.IsKnown(command.CommandCode)
                || command.SnapshotStableId != snapshot.StableId
                || command.ExpectedDataRevision != snapshot.DataRevision
                || command.SimulationTick != snapshot.DataRevision + 1)
            {
                throw new InvalidOperationException("PotatoCultivationCommandStale");
            }

            var expectedId = CommandId(snapshot, command.TileStableId, command.CommandCode)
                + (command.CommandCode == 재배LifecycleCommandCodes.AdvanceDays
                    ? ".d" + command.Days.ToString(CultureInfo.InvariantCulture)
                    : string.Empty);
            if (command.StableId != expectedId)
                throw new InvalidOperationException("PotatoCultivationCommandInvalid");

            if (command.CommandCode == 재배LifecycleCommandCodes.Sow)
                ValidateConfirmedPreview(snapshot, command, PreviewSowing(snapshot, command.TileStableId));
            else if (command.CommandCode == 재배LifecycleCommandCodes.Harvest)
                ValidateConfirmedPreview(snapshot, command, PreviewHarvest(snapshot));
            else if (command.PreviewStableId != null || command.Days <= 0 || command.Days > 31
                || snapshot.Cultivation == null
                || command.TileStableId != snapshot.Cultivation.TileStableId)
                throw new InvalidOperationException("PotatoAdvanceDaysCommandInvalid");

            var next = Clone(snapshot);
            next.DataRevision = command.SimulationTick;
            next.Soil.DataRevision = command.SimulationTick;
            next.GeneratedAt = snapshot.GeneratedAt.AddSeconds(1);
            next.SourceStableIds = snapshot.SourceStableIds.Concat(new[] { command.StableId }).ToArray();
            next.Soil.SourceStableIds = snapshot.Soil.SourceStableIds
                .Concat(new[] { command.StableId })
                .ToArray();

            switch (command.CommandCode)
            {
                case 재배LifecycleCommandCodes.Sow:
                    ApplySowing(next, command);
                    break;
                case 재배LifecycleCommandCodes.AdvanceDays:
                    ApplyAdvanceDays(next, command.Days);
                    break;
                case 재배LifecycleCommandCodes.Harvest:
                    ApplyHarvest(next, command);
                    break;
            }

            validator.Validate(next);
            return next;
        }

        private static void ApplySowing(
            감자재배LifecycleSimulationSnapshot snapshot,
            감자재배LifecycleCommand command)
        {
            var cultivationId = "cultivation:sim.potato."
                + snapshot.SimulationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".tile."
                + FindTile(snapshot, command.TileStableId).GridX.ToString(CultureInfo.InvariantCulture) + "."
                + FindTile(snapshot, command.TileStableId).GridZ.ToString(CultureInfo.InvariantCulture);
            snapshot.Cultivation = new 재배작기SimulationData
            {
                StableId = cultivationId,
                Revision = 1,
                CanonicalProductStableId = snapshot.CalendarProfile.CanonicalProductStableId,
                CropVariantStableId = snapshot.CalendarProfile.CropVariantStableId,
                CalendarProfileStableId = snapshot.CalendarProfile.StableId,
                CalendarProfileRevision = snapshot.CalendarProfile.Revision,
                PlotStableId = snapshot.Soil.PlotStableId,
                TileStableId = command.TileStableId,
                SownOn = snapshot.SimulationDate,
                GrowthStageCode = 재배생육단계Codes.Sown,
            };
            var tile = FindTile(snapshot, command.TileStableId);
            tile.Revision++;
            tile.CultivationStateCode = FarmSoilTileCultivationStateCodes.Sown;
            tile.ActiveCultivationStableId = cultivationId;
        }

        private static void ApplyAdvanceDays(
            감자재배LifecycleSimulationSnapshot snapshot,
            int days)
        {
            var cultivation = snapshot.Cultivation
                ?? throw new InvalidOperationException("PotatoAdvanceDaysCultivationMissing");
            snapshot.SimulationDate = snapshot.SimulationDate.AddDays(days);
            cultivation.DaysAfterSowing += days;
            cultivation.Revision++;
            cultivation.GrowthStageCode = snapshot.SimulationRule.GrowthStages
                .Where(value => value.MinimumDaysAfterSowing <= cultivation.DaysAfterSowing)
                .OrderByDescending(value => value.MinimumDaysAfterSowing)
                .ThenBy(value => value.StageCode, StringComparer.Ordinal)
                .First().StageCode;
        }

        private static void ApplyHarvest(
            감자재배LifecycleSimulationSnapshot snapshot,
            감자재배LifecycleCommand command)
        {
            var cultivation = snapshot.Cultivation
                ?? throw new InvalidOperationException("PotatoHarvestCultivationMissing");
            cultivation.Revision++;
            cultivation.GrowthStageCode = 재배생육단계Codes.Harvested;
            cultivation.HarvestedOn = snapshot.SimulationDate;
            var tile = FindTile(snapshot, command.TileStableId);
            tile.Revision++;
            tile.CultivationStateCode = FarmSoilTileCultivationStateCodes.Harvested;
            tile.ActiveCultivationStableId = null;
            snapshot.HarvestLot = new 수확LotSimulationData
            {
                StableId = "harvest-lot:sim.potato."
                    + snapshot.SimulationDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                    + ".r" + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture),
                Revision = 1,
                CanonicalProductStableId = cultivation.CanonicalProductStableId,
                CultivationStableId = cultivation.StableId,
                HarvestedOn = snapshot.SimulationDate,
                Quantity = snapshot.SimulationRule.BaseHarvestQuantityKg,
                UnitCode = "kg",
                SourceStableIds = new[] { cultivation.StableId, command.StableId },
            };
        }

        private static 감자재배LifecyclePreview Preview(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId,
            string commandCode)
            => new 감자재배LifecyclePreview
            {
                StableId = PreviewId(snapshot, tileStableId, commandCode),
                CommandCode = commandCode,
                SnapshotStableId = snapshot.StableId,
                ExpectedDataRevision = snapshot.DataRevision,
                SimulationDate = snapshot.SimulationDate,
                TileStableId = tileStableId,
                CalendarProfileStableId = snapshot.CalendarProfile.StableId,
                CalendarProfileRevision = snapshot.CalendarProfile.Revision,
                RequiresExplicitConfirmation = true,
            };

        private static void ValidateConfirmedPreview(
            감자재배LifecycleSimulationSnapshot snapshot,
            감자재배LifecycleCommand command,
            감자재배LifecyclePreview expected)
        {
            if (command.PreviewStableId != expected.StableId)
                throw new InvalidOperationException("PotatoCultivationCommandInvalid");
        }

        private static string PreviewId(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId,
            string commandCode)
            => "farm-" + commandCode.ToLowerInvariant() + "-preview:sim.potato.r"
                + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture)
                + "." + TileSuffix(snapshot, tileStableId);

        private static string CommandId(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId,
            string commandCode)
            => "farm-" + commandCode.ToLowerInvariant() + "-command:sim.potato.r"
                + snapshot.DataRevision.ToString(CultureInfo.InvariantCulture)
                + "." + TileSuffix(snapshot, tileStableId);

        private static string TileSuffix(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId)
        {
            var tile = FindTile(snapshot, tileStableId);
            return "tile." + tile.GridX.ToString(CultureInfo.InvariantCulture)
                + "." + tile.GridZ.ToString(CultureInfo.InvariantCulture);
        }

        private static FarmSoilTileSimulationData FindTile(
            감자재배LifecycleSimulationSnapshot snapshot,
            string tileStableId)
            => snapshot.Soil.Tiles.SingleOrDefault(value => value.StableId == tileStableId)
                ?? throw new InvalidOperationException("PotatoCultivationTileMissing:" + tileStableId);

        private static 재배활동Window Window(
            재배달력ProfileSnapshot profile,
            string activityCode)
            => profile.ActivityWindows.Single(value => value.ActivityCode == activityCode);

        private static 감자재배LifecycleSimulationSnapshot Clone(
            감자재배LifecycleSimulationSnapshot source)
            => new 감자재배LifecycleSimulationSnapshot
            {
                StableId = source.StableId,
                DataRevision = source.DataRevision,
                ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId,
                RuleRevision = source.RuleRevision,
                SimulationDate = source.SimulationDate,
                GeneratedAt = source.GeneratedAt,
                SourceStableIds = source.SourceStableIds.ToArray(),
                CalendarProfile = CloneCalendar(source.CalendarProfile),
                SimulationRule = CloneRule(source.SimulationRule),
                Soil = CloneSoil(source.Soil),
                Cultivation = source.Cultivation == null ? null : new 재배작기SimulationData
                {
                    StableId = source.Cultivation.StableId,
                    Revision = source.Cultivation.Revision,
                    CanonicalProductStableId = source.Cultivation.CanonicalProductStableId,
                    CropVariantStableId = source.Cultivation.CropVariantStableId,
                    CalendarProfileStableId = source.Cultivation.CalendarProfileStableId,
                    CalendarProfileRevision = source.Cultivation.CalendarProfileRevision,
                    PlotStableId = source.Cultivation.PlotStableId,
                    TileStableId = source.Cultivation.TileStableId,
                    SownOn = source.Cultivation.SownOn,
                    DaysAfterSowing = source.Cultivation.DaysAfterSowing,
                    GrowthStageCode = source.Cultivation.GrowthStageCode,
                    HarvestedOn = source.Cultivation.HarvestedOn,
                },
                HarvestLot = source.HarvestLot == null ? null : new 수확LotSimulationData
                {
                    StableId = source.HarvestLot.StableId,
                    Revision = source.HarvestLot.Revision,
                    CanonicalProductStableId = source.HarvestLot.CanonicalProductStableId,
                    CultivationStableId = source.HarvestLot.CultivationStableId,
                    HarvestedOn = source.HarvestLot.HarvestedOn,
                    Quantity = source.HarvestLot.Quantity,
                    UnitCode = source.HarvestLot.UnitCode,
                    SourceStableIds = source.HarvestLot.SourceStableIds.ToArray(),
                },
            };

        private static 재배달력ProfileSnapshot CloneCalendar(재배달력ProfileSnapshot source)
            => new 재배달력ProfileSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                CanonicalProductStableId = source.CanonicalProductStableId,
                CropVariantStableId = source.CropVariantStableId,
                RegionCode = source.RegionCode,
                CultivationMethodCode = source.CultivationMethodCode,
                EffectiveOn = source.EffectiveOn,
                SourceTypeCode = source.SourceTypeCode,
                QualityCode = source.QualityCode,
                SourceStableIds = source.SourceStableIds.ToArray(),
                Limitations = source.Limitations.ToArray(),
                ActivityWindows = source.ActivityWindows.Select(value => new 재배활동Window
                {
                    ActivityCode = value.ActivityCode,
                    StartMonth = value.StartMonth,
                    StartDay = value.StartDay,
                    EndMonth = value.EndMonth,
                    EndDay = value.EndDay,
                }).ToArray(),
            };

        private static 재배SimulationRuleSnapshot CloneRule(재배SimulationRuleSnapshot source)
            => new 재배SimulationRuleSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                CanonicalProductStableId = source.CanonicalProductStableId,
                CropVariantStableId = source.CropVariantStableId,
                SourceTypeCode = source.SourceTypeCode,
                SourceStableIds = source.SourceStableIds.ToArray(),
                Limitations = source.Limitations.ToArray(),
                GrowthStages = source.GrowthStages.Select(value => new 재배생육단계Definition
                {
                    StageCode = value.StageCode,
                    MinimumDaysAfterSowing = value.MinimumDaysAfterSowing,
                }).ToArray(),
                BaseHarvestQuantityKg = source.BaseHarvestQuantityKg,
            };

        private static FarmSoilTileSimulationDataSnapshot CloneSoil(
            FarmSoilTileSimulationDataSnapshot source)
            => new FarmSoilTileSimulationDataSnapshot
            {
                StableId = source.StableId,
                DataRevision = source.DataRevision,
                ModeCode = source.ModeCode,
                ScenarioStableId = source.ScenarioStableId,
                ScenarioSeed = source.ScenarioSeed,
                RuleRevision = source.RuleRevision,
                PlotStableId = source.PlotStableId,
                GridWidth = source.GridWidth,
                GridHeight = source.GridHeight,
                GeneratedAt = source.GeneratedAt,
                SourceStableIds = source.SourceStableIds.ToArray(),
                Tiles = source.Tiles.Select(value => new FarmSoilTileSimulationData
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

    public sealed class 감자재배LifecyclePresentationModel
    {
        public long SourceRevision { get; set; }
        public string SourceModeCode { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string SimulationDateText { get; set; } = string.Empty;
        public string CalendarContextText { get; set; } = string.Empty;
        public string GrowthStageCode { get; set; } = string.Empty;
        public bool CanPreviewSowing { get; set; }
        public bool CanPreviewHarvest { get; set; }
        public string HarvestLotText { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class 감자재배LifecycleProjector
    {
        private readonly 감자재배LifecycleSimulationValidator validator;

        public 감자재배LifecycleProjector(감자재배LifecycleSimulationValidator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public 감자재배LifecyclePresentationModel Project(
            감자재배LifecycleSimulationSnapshot snapshot)
        {
            validator.Validate(snapshot);
            var sowingWindow = snapshot.CalendarProfile.ActivityWindows
                .Single(value => value.ActivityCode == 재배활동Codes.Sowing);
            var harvestWindow = snapshot.CalendarProfile.ActivityWindows
                .Single(value => value.ActivityCode == 재배활동Codes.Harvest);
            return new 감자재배LifecyclePresentationModel
            {
                SourceRevision = snapshot.DataRevision,
                SourceModeCode = snapshot.ModeCode + "/" + snapshot.CalendarProfile.SourceTypeCode,
                CanonicalProductStableId = snapshot.CalendarProfile.CanonicalProductStableId,
                SimulationDateText = snapshot.SimulationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CalendarContextText = snapshot.CalendarProfile.RegionCode + " · "
                    + snapshot.CalendarProfile.CultivationMethodCode
                    + " · 파종 " + WindowText(sowingWindow)
                    + " · 수확 " + WindowText(harvestWindow),
                GrowthStageCode = snapshot.Cultivation?.GrowthStageCode ?? "NotStarted",
                CanPreviewSowing = snapshot.Cultivation == null
                    && sowingWindow.Contains(snapshot.SimulationDate)
                    && snapshot.Soil.Tiles.Any(value =>
                        value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled
                        && value.WorkStateCode == FarmSoilTileWorkStateCodes.None),
                CanPreviewHarvest = snapshot.Cultivation?.GrowthStageCode
                    == 재배생육단계Codes.HarvestReady
                    && harvestWindow.Contains(snapshot.SimulationDate),
                HarvestLotText = snapshot.HarvestLot == null
                    ? "수확 Lot 없음"
                    : snapshot.HarvestLot.StableId + " · "
                        + snapshot.HarvestLot.Quantity.ToString(CultureInfo.InvariantCulture)
                        + snapshot.HarvestLot.UnitCode,
                LimitationText = string.Join(" · ", snapshot.CalendarProfile.Limitations
                    .Concat(snapshot.SimulationRule.Limitations)),
            };
        }

        private static string WindowText(재배활동Window value)
            => value.StartMonth.ToString(CultureInfo.InvariantCulture) + "/"
                + value.StartDay.ToString(CultureInfo.InvariantCulture) + "~"
                + value.EndMonth.ToString(CultureInfo.InvariantCulture) + "/"
                + value.EndDay.ToString(CultureInfo.InvariantCulture);
    }

    public static class 감자재배LifecycleSimulationFixture
    {
        public static 재배달력ProfileSnapshot CreateCalendarProfile()
            => new 재배달력ProfileSnapshot
            {
                StableId = "cultivation-calendar:fixture.potato.open-field.central-kr.1",
                Revision = 1,
                CanonicalProductStableId = "product:potato",
                CropVariantStableId = "crop-variant:fixture.potato.open-field.1",
                RegionCode = "FixtureCentralKr",
                CultivationMethodCode = "OpenFieldFixture",
                EffectiveOn = UtcDate(2026, 1, 1),
                SourceTypeCode = 재배달력SourceTypeCodes.Fixture,
                QualityCode = 데이터품질Codes.Fixture,
                SourceStableIds = new[] { "source:fixture.potato.calendar.1" },
                Limitations = new[]
                {
                    "검증용 Simulation fixture이며 실제 파종·수확 권고나 농업 처방이 아닙니다.",
                },
                ActivityWindows = new[]
                {
                    new 재배활동Window
                    {
                        ActivityCode = 재배활동Codes.Sowing,
                        StartMonth = 4,
                        StartDay = 1,
                        EndMonth = 4,
                        EndDay = 30,
                    },
                    new 재배활동Window
                    {
                        ActivityCode = 재배활동Codes.Harvest,
                        StartMonth = 4,
                        StartDay = 7,
                        EndMonth = 5,
                        EndDay = 31,
                    },
                },
            };

        public static 재배SimulationRuleSnapshot CreateSimulationRule()
            => new 재배SimulationRuleSnapshot
            {
                StableId = "cultivation-simulation-rule:fixture.potato.open-field.1",
                Revision = 1,
                CanonicalProductStableId = "product:potato",
                CropVariantStableId = "crop-variant:fixture.potato.open-field.1",
                SourceTypeCode = 데이터SourceTypes.Fixture,
                SourceStableIds = new[] { "simulation-assumption:potato.growth-and-yield.1" },
                Limitations = new[]
                {
                    "생육 일수와 수확량은 검증용 게임 규칙이며 실제 농업 예측이 아닙니다.",
                },
                GrowthStages = new[]
                {
                    new 재배생육단계Definition { StageCode = 재배생육단계Codes.Sown, MinimumDaysAfterSowing = 0 },
                    new 재배생육단계Definition { StageCode = 재배생육단계Codes.Emerged, MinimumDaysAfterSowing = 1 },
                    new 재배생육단계Definition { StageCode = 재배생육단계Codes.Vegetative, MinimumDaysAfterSowing = 2 },
                    new 재배생육단계Definition { StageCode = 재배생육단계Codes.Bulking, MinimumDaysAfterSowing = 4 },
                    new 재배생육단계Definition { StageCode = 재배생육단계Codes.HarvestReady, MinimumDaysAfterSowing = 6 },
                },
                BaseHarvestQuantityKg = 300m,
            };

        public static 감자재배LifecycleSimulationSnapshot Create()
        {
            var soil = FarmPotatoSoilTileSimulationFixture.Create();
            soil.DataRevision = 2;
            soil.GeneratedAt = UtcDate(2026, 4, 1);
            return new 감자재배LifecycleSimulationSnapshot
            {
                StableId = "farm-crop-lifecycle-snapshot:sim.potato.1",
                DataRevision = soil.DataRevision,
                ModeCode = "Simulation",
                ScenarioStableId = soil.ScenarioStableId,
                RuleRevision = "farm-potato-lifecycle-rule:1",
                SimulationDate = UtcDate(2026, 4, 1),
                GeneratedAt = UtcDate(2026, 4, 1),
                SourceStableIds = new[]
                {
                    "simulation-assumption:farm.potato.lifecycle.1",
                    "source:fixture.potato.calendar.1",
                },
                CalendarProfile = CreateCalendarProfile(),
                SimulationRule = CreateSimulationRule(),
                Soil = soil,
            };
        }

        private static DateTimeOffset UtcDate(int year, int month, int day)
            => new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
    }
}
