using System;
using System.Linq;

namespace Ssalddel.Unity.Exhibition
{
    public static class PotatoProductionRuleSeedbedCodes
    {
        public const string ScenarioStableId = "rule-seedbed:production.potato";
        public const string RuleDomainCode = "Production";
        public const string ModeCode = "Simulation";
        public const string PendingEffectStateCode = "Pending";
        public const string ProductionMutationKindCode = "Production";
        public const string OutputRoleCode = "Output";
        public const string PreviewStepCode = "HarvestLot";
    }

    public sealed class PotatoProductionRuleSeedbedEffectLineApiSnapshot
    {
        public string EffectLineStableId { get; set; } = string.Empty;
        public string MutationKindCode { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string TargetLedgerStableId { get; set; } = string.Empty;
        public decimal BeforeValue { get; set; }
        public decimal DeltaValue { get; set; }
        public decimal AfterValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoProductionRuleSeedbedPreviewApiSnapshot
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long WorldTick { get; set; }
        public bool IsServerAuthoritative { get; set; }
        public string PreviewStableId { get; set; } = string.Empty;
        public long BasedOnRevision { get; set; }
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string RuleStableId { get; set; } = string.Empty;
        public long RuleRevision { get; set; }
        public string RuleDomainCode { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public string EffectStateCode { get; set; } = string.Empty;
        public string CultivationUnitStableId { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public decimal EffectiveCultivationAreaSquareMeters { get; set; }
        public decimal BaseHarvestQuantityKilograms { get; set; }
        public decimal ExpectedHarvestQuantityKilograms { get; set; }
        public PotatoProductionRuleSeedbedEffectLineApiSnapshot[] EffectLines { get; set; }
            = Array.Empty<PotatoProductionRuleSeedbedEffectLineApiSnapshot>();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoProductionRuleSeedbedLedgerApiSnapshot
    {
        public string LedgerStableId { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class PotatoProductionRuleSeedbedCanonicalApiSnapshot
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long WorldTick { get; set; }
        public bool IsServerAuthoritative { get; set; }
        public PotatoProductionRuleSeedbedLedgerApiSnapshot[] Ledgers { get; set; }
            = Array.Empty<PotatoProductionRuleSeedbedLedgerApiSnapshot>();
        public string[] AppliedEffectBundleStableIds { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoProductionRuleSeedbedEnvelope
    {
        public RuleSeedbedScenarioDescriptor Scenario { get; set; }
            = new RuleSeedbedScenarioDescriptor();
        public RuleSeedbedCanonicalStateSnapshot Baseline { get; set; }
            = new RuleSeedbedCanonicalStateSnapshot();
        public RuleSeedbedPreviewSnapshot Preview { get; set; }
            = new RuleSeedbedPreviewSnapshot();
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string CultivationUnitStableId { get; set; } = string.Empty;
        public string TileStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public decimal EffectiveCultivationAreaSquareMeters { get; set; }
        public decimal BaseHarvestQuantityKilograms { get; set; }
        public decimal ExpectedHarvestQuantityKilograms { get; set; }
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoProductionRuleSeedbedAdapter
    {
        public PotatoProductionRuleSeedbedEnvelope MapPreview(
            PotatoProductionRuleSeedbedPreviewApiSnapshot source)
        {
            ValidatePreviewSource(source);
            var scenario = IntegratedRuleSeedbedCatalog.Create()
                .Single(value => value.ScenarioStableId
                    == PotatoProductionRuleSeedbedCodes.ScenarioStableId);

            if (source.RuleStableId != scenario.RuleStableId)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoRuleMismatch");
            }

            var baselineValues = source.EffectLines
                .Select(line => new RuleSeedbedResourceValueSnapshot
                {
                    LedgerStableId = line.TargetLedgerStableId,
                    Value = line.BeforeValue,
                    Unit = line.UnitCode,
                })
                .ToArray();
            var effects = source.EffectLines
                .Select(line => new RuleSeedbedEffectSnapshot
                {
                    EffectStableId = line.EffectLineStableId,
                    StepCode = PotatoProductionRuleSeedbedCodes.PreviewStepCode,
                    TargetStableId = line.TargetLedgerStableId,
                    BeforeValue = line.BeforeValue,
                    DeltaValue = line.DeltaValue,
                    AfterValue = line.AfterValue,
                    Unit = line.UnitCode,
                    IsCanonicalResourceEffect = true,
                    SourceStableIds = line.SourceStableIds.ToArray(),
                })
                .ToArray();

            return new PotatoProductionRuleSeedbedEnvelope
            {
                Scenario = scenario,
                Baseline = new RuleSeedbedCanonicalStateSnapshot
                {
                    SnapshotStableId = source.SnapshotStableId,
                    Revision = source.Revision,
                    WorldTick = source.WorldTick,
                    IsServerAuthoritative = source.IsServerAuthoritative,
                    Values = baselineValues,
                    SourceStableIds = source.SourceStableIds.ToArray(),
                },
                Preview = new RuleSeedbedPreviewSnapshot
                {
                    PreviewStableId = source.PreviewStableId,
                    ScenarioStableId = scenario.ScenarioStableId,
                    BasedOnRevision = source.BasedOnRevision,
                    RuleStableId = source.RuleStableId,
                    Effects = effects,
                    BlockingReasonCodes = source.BlockingReasonCodes.ToArray(),
                    SourceStableIds = source.SourceStableIds.ToArray(),
                },
                EffectBundleStableId = source.EffectBundleStableId,
                CultivationUnitStableId = source.CultivationUnitStableId,
                TileStableId = source.TileStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                EffectiveCultivationAreaSquareMeters = source.EffectiveCultivationAreaSquareMeters,
                BaseHarvestQuantityKilograms = source.BaseHarvestQuantityKilograms,
                ExpectedHarvestQuantityKilograms = source.ExpectedHarvestQuantityKilograms,
                Limitations = source.Limitations.ToArray(),
            };
        }

        public RuleSeedbedCanonicalStateSnapshot MapCanonicalRefresh(
            PotatoProductionRuleSeedbedCanonicalApiSnapshot source,
            string expectedEffectBundleStableId)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            RequireText(expectedEffectBundleStableId, "RuleSeedbedPotatoEffectBundleMissing");
            RequireText(source.SnapshotStableId, "RuleSeedbedPotatoCanonicalSnapshotMissing");
            RequireSources(source.SourceStableIds, "RuleSeedbedPotatoCanonicalSourcesMissing");
            if (!source.IsServerAuthoritative)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoCanonicalAuthorityInvalid");
            }

            if (source.Revision <= 0 || source.WorldTick < 0)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoCanonicalRevisionInvalid");
            }

            if (source.AppliedEffectBundleStableIds == null
                || !source.AppliedEffectBundleStableIds.Contains(
                    expectedEffectBundleStableId,
                    StringComparer.Ordinal))
            {
                throw new InvalidOperationException("RuleSeedbedPotatoEffectBundleNotApplied");
            }

            if (source.Ledgers == null || source.Ledgers.Length == 0)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoCanonicalLedgersMissing");
            }

            var duplicateLedger = source.Ledgers
                .GroupBy(value => value.LedgerStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateLedger != null)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoCanonicalLedgerDuplicate");
            }

            var values = source.Ledgers.Select(value =>
            {
                RequireText(value.LedgerStableId, "RuleSeedbedPotatoCanonicalLedgerInvalid");
                RequireText(value.UnitCode, "RuleSeedbedPotatoCanonicalLedgerUnitInvalid");
                return new RuleSeedbedResourceValueSnapshot
                {
                    LedgerStableId = value.LedgerStableId,
                    Value = value.Value,
                    Unit = value.UnitCode,
                };
            }).ToArray();

            return new RuleSeedbedCanonicalStateSnapshot
            {
                SnapshotStableId = source.SnapshotStableId,
                Revision = source.Revision,
                WorldTick = source.WorldTick,
                IsServerAuthoritative = true,
                Values = values,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
        }

        private static void ValidatePreviewSource(
            PotatoProductionRuleSeedbedPreviewApiSnapshot source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            RequireText(source.SnapshotStableId, "RuleSeedbedPotatoSnapshotMissing");
            RequireText(source.PreviewStableId, "RuleSeedbedPotatoPreviewMissing");
            RequireText(source.EffectBundleStableId, "RuleSeedbedPotatoEffectBundleMissing");
            RequireText(source.RuleStableId, "RuleSeedbedPotatoRuleMissing");
            RequireText(source.CultivationUnitStableId, "RuleSeedbedPotatoCultivationUnitMissing");
            RequireText(source.TileStableId, "RuleSeedbedPotatoTileMissing");
            RequireText(source.HarvestLotStableId, "RuleSeedbedPotatoHarvestLotMissing");
            RequireSources(source.SourceStableIds, "RuleSeedbedPotatoSourcesMissing");
            RequireSources(source.Limitations, "RuleSeedbedPotatoLimitationsMissing");

            if (!source.IsServerAuthoritative)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoAuthorityInvalid");
            }

            if (source.Revision <= 0
                || source.BasedOnRevision != source.Revision
                || source.WorldTick < 0
                || source.RuleRevision <= 0)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoRevisionInvalid");
            }

            if (source.RuleDomainCode != PotatoProductionRuleSeedbedCodes.RuleDomainCode
                || source.ModeCode != PotatoProductionRuleSeedbedCodes.ModeCode
                || source.EffectStateCode != PotatoProductionRuleSeedbedCodes.PendingEffectStateCode)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoBoundaryInvalid");
            }

            if (source.EffectiveCultivationAreaSquareMeters <= 0m
                || source.BaseHarvestQuantityKilograms <= 0m
                || source.ExpectedHarvestQuantityKilograms <= 0m)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoQuantityInvalid");
            }

            if (source.EffectLines == null || source.EffectLines.Length != 1)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoEffectLineCountInvalid");
            }

            var line = source.EffectLines[0];
            RequireText(line.EffectLineStableId, "RuleSeedbedPotatoEffectLineMissing");
            RequireText(line.TargetLedgerStableId, "RuleSeedbedPotatoLedgerMissing");
            RequireText(line.UnitCode, "RuleSeedbedPotatoUnitMissing");
            RequireSources(line.SourceStableIds, "RuleSeedbedPotatoEffectSourcesMissing");
            if (line.MutationKindCode != PotatoProductionRuleSeedbedCodes.ProductionMutationKindCode
                || line.RoleCode != PotatoProductionRuleSeedbedCodes.OutputRoleCode
                || line.UnitCode != "kg"
                || line.BeforeValue != 0m
                || line.DeltaValue <= 0m
                || line.BeforeValue + line.DeltaValue != line.AfterValue
                || line.AfterValue != source.ExpectedHarvestQuantityKilograms)
            {
                throw new InvalidOperationException("RuleSeedbedPotatoEffectInvalid");
            }
        }

        private static void RequireSources(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException(errorCode);
            }
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(errorCode);
            }
        }
    }
}
