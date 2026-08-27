using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "승인된 상품 특성을 revision 고정 규칙으로 게임용 효과 정의에 결정적으로 변환한다.",
        StepKey = "domain.marketplace-grounded-item-effect-derive",
        DependsOnStepKeys = new[] { "contract.marketplace-grounded-interior-item" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 20,
        Boundary = "상품명 문자열이나 실시간 Marketplace 상태로 효과를 임의 생성하지 않고 Simulation 상태를 변경하지 않는다.")]
    public sealed class 상품특성효과DerivationEngine : I상품특성효과DerivationEngine
    {
        public 상품근거ItemDefinition Derive(상품근거ItemDerivationRequest request)
        {
            Validate(request);
            var profile = request.TraitProfile;
            var ruleSet = request.EffectRuleSet;
            var profileHash = 상품근거ItemHash.ComputeTraitProfileHash(profile);
            var ruleSetHash = 상품근거ItemHash.ComputeRuleSetHash(ruleSet);
            RequireMatchingHash(profile.ProfileHashSha256, profileHash, "Trait profile hash");
            RequireMatchingHash(ruleSet.RuleSetHashSha256, ruleSetHash, "Effect rule set hash");

            var traits = (profile.Traits ?? Array.Empty<상품근거특성Value>())
                .OrderBy(value => value.TraitCode, StringComparer.Ordinal)
                .ThenBy(value => value.ValueCode, StringComparer.Ordinal)
                .Select(Clone)
                .ToArray();
            var traitMap = traits.ToDictionary(value => value.TraitCode, StringComparer.Ordinal);
            var effects = (ruleSet.Rules ?? Array.Empty<상품특성효과Rule>())
                .Where(rule => string.Equals(rule.CategoryCode, profile.CategoryCode, StringComparison.Ordinal)
                               && traitMap.TryGetValue(rule.TraitCode, out var trait)
                               && string.Equals(trait.ValueCode, rule.TraitValueCode, StringComparison.Ordinal))
                .OrderBy(rule => rule.StableId, StringComparer.Ordinal)
                .GroupBy(rule => new
                {
                    rule.EffectCode,
                    rule.UnitCode,
                    rule.DisplayName,
                })
                .Select(group => new 실내ItemEffect
                {
                    EffectCode = group.Key.EffectCode.Trim(),
                    Magnitude = group.Sum(value => value.Magnitude),
                    UnitCode = group.Key.UnitCode.Trim(),
                    DisplayName = group.Key.DisplayName.Trim(),
                    BasisTraitCodes = group.Select(value => value.TraitCode.Trim())
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                })
                .OrderBy(value => value.EffectCode, StringComparer.Ordinal)
                .ThenBy(value => value.UnitCode, StringComparer.Ordinal)
                .ToArray();

            var definition = new 상품근거ItemDefinition
            {
                StableId = request.ItemDefinitionStableId.Trim(),
                ReferenceStableId = profile.ReferenceStableId.Trim(),
                CategoryCode = profile.CategoryCode.Trim(),
                VisualKey = request.VisualKey.Trim(),
                ReferenceCatalogRevision = request.ReferenceCatalog.Revision.Trim(),
                ReferenceCatalogHashSha256 = request.ReferenceCatalog.CatalogHashSha256.Trim(),
                TraitProfileRevision = profile.ProfileRevision.Trim(),
                TraitProfileHashSha256 = profileHash,
                EffectRuleRevision = ruleSet.Revision.Trim(),
                EffectRuleHashSha256 = ruleSetHash,
                Traits = traits,
                Effects = effects,
                ActivationStateCode = 상품근거ItemCodes.DefinitionOnly,
                EffectAuthorityCode = 상품근거ItemCodes.SimulationCoreRequired,
            };
            definition.DefinitionHashSha256 = 상품근거ItemHash.ComputeDefinitionHash(definition);
            return definition;
        }

        private static void Validate(상품근거ItemDerivationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            Require(request.ItemDefinitionStableId, "ItemDefinitionStableId");
            Require(request.VisualKey, "VisualKey");
            var catalog = request.ReferenceCatalog
                          ?? throw new ArgumentException("ReferenceCatalog가 필요합니다.", nameof(request));
            var profile = request.TraitProfile
                          ?? throw new ArgumentException("TraitProfile이 필요합니다.", nameof(request));
            var ruleSet = request.EffectRuleSet
                          ?? throw new ArgumentException("EffectRuleSet이 필요합니다.", nameof(request));
            Require(catalog.Revision, "ReferenceCatalog.Revision");
            Require(catalog.CatalogHashSha256, "ReferenceCatalog.CatalogHashSha256");
            Require(profile.StableId, "TraitProfile.StableId");
            Require(profile.ReferenceStableId, "TraitProfile.ReferenceStableId");
            Require(profile.CategoryCode, "TraitProfile.CategoryCode");
            Require(profile.ProfileRevision, "TraitProfile.ProfileRevision");
            Require(profile.ApprovalRevision, "TraitProfile.ApprovalRevision");
            Require(ruleSet.StableId, "EffectRuleSet.StableId");
            Require(ruleSet.Revision, "EffectRuleSet.Revision");

            var expectedCatalogHash = InteriorLayoutHash.ComputeCatalogHash(catalog);
            if (!string.Equals(expectedCatalogHash, catalog.CatalogHashSha256, StringComparison.Ordinal))
                throw new ArgumentException("ReferenceCatalog hash가 현재 내용과 일치하지 않습니다.", nameof(request));

            var reference = (catalog.Items ?? Array.Empty<ApprovedInteriorReference>()).SingleOrDefault(value => string.Equals(
                value.ReferenceStableId,
                profile.ReferenceStableId,
                StringComparison.Ordinal));
            if (reference is null)
                throw new ArgumentException("승인 Catalog에 TraitProfile의 Reference가 없습니다.", nameof(request));
            if (!string.Equals(reference.CategoryCode, profile.CategoryCode, StringComparison.Ordinal))
                throw new ArgumentException("승인 Reference와 TraitProfile의 Category가 다릅니다.", nameof(request));

            var traits = profile.Traits ?? Array.Empty<상품근거특성Value>();
            var duplicateTrait = traits
                .Where(value => value is not null)
                .GroupBy(value => value.TraitCode?.Trim(), StringComparer.Ordinal)
                .FirstOrDefault(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1);
            if (duplicateTrait is not null)
                throw new ArgumentException("TraitCode는 Profile 안에서 하나의 승인 값만 가질 수 있습니다.", nameof(request));
            foreach (var trait in traits)
            {
                if (trait is null) throw new ArgumentException("Trait 값은 null일 수 없습니다.", nameof(request));
                Require(trait.TraitCode, "Trait.TraitCode");
                Require(trait.ValueCode, "Trait.ValueCode");
                Require(trait.UnitCode, "Trait.UnitCode");
                Require(trait.DisplayName, "Trait.DisplayName");
                Require(trait.EvidenceSummary, "Trait.EvidenceSummary");
                if (trait.NumericValue.HasValue && (double.IsNaN(trait.NumericValue.Value)
                                                     || double.IsInfinity(trait.NumericValue.Value)))
                    throw new ArgumentException("Trait NumericValue는 유한한 값이어야 합니다.", nameof(request));
            }

            var rules = ruleSet.Rules ?? Array.Empty<상품특성효과Rule>();
            var duplicateRule = rules
                .Where(value => value is not null)
                .GroupBy(value => value.StableId?.Trim(), StringComparer.Ordinal)
                .FirstOrDefault(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1);
            if (duplicateRule is not null)
                throw new ArgumentException("Effect Rule StableId는 중복될 수 없습니다.", nameof(request));
            foreach (var rule in rules)
            {
                if (rule is null) throw new ArgumentException("Effect Rule은 null일 수 없습니다.", nameof(request));
                Require(rule.StableId, "EffectRule.StableId");
                Require(rule.CategoryCode, "EffectRule.CategoryCode");
                Require(rule.TraitCode, "EffectRule.TraitCode");
                Require(rule.TraitValueCode, "EffectRule.TraitValueCode");
                Require(rule.EffectCode, "EffectRule.EffectCode");
                Require(rule.UnitCode, "EffectRule.UnitCode");
                Require(rule.DisplayName, "EffectRule.DisplayName");
                if (double.IsNaN(rule.Magnitude) || double.IsInfinity(rule.Magnitude))
                    throw new ArgumentException("Effect Rule Magnitude는 유한한 값이어야 합니다.", nameof(request));
            }
        }

        private static 상품근거특성Value Clone(상품근거특성Value source)
            => new 상품근거특성Value
            {
                TraitCode = source.TraitCode.Trim(),
                ValueCode = source.ValueCode.Trim(),
                NumericValue = source.NumericValue,
                UnitCode = source.UnitCode.Trim(),
                DisplayName = source.DisplayName.Trim(),
                EvidenceSummary = source.EvidenceSummary.Trim(),
            };

        private static void RequireMatchingHash(string supplied, string expected, string name)
        {
            if (!string.IsNullOrWhiteSpace(supplied)
                && !string.Equals(supplied.Trim(), expected, StringComparison.Ordinal))
                throw new ArgumentException(name + "가 현재 내용과 일치하지 않습니다.");
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " 값이 필요합니다.");
        }
    }

    public static class 상품근거ItemHash
    {
        public static string ComputeTraitProfileHash(상품근거특성Profile profile)
        {
            if (profile is null) throw new ArgumentNullException(nameof(profile));
            return Hash(string.Join("\n", new[]
            {
                profile.StableId?.Trim() ?? string.Empty,
                profile.ReferenceStableId?.Trim() ?? string.Empty,
                profile.CategoryCode?.Trim() ?? string.Empty,
                profile.ProfileRevision?.Trim() ?? string.Empty,
                profile.ApprovalRevision?.Trim() ?? string.Empty,
                string.Join("\n", (profile.Traits ?? Array.Empty<상품근거특성Value>())
                    .Where(value => value is not null)
                    .OrderBy(value => value.TraitCode, StringComparer.Ordinal)
                    .ThenBy(value => value.ValueCode, StringComparer.Ordinal)
                    .Select(CanonicalTrait)),
            }));
        }

        public static string ComputeRuleSetHash(상품특성효과RuleSet ruleSet)
        {
            if (ruleSet is null) throw new ArgumentNullException(nameof(ruleSet));
            return Hash(string.Join("\n", new[]
            {
                ruleSet.StableId?.Trim() ?? string.Empty,
                ruleSet.Revision?.Trim() ?? string.Empty,
                string.Join("\n", (ruleSet.Rules ?? Array.Empty<상품특성효과Rule>())
                    .Where(value => value is not null)
                    .OrderBy(value => value.StableId, StringComparer.Ordinal)
                    .Select(rule => string.Join("|", new[]
                    {
                        rule.StableId?.Trim() ?? string.Empty,
                        rule.CategoryCode?.Trim() ?? string.Empty,
                        rule.TraitCode?.Trim() ?? string.Empty,
                        rule.TraitValueCode?.Trim() ?? string.Empty,
                        rule.EffectCode?.Trim() ?? string.Empty,
                        rule.Magnitude.ToString("R", CultureInfo.InvariantCulture),
                        rule.UnitCode?.Trim() ?? string.Empty,
                        rule.DisplayName?.Trim() ?? string.Empty,
                    }))),
            }));
        }

        public static string ComputeDefinitionHash(상품근거ItemDefinition definition)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));
            return Hash(string.Join("\n", new[]
            {
                definition.SchemaVersion?.Trim() ?? string.Empty,
                definition.StableId?.Trim() ?? string.Empty,
                definition.ReferenceStableId?.Trim() ?? string.Empty,
                definition.CategoryCode?.Trim() ?? string.Empty,
                definition.VisualKey?.Trim() ?? string.Empty,
                definition.ReferenceCatalogRevision?.Trim() ?? string.Empty,
                definition.ReferenceCatalogHashSha256?.Trim() ?? string.Empty,
                definition.TraitProfileRevision?.Trim() ?? string.Empty,
                definition.TraitProfileHashSha256?.Trim() ?? string.Empty,
                definition.EffectRuleRevision?.Trim() ?? string.Empty,
                definition.EffectRuleHashSha256?.Trim() ?? string.Empty,
                definition.ActivationStateCode?.Trim() ?? string.Empty,
                definition.EffectAuthorityCode?.Trim() ?? string.Empty,
                string.Join("\n", (definition.Traits ?? Array.Empty<상품근거특성Value>())
                    .OrderBy(value => value.TraitCode, StringComparer.Ordinal)
                    .ThenBy(value => value.ValueCode, StringComparer.Ordinal)
                    .Select(CanonicalTrait)),
                string.Join("\n", (definition.Effects ?? Array.Empty<실내ItemEffect>())
                    .OrderBy(value => value.EffectCode, StringComparer.Ordinal)
                    .ThenBy(value => value.UnitCode, StringComparer.Ordinal)
                    .Select(value => string.Join("|", new[]
                    {
                        value.EffectCode?.Trim() ?? string.Empty,
                        value.Magnitude.ToString("R", CultureInfo.InvariantCulture),
                        value.UnitCode?.Trim() ?? string.Empty,
                        value.DisplayName?.Trim() ?? string.Empty,
                        string.Join(",", (value.BasisTraitCodes ?? Array.Empty<string>())
                            .OrderBy(item => item, StringComparer.Ordinal)),
                    }))),
            }));
        }

        private static string CanonicalTrait(상품근거특성Value value)
            => string.Join("|", new[]
            {
                value.TraitCode?.Trim() ?? string.Empty,
                value.ValueCode?.Trim() ?? string.Empty,
                value.NumericValue.HasValue
                    ? value.NumericValue.Value.ToString("R", CultureInfo.InvariantCulture)
                    : string.Empty,
                value.UnitCode?.Trim() ?? string.Empty,
                value.DisplayName?.Trim() ?? string.Empty,
                value.EvidenceSummary?.Trim() ?? string.Empty,
            });

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var valueByte in bytes)
                builder.Append(valueByte.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }
    }
}
