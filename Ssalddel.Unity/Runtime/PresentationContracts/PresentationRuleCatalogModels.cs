using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.PresentationContracts
{
    public static class 표현규칙영역Codes
    {
        public const string Graphics = "Presentation.Graphics";
        public const string Camera = "Presentation.Camera";
        public const string Animation = "Presentation.Animation";
        public const string Lighting = "Presentation.Lighting";
        public const string Audio = "Presentation.Audio";
        public const string UI = "Presentation.UI";

        public static readonly string[] All =
        {
            Graphics, Camera, Animation, Lighting, Audio, UI,
        };
    }

    public static class 표현규칙구현상태Codes
    {
        public const string ExistingRuleMapped = "ExistingRuleMapped";
        public const string ContractPrepared = "ContractPrepared";
    }

    public static class 표현출력채널Codes
    {
        public const string Material = "Material";
        public const string Color = "Color";
        public const string MeshVariant = "MeshVariant";
        public const string LOD = "LOD";
        public const string FX = "FX";
        public const string FocusTarget = "FocusTarget";
        public const string Distance = "Distance";
        public const string Framing = "Framing";
        public const string Transition = "Transition";
        public const string AnimatorState = "AnimatorState";
        public const string MovementPlayback = "MovementPlayback";
        public const string PlaybackSpeed = "PlaybackSpeed";
        public const string Light = "Light";
        public const string Ambient = "Ambient";
        public const string Fog = "Fog";
        public const string TimeOfDayVisual = "TimeOfDayVisual";
        public const string AudioCue = "AudioCue";
        public const string Volume = "Volume";
        public const string SpatialBlend = "SpatialBlend";
        public const string Label = "Label";
        public const string Icon = "Icon";
        public const string Panel = "Panel";
        public const string Badge = "Badge";
        public const string Progress = "Progress";
    }

    public sealed class 표현규칙Descriptor
    {
        public string RuleStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string DomainCode { get; set; } = string.Empty;
        public string ImplementationStateCode { get; set; } = string.Empty;
        public string? LegacyVisualRuleRevision { get; set; }
        public string PresentationContractVersion { get; set; } = string.Empty;
        public string[] InputPresentationStateCodes { get; set; } = Array.Empty<string>();
        public string[] OutputChannelCodes { get; set; } = Array.Empty<string>();
        public string[] AppliesToVisualKeys { get; set; } = Array.Empty<string>();
        public bool MutatesCanonicalState { get; set; }
        public bool ConfirmsBusinessCompletion { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class 표현규칙CatalogSnapshot
    {
        public string CatalogStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public 표현규칙Descriptor[] Rules { get; set; } = Array.Empty<표현규칙Descriptor>();
    }

    public sealed class 표현규칙Validator
    {
        private static readonly IReadOnlyDictionary<string, HashSet<string>> AllowedChannels =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
            {
                [표현규칙영역Codes.Graphics] = Set(
                    표현출력채널Codes.Material, 표현출력채널Codes.Color,
                    표현출력채널Codes.MeshVariant, 표현출력채널Codes.LOD,
                    표현출력채널Codes.FX),
                [표현규칙영역Codes.Camera] = Set(
                    표현출력채널Codes.FocusTarget, 표현출력채널Codes.Distance,
                    표현출력채널Codes.Framing, 표현출력채널Codes.Transition),
                [표현규칙영역Codes.Animation] = Set(
                    표현출력채널Codes.AnimatorState, 표현출력채널Codes.MovementPlayback,
                    표현출력채널Codes.PlaybackSpeed),
                [표현규칙영역Codes.Lighting] = Set(
                    표현출력채널Codes.Light, 표현출력채널Codes.Ambient,
                    표현출력채널Codes.Fog, 표현출력채널Codes.TimeOfDayVisual),
                [표현규칙영역Codes.Audio] = Set(
                    표현출력채널Codes.AudioCue, 표현출력채널Codes.Volume,
                    표현출력채널Codes.SpatialBlend),
                [표현규칙영역Codes.UI] = Set(
                    표현출력채널Codes.Label, 표현출력채널Codes.Icon,
                    표현출력채널Codes.Panel, 표현출력채널Codes.Badge,
                    표현출력채널Codes.Progress),
            };

        public void Validate(표현규칙CatalogSnapshot catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            RequireId(catalog.CatalogStableId, "PresentationRuleCatalogStableIdInvalid");
            if (catalog.Revision <= 0 || catalog.Rules == null || catalog.Rules.Length == 0)
                throw new InvalidOperationException("PresentationRuleCatalogInvalid");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rule in catalog.Rules)
            {
                Validate(rule);
                if (!ids.Add(rule.RuleStableId.Trim()))
                    throw new InvalidOperationException("PresentationRuleStableIdDuplicate");
            }
        }

        public void Validate(표현규칙Descriptor rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            RequireId(rule.RuleStableId, "PresentationRuleStableIdInvalid");
            if (rule.Revision <= 0 || !AllowedChannels.TryGetValue(rule.DomainCode, out var allowed))
                throw new InvalidOperationException("PresentationRuleDomainInvalid");
            if (rule.ImplementationStateCode != 표현규칙구현상태Codes.ExistingRuleMapped
                && rule.ImplementationStateCode != 표현규칙구현상태Codes.ContractPrepared)
                throw new InvalidOperationException("PresentationRuleImplementationStateInvalid");
            if (rule.ImplementationStateCode == 표현규칙구현상태Codes.ExistingRuleMapped
                && string.IsNullOrWhiteSpace(rule.LegacyVisualRuleRevision))
                throw new InvalidOperationException("PresentationRuleLegacyRevisionMissing");
            if (rule.ImplementationStateCode == 표현규칙구현상태Codes.ContractPrepared
                && !string.IsNullOrWhiteSpace(rule.LegacyVisualRuleRevision))
                throw new InvalidOperationException("PresentationRulePreparedLegacyRevisionUnexpected");
            RequireId(rule.PresentationContractVersion,
                "PresentationRuleContractVersionInvalid");
            ValidateValues(rule.InputPresentationStateCodes, true,
                "PresentationRuleInputsInvalid");
            ValidateValues(rule.OutputChannelCodes, true,
                "PresentationRuleOutputsInvalid");
            if (rule.OutputChannelCodes.Any(value => !allowed.Contains(value)))
                throw new InvalidOperationException("PresentationRuleOutputChannelNotAllowed");
            ValidateValues(rule.AppliesToVisualKeys, true,
                "PresentationRuleVisualKeysInvalid");
            ValidateValues(rule.SourceStableIds, true,
                "PresentationRuleSourcesInvalid");
            ValidateValues(rule.Limitations, true,
                "PresentationRuleLimitationsInvalid", requireStableId: false);
            if (rule.MutatesCanonicalState)
                throw new InvalidOperationException("PresentationRuleCanonicalMutationForbidden");
            if (rule.ConfirmsBusinessCompletion)
                throw new InvalidOperationException("PresentationRuleBusinessCompletionForbidden");
        }

        private static HashSet<string> Set(params string[] values)
            => new HashSet<string>(values, StringComparer.Ordinal);

        private static void ValidateValues(
            string[] values,
            bool requireAny,
            string errorCode,
            bool requireStableId = true)
        {
            if (values == null || (requireAny && values.Length == 0)
                || values.Any(string.IsNullOrWhiteSpace)
                || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException(errorCode);
            if (requireStableId)
                foreach (var value in values) RequireId(value, errorCode);
        }

        private static void RequireId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new InvalidOperationException(errorCode);
        }
    }

    public sealed class 표현규칙Catalog
    {
        private readonly 표현규칙CatalogSnapshot snapshot;

        public 표현규칙Catalog(표현규칙CatalogSnapshot value)
        {
            new 표현규칙Validator().Validate(value);
            snapshot = value;
        }

        public 표현규칙Descriptor ResolveLegacyVisualRule(string visualRuleRevision)
        {
            if (string.IsNullOrWhiteSpace(visualRuleRevision))
                throw new ArgumentException("Visual rule revision is required.", nameof(visualRuleRevision));
            return snapshot.Rules.SingleOrDefault(value =>
                    value.LegacyVisualRuleRevision == visualRuleRevision.Trim())
                ?? throw new KeyNotFoundException("PresentationRuleNotFound:" + visualRuleRevision.Trim());
        }

        public 표현규칙Descriptor[] RulesForDomain(string domainCode)
            => snapshot.Rules.Where(value => value.DomainCode == domainCode)
                .OrderBy(value => value.RuleStableId, StringComparer.Ordinal).ToArray();
    }

    public static class 통합표현규칙CatalogFixture
    {
        public static 표현규칙CatalogSnapshot Create()
            => new 표현규칙CatalogSnapshot
            {
                CatalogStableId = "presentation-rule-catalog:integrated-seedbed.v1",
                Revision = 1,
                Rules = new[]
                {
                    Existing("presentation-rule:graphics.community-square.v1",
                        표현규칙영역Codes.Graphics, "community-square-visual-v1",
                        "community-square-presentation-v1", 표현출력채널Codes.Material,
                        표현출력채널Codes.Color),
                    Existing("presentation-rule:graphics.public-marker.v1",
                        표현규칙영역Codes.Graphics, "public-marker-visual-v1",
                        "public-world-map-presentation-v1", 표현출력채널Codes.Material,
                        표현출력채널Codes.Color, 표현출력채널Codes.FX),
                    Existing("presentation-rule:graphics.public-data-surface.v1",
                        표현규칙영역Codes.Graphics, "public-data-surface-visual-v1",
                        "public-data-surface-presentation-v1", 표현출력채널Codes.Material,
                        표현출력채널Codes.Color),
                    Existing("presentation-rule:graphics.warehouse.v1",
                        표현규칙영역Codes.Graphics, "warehouse-primitive-visual-v1",
                        "warehouse-presentation-v1", 표현출력채널Codes.Material,
                        표현출력채널Codes.Color),
                    Existing("presentation-rule:graphics.urban-market-manager.v2",
                        표현규칙영역Codes.Graphics, "urban-market-manager-primitive-visual.v2",
                        "urban-market-manager-presentation.v2", 표현출력채널Codes.Material,
                        표현출력채널Codes.Color),
                    Existing("presentation-rule:ui.role-emphasis.v1",
                        표현규칙영역Codes.UI, "role-emphasis-visual-v1",
                        "role-presentation-v1", 표현출력채널Codes.Label,
                        표현출력채널Codes.Panel, 표현출력채널Codes.Badge),
                    Existing("presentation-rule:ui.concept-card.v1",
                        표현규칙영역Codes.UI, "concept-card-visual-v1",
                        "concept-card-presentation-v1", 표현출력채널Codes.Label,
                        표현출력채널Codes.Icon, 표현출력채널Codes.Panel),
                    Existing("presentation-rule:animation.npc-movement.v1",
                        표현규칙영역Codes.Animation, "npc-movement-visual-v1",
                        "npc-movement-presentation-v1", 표현출력채널Codes.MovementPlayback,
                        표현출력채널Codes.PlaybackSpeed),
                    Existing("presentation-rule:animation.transport-corridor.v1",
                        표현규칙영역Codes.Animation, "transport-corridor-visual-v1",
                        "transport-corridor-presentation-v1", 표현출력채널Codes.MovementPlayback,
                        표현출력채널Codes.PlaybackSpeed),
                    Prepared("presentation-rule:camera.object-focus.v1",
                        표현규칙영역Codes.Camera, 표현출력채널Codes.FocusTarget,
                        표현출력채널Codes.Distance, 표현출력채널Codes.Framing,
                        표현출력채널Codes.Transition),
                    Prepared("presentation-rule:lighting.world-time.v1",
                        표현규칙영역Codes.Lighting, 표현출력채널Codes.Light,
                        표현출력채널Codes.Ambient, 표현출력채널Codes.Fog,
                        표현출력채널Codes.TimeOfDayVisual),
                    Prepared("presentation-rule:audio.spatial-state-cue.v1",
                        표현규칙영역Codes.Audio, 표현출력채널Codes.AudioCue,
                        표현출력채널Codes.Volume, 표현출력채널Codes.SpatialBlend),
                },
            };

        private static 표현규칙Descriptor Existing(
            string stableId,
            string domain,
            string legacyRevision,
            string contractVersion,
            params string[] outputs)
            => Descriptor(stableId, domain, 표현규칙구현상태Codes.ExistingRuleMapped,
                legacyRevision, contractVersion, outputs);

        private static 표현규칙Descriptor Prepared(
            string stableId,
            string domain,
            params string[] outputs)
            => Descriptor(stableId, domain, 표현규칙구현상태Codes.ContractPrepared,
                null, "presentation-rule-contract.v1", outputs);

        private static 표현규칙Descriptor Descriptor(
            string stableId,
            string domain,
            string implementation,
            string? legacyRevision,
            string contractVersion,
            string[] outputs)
            => new 표현규칙Descriptor
            {
                RuleStableId = stableId,
                Revision = 1,
                DomainCode = domain,
                ImplementationStateCode = implementation,
                LegacyVisualRuleRevision = legacyRevision,
                PresentationContractVersion = contractVersion,
                InputPresentationStateCodes = new[] { "AuthorizedPresentationModel" },
                OutputChannelCodes = outputs,
                AppliesToVisualKeys = new[] { "visual-key:integrated-seedbed" },
                MutatesCanonicalState = false,
                ConfirmsBusinessCompletion = false,
                SourceStableIds = legacyRevision == null
                    ? new[] { "source:presentation-rule-classification" }
                    : new[] { "source:legacy-visual-rule", "visual-rule:" + legacyRevision },
                Limitations = new[]
                {
                    "서버·Simulation 기준 원장을 변경하지 않는다.",
                    "표현 완료를 업무 완료 근거로 사용하지 않는다.",
                },
            };
    }
}
