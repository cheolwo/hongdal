using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.Cards
{
    public static class 처방기록PresentationCodes
    {
        public const string RequiredHCapability = "ReadableKnowledgeSource";
        public const string OpenBookVisualKey =
            "Knowledge.Recipe.Record.OpenBook";
        public const string LoosePaperVisualKey =
            "Knowledge.Recipe.Record.LoosePaper";
        public const string FallbackVisualKey =
            "Primitive.ReadableKnowledgeSourceMarker";
        public const string InteractionAnchorCode =
            "InteractionAnchor.WI-ACTOR-03.Preview";
        public const string CandidateRevision =
            "recipe-knowledge-source-candidates.r1";
    }

    public sealed class 처방기록VisualBinding
    {
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; }
            = string.Empty;
    }

    public sealed class 처방기록SourcePresentation
    {
        public string PresentationStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string RequiredHCapability { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string InteractionAnchorCode { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; }
            = string.Empty;
        public bool CanOpenInformation { get; set; }
        public bool CanRequestPreview { get; set; }
        public bool CanConfirmAuthority { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class 처방기록PresentationPreparation
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long SourceRevision { get; set; }
        public string CandidateRevision { get; set; } = string.Empty;
        public 처방기록SourcePresentation[] Sources { get; set; }
            = Array.Empty<처방기록SourcePresentation>();
        public bool PresentationOnly { get; set; } = true;
        public bool MutatesCanonicalState { get; set; }
        public string PlanHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 같은 revision의 처방 지식 카드 상태를 물리 기록 후보의 VisualKey,
    /// H 능력과 Preview 상호작용 의도로 결속한다. 실제 좌표·Prefab·Collider를
    /// 만들지 않으며 Confirm과 권위 상태 변경도 수행하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "처방 기록의 판독 순간, H 능력, VisualKey와 fallback을 구현 준비 계획으로 결속한다.",
        WorkOrderIds = new[] { "E7-WO-NATURE-BASIC-HERBAL-RECOVERY" },
        Boundary = "E4 준비 계획은 실제 Prefab·Scene·Collider·입력·Game View 또는 지식 습득 Confirm 증거가 아니다.")]
    public sealed class 처방기록PresentationPreparationProjector
    {
        public 처방기록PresentationPreparation Project(
            처방지식CardFamilyProjection cardFamily,
            IEnumerable<처방기록VisualBinding> sourceBindings)
        {
            if (cardFamily == null)
                throw new ArgumentNullException(nameof(cardFamily));
            if (sourceBindings == null)
                throw new ArgumentNullException(nameof(sourceBindings));

            Validate(cardFamily);
            var bindings = sourceBindings.ToArray();
            Validate(bindings);
            var bindingBySource = bindings.ToDictionary(
                value => value.KnowledgeSourceStableId,
                StringComparer.Ordinal);

            var sources = cardFamily.Cards
                .SelectMany(card => card.KnowledgeSourceStableIds.Select(
                    sourceId => CreateSource(card, sourceId,
                        bindingBySource.TryGetValue(sourceId, out var binding)
                            ? binding
                            : null)))
                .OrderBy(value => value.KnowledgeSourceStableId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.RecipeStableId, StringComparer.Ordinal)
                .ToArray();

            var result = new 처방기록PresentationPreparation
            {
                PlanStableId = "presentation-plan:recipe-knowledge-source:"
                    + cardFamily.WorldStableId + ":" + cardFamily.PlayerStableId,
                WorldStableId = cardFamily.WorldStableId,
                PlayerStableId = cardFamily.PlayerStableId,
                SourceRevision = cardFamily.WorldRevision,
                CandidateRevision = 처방기록PresentationCodes.CandidateRevision,
                Sources = sources,
                PresentationOnly = true,
                MutatesCanonicalState = false,
            };
            result.PlanHashSha256 = ComputeHash(result);
            return result;
        }

        private static 처방기록SourcePresentation CreateSource(
            처방지식CardProjection card, string sourceId,
            처방기록VisualBinding? binding)
        {
            var readable = string.Equals(card.StateCode,
                처방지식CardStateCodes.Readable, StringComparison.Ordinal);
            var known = string.Equals(card.StateCode,
                처방지식CardStateCodes.Known, StringComparison.Ordinal);
            var visualKey = binding?.VisualKey
                ?? 처방기록PresentationCodes.FallbackVisualKey;
            var fallback = binding?.FallbackVisualKey
                ?? 처방기록PresentationCodes.FallbackVisualKey;
            var fingerprint = binding?.CandidateRevisionOrFingerprint
                ?? "fallback:readable-knowledge-source-marker.r1";

            return new 처방기록SourcePresentation
            {
                PresentationStableId = "presentation:recipe-source:"
                    + sourceId + ":" + card.RecipeStableId,
                RecipeStableId = card.RecipeStableId,
                KnowledgeSourceStableId = sourceId,
                StateCode = card.StateCode,
                RequiredHCapability =
                    처방기록PresentationCodes.RequiredHCapability,
                VisualKey = visualKey,
                FallbackVisualKey = fallback,
                InteractionAnchorCode =
                    처방기록PresentationCodes.InteractionAnchorCode,
                CandidateRevisionOrFingerprint = fingerprint,
                CanOpenInformation = readable || known,
                CanRequestPreview = readable,
                CanConfirmAuthority = false,
                PresentationOnly = true,
            };
        }

        private static void Validate(처방지식CardFamilyProjection value)
        {
            if (!value.PresentationOnly
                || string.IsNullOrWhiteSpace(value.WorldStableId)
                || string.IsNullOrWhiteSpace(value.PlayerStableId)
                || value.WorldRevision < 0
                || value.Cards == null
                || value.Cards.Any(card => card == null
                    || string.IsNullOrWhiteSpace(card.RecipeStableId)
                    || card.KnowledgeSourceStableIds == null
                    || card.KnowledgeSourceStableIds.Any(
                        string.IsNullOrWhiteSpace)
                    || card.KnowledgeSourceStableIds.Distinct(
                        StringComparer.Ordinal).Count()
                        != card.KnowledgeSourceStableIds.Length
                    || !new[]
                    {
                        처방지식CardStateCodes.Readable,
                        처방지식CardStateCodes.Known,
                        처방지식CardStateCodes.Blocked,
                    }.Contains(card.StateCode, StringComparer.Ordinal)))
                throw new InvalidOperationException(
                    "RecipeKnowledgePresentationSourceInvalid");
        }

        private static void Validate(처방기록VisualBinding[] values)
        {
            if (values.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.KnowledgeSourceStableId)
                    || string.IsNullOrWhiteSpace(value.VisualKey)
                    || string.IsNullOrWhiteSpace(value.FallbackVisualKey)
                    || (!string.Equals(value.VisualKey,
                            처방기록PresentationCodes.OpenBookVisualKey,
                            StringComparison.Ordinal)
                        && !string.Equals(value.VisualKey,
                            처방기록PresentationCodes.LoosePaperVisualKey,
                            StringComparison.Ordinal)
                        && !string.Equals(value.VisualKey,
                            처방기록PresentationCodes.FallbackVisualKey,
                            StringComparison.Ordinal))
                    || !string.Equals(value.FallbackVisualKey,
                        처방기록PresentationCodes.FallbackVisualKey,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(
                        value.CandidateRevisionOrFingerprint))
                || values.Select(value => value.KnowledgeSourceStableId)
                    .Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException(
                    "RecipeKnowledgeVisualBindingInvalid");
        }

        private static string ComputeHash(
            처방기록PresentationPreparation value)
        {
            var text = new StringBuilder()
                .Append(value.PlanStableId).Append('|')
                .Append(value.WorldStableId).Append('|')
                .Append(value.PlayerStableId).Append('|')
                .Append(value.SourceRevision).Append('|')
                .Append(value.CandidateRevision).Append('|')
                .Append(value.PresentationOnly).Append('|')
                .Append(value.MutatesCanonicalState);
            foreach (var source in value.Sources)
                text.Append('\n').Append(source.PresentationStableId).Append('|')
                    .Append(source.RecipeStableId).Append('|')
                    .Append(source.KnowledgeSourceStableId).Append('|')
                    .Append(source.StateCode).Append('|')
                    .Append(source.RequiredHCapability).Append('|')
                    .Append(source.VisualKey).Append('|')
                    .Append(source.FallbackVisualKey).Append('|')
                    .Append(source.InteractionAnchorCode).Append('|')
                    .Append(source.CandidateRevisionOrFingerprint).Append('|')
                    .Append(source.CanOpenInformation).Append('|')
                    .Append(source.CanRequestPreview).Append('|')
                    .Append(source.CanConfirmAuthority).Append('|')
                    .Append(source.PresentationOnly);
            using var sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    text.ToString())).Select(value => value.ToString("x2")));
        }
    }
}
