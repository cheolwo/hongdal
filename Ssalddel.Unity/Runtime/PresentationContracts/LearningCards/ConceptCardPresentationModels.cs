using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.PresentationContracts.LearningCards
{
    public static class ConceptCardPresentationVersions
    {
        public const string Contract = "concept-card-presentation-v1";
        public const string VisualRule = "concept-card-visual-v1";
    }

    public static class ConceptCardKindCodes
    {
        public const string Concept = "Concept";
        public const string Status = "Status";
        public const string Reason = "Reason";
        public const string Action = "Action";

        internal static bool IsKnown(string value)
            => value == Concept || value == Status || value == Reason || value == Action;
    }

    public static class ConceptCardCalculationRoleCodes
    {
        public const string Input = "Input";
        public const string Adjustment = "Adjustment";
        public const string Result = "Result";
        public const string Limitation = "Limitation";

        internal static bool IsKnown(string value)
            => value == Input || value == Adjustment || value == Result || value == Limitation;
    }

    public sealed class ConceptCardSourceLineageItem
    {
        public string SourceStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceAsOfUtc { get; set; }
        public string QualityCode { get; set; } = DataQualityCodes.Observed;
    }

    public sealed class ConceptCardEvidenceRow
    {
        public string LabelText { get; set; } = string.Empty;
        public string ValueText { get; set; } = string.Empty;
        public string CalculationRoleCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class ConceptCardActionItem
    {
        public string IntentCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class ConceptCardPresentationModel
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public string CardKindCode { get; set; } = string.Empty;
        public string ConceptStableId { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string PrimaryValueText { get; set; } = string.Empty;
        public string SimulationLabel { get; set; } = string.Empty;
        public ConceptCardEvidenceRow[] EvidenceRows { get; set; } = Array.Empty<ConceptCardEvidenceRow>();
        public string[] Cautions { get; set; } = Array.Empty<string>();
        public string[] RelatedConceptStableIds { get; set; } = Array.Empty<string>();
        public ConceptCardActionItem[] ActionItems { get; set; } = Array.Empty<ConceptCardActionItem>();
        public ConceptCardSourceLineageItem[] SourceLineage { get; set; } = Array.Empty<ConceptCardSourceLineageItem>();
    }

    public sealed class ConceptCardDeckPresentationModel
    {
        public PresentationStableId DeckStableId { get; set; }
        public WorldObjectRef AnchorWorldObjectRef { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string IntentCode { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public long SourceRevision { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public PresentationStableId? SelectedCardStableId { get; set; }
        public ConceptCardPresentationModel[] Cards { get; set; } = Array.Empty<ConceptCardPresentationModel>();
    }

    public sealed class ConceptCardEvidenceDraft
    {
        public string LabelText { get; set; } = string.Empty;
        public string ValueText { get; set; } = string.Empty;
        public string CalculationRoleCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class ConceptCardActionDraft
    {
        public string IntentCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class ConceptCardDraft
    {
        public int Sequence { get; set; }
        public string StableId { get; set; } = string.Empty;
        public string CardKindCode { get; set; } = string.Empty;
        public string ConceptStableId { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string PrimaryValueText { get; set; } = string.Empty;
        public string SimulationLabel { get; set; } = string.Empty;
        public WorldStableId[] SourceWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public ConceptCardEvidenceDraft[] EvidenceRows { get; set; } = Array.Empty<ConceptCardEvidenceDraft>();
        public string[] Cautions { get; set; } = Array.Empty<string>();
        public string[] RelatedConceptStableIds { get; set; } = Array.Empty<string>();
        public ConceptCardActionDraft[] ActionItems { get; set; } = Array.Empty<ConceptCardActionDraft>();
        public ConceptCardSourceLineageItem[] SourceLineage { get; set; } = Array.Empty<ConceptCardSourceLineageItem>();
    }

    public sealed class ConceptCardDeckProjectionInput
    {
        public string DeckStableId { get; set; } = string.Empty;
        public WorldObjectRef AnchorWorldObjectRef { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string IntentCode { get; set; } = string.Empty;
        public DataRuntimeMode Mode { get; set; }
        public long SourceRevision { get; set; }
        public string InterpretationRevision { get; set; } = string.Empty;
        public string VisualRuleRevision { get; set; } = ConceptCardPresentationVersions.VisualRule;
        public string SelectedCardStableId { get; set; } = string.Empty;
        public bool IsRoleAuthorized { get; set; }
        public string[] AuthorizedIntentCodes { get; set; } = Array.Empty<string>();
        public ConceptCardDraft[] Cards { get; set; } = Array.Empty<ConceptCardDraft>();
    }

    /// <summary>
    /// Perspective가 결정한 의미를 공통 카드 계약으로 정규화합니다.
    /// 권한을 만들거나 업무 값을 다시 계산하지 않습니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class ConceptCardDeckProjector
    {
        public ConceptCardDeckPresentationModel? Project(ConceptCardDeckProjectionInput source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.IsRoleAuthorized) return null;

            StableDataId.EnsureValid(source.DeckStableId, nameof(source.DeckStableId));
            EnsureAnchor(source.AnchorWorldObjectRef);
            var roleCode = Require(source.RoleCode, "ConceptCardRoleCodeMissing");
            var intentCode = Require(source.IntentCode, "ConceptCardIntentCodeMissing");
            if (source.SourceRevision < 0)
                throw new InvalidOperationException("ConceptCardSourceRevisionInvalid");

            var interpretationRevision = Require(
                source.InterpretationRevision,
                "ConceptCardInterpretationRevisionMissing");
            var visualRuleRevision = Require(
                source.VisualRuleRevision,
                "ConceptCardVisualRuleRevisionMissing");
            var allowedIntents = NormalizeCodes(source.AuthorizedIntentCodes);
            var drafts = source.Cards ?? throw new InvalidOperationException("ConceptCardDraftsMissing");
            var duplicate = drafts
                .Where(value => value != null)
                .GroupBy(value => value.StableId?.Trim() ?? string.Empty, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("DuplicateConceptCardStableId:" + duplicate.Key);

            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                interpretationRevision,
                roleCode + ":" + intentCode + ":" + source.Mode,
                visualRuleRevision,
                ConceptCardPresentationVersions.Contract);
            var cards = drafts
                .Select(value => value ?? throw new InvalidOperationException("ConceptCardDraftMissing"))
                .OrderBy(value => value.Sequence)
                .ThenBy(value => value.StableId, StringComparer.Ordinal)
                .Select(value => ProjectCard(value, presentationRevision, allowedIntents))
                .Where(value => value != null)
                .Cast<ConceptCardPresentationModel>()
                .ToArray();

            if (cards.Length == 0)
                throw new InvalidOperationException("ConceptCardDeckEmpty");

            PresentationStableId? selected = null;
            if (!string.IsNullOrWhiteSpace(source.SelectedCardStableId))
            {
                StableDataId.EnsureValid(source.SelectedCardStableId, nameof(source.SelectedCardStableId));
                if (cards.Any(value => value.StableId.Value == source.SelectedCardStableId.Trim()))
                    selected = new PresentationStableId(source.SelectedCardStableId);
            }

            return new ConceptCardDeckPresentationModel
            {
                DeckStableId = new PresentationStableId(source.DeckStableId),
                AnchorWorldObjectRef = source.AnchorWorldObjectRef,
                RoleCode = roleCode,
                IntentCode = intentCode,
                ModeCode = source.Mode.ToString(),
                SourceRevision = source.SourceRevision,
                PresentationRevision = presentationRevision,
                SelectedCardStableId = selected,
                Cards = cards,
            };
        }

        private static ConceptCardPresentationModel? ProjectCard(
            ConceptCardDraft source,
            string deckRevision,
            ISet<string> allowedIntents)
        {
            StableDataId.EnsureValid(source.StableId, nameof(source.StableId));
            StableDataId.EnsureValid(source.ConceptStableId, nameof(source.ConceptStableId));
            if (!ConceptCardKindCodes.IsKnown(source.CardKindCode))
                throw new InvalidOperationException("ConceptCardKindInvalid:" + source.CardKindCode);
            if (source.Sequence < 0)
                throw new InvalidOperationException("ConceptCardSequenceInvalid:" + source.StableId);

            var presentationId = new PresentationStableId(source.StableId);
            var sourceWorldIds = NormalizeWorldIds(source.SourceWorldIds, source.StableId);
            var sourceLineage = NormalizeSourceLineage(source.SourceLineage, source.StableId);
            var evidenceRows = NormalizeEvidence(source.EvidenceRows, sourceLineage, source.StableId);
            var actionItems = NormalizeActions(source.ActionItems, allowedIntents, source.StableId);

            if (source.CardKindCode == ConceptCardKindCodes.Reason && evidenceRows.Length == 0)
                throw new InvalidOperationException("ConceptCardReasonEvidenceMissing:" + source.StableId);
            if (source.CardKindCode != ConceptCardKindCodes.Action && actionItems.Length > 0)
                throw new InvalidOperationException("ConceptCardActionKindMismatch:" + source.StableId);
            if (source.CardKindCode == ConceptCardKindCodes.Action && actionItems.Length == 0)
                return null;

            return new ConceptCardPresentationModel
            {
                StableId = presentationId,
                Identity = new PresentationIdentityLineage(presentationId, sourceWorldIds),
                PresentationRevision = deckRevision + ":" + source.StableId,
                CardKindCode = source.CardKindCode,
                ConceptStableId = source.ConceptStableId.Trim(),
                TitleText = Require(source.TitleText, "ConceptCardTitleMissing:" + source.StableId),
                SummaryText = Require(source.SummaryText, "ConceptCardSummaryMissing:" + source.StableId),
                PrimaryValueText = source.PrimaryValueText?.Trim() ?? string.Empty,
                SimulationLabel = source.SimulationLabel?.Trim() ?? string.Empty,
                EvidenceRows = evidenceRows,
                Cautions = NormalizeText(source.Cautions),
                RelatedConceptStableIds = NormalizeStableIds(source.RelatedConceptStableIds),
                ActionItems = actionItems,
                SourceLineage = sourceLineage,
            };
        }

        private static ConceptCardEvidenceRow[] NormalizeEvidence(
            IEnumerable<ConceptCardEvidenceDraft>? values,
            IReadOnlyCollection<ConceptCardSourceLineageItem> sourceLineage,
            string cardStableId)
        {
            var sources = sourceLineage
                .Select(value => value.SourceStableId)
                .ToHashSet(StringComparer.Ordinal);
            return (values ?? Array.Empty<ConceptCardEvidenceDraft>())
                .Select(value => value ?? throw new InvalidOperationException("ConceptCardEvidenceMissing:" + cardStableId))
                .Select(value =>
                {
                    if (!ConceptCardCalculationRoleCodes.IsKnown(value.CalculationRoleCode))
                        throw new InvalidOperationException("ConceptCardCalculationRoleInvalid:" + cardStableId);
                    var sourceStableId = value.SourceStableId?.Trim() ?? string.Empty;
                    if (sourceStableId.Length > 0)
                    {
                        StableDataId.EnsureValid(sourceStableId, nameof(value.SourceStableId));
                        if (!sources.Contains(sourceStableId))
                            throw new InvalidOperationException("ConceptCardEvidenceSourceMissing:" + sourceStableId);
                    }

                    return new ConceptCardEvidenceRow
                    {
                        LabelText = Require(value.LabelText, "ConceptCardEvidenceLabelMissing:" + cardStableId),
                        ValueText = Require(value.ValueText, "ConceptCardEvidenceValueMissing:" + cardStableId),
                        CalculationRoleCode = value.CalculationRoleCode,
                        SourceStableId = sourceStableId,
                        RuleRevision = value.RuleRevision?.Trim() ?? string.Empty,
                    };
                })
                .ToArray();
        }

        private static ConceptCardActionItem[] NormalizeActions(
            IEnumerable<ConceptCardActionDraft>? values,
            ISet<string> allowedIntents,
            string cardStableId)
        {
            var result = new List<ConceptCardActionItem>();
            foreach (var value in values ?? Array.Empty<ConceptCardActionDraft>())
            {
                if (value == null)
                    throw new InvalidOperationException("ConceptCardActionMissing:" + cardStableId);
                var intentCode = Require(value.IntentCode, "ConceptCardActionIntentMissing:" + cardStableId);
                if (!allowedIntents.Contains(intentCode)) continue;

                var blockers = NormalizeCodes(value.BlockReasonCodes).ToArray();
                if (value.IsAvailable && blockers.Length > 0)
                    throw new InvalidOperationException("ConceptCardAvailableActionBlocked:" + intentCode);
                if (!value.IsAvailable && blockers.Length == 0)
                    throw new InvalidOperationException("ConceptCardBlockedActionReasonMissing:" + intentCode);

                result.Add(new ConceptCardActionItem
                {
                    IntentCode = intentCode,
                    LabelText = Require(value.LabelText, "ConceptCardActionLabelMissing:" + intentCode),
                    EffectCode = Require(value.EffectCode, "ConceptCardActionEffectMissing:" + intentCode),
                    IsAvailable = value.IsAvailable,
                    BlockReasonCodes = blockers,
                });
            }

            var duplicate = result
                .GroupBy(value => value.IntentCode, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("DuplicateConceptCardActionIntent:" + duplicate.Key);
            return result.OrderBy(value => value.IntentCode, StringComparer.Ordinal).ToArray();
        }

        private static ConceptCardSourceLineageItem[] NormalizeSourceLineage(
            IEnumerable<ConceptCardSourceLineageItem>? values,
            string cardStableId)
        {
            var result = (values ?? Array.Empty<ConceptCardSourceLineageItem>())
                .Select(value => value ?? throw new InvalidOperationException("ConceptCardSourceLineageMissing:" + cardStableId))
                .Select(value =>
                {
                    StableDataId.EnsureValid(value.SourceStableId, nameof(value.SourceStableId));
                    return new ConceptCardSourceLineageItem
                    {
                        SourceStableId = value.SourceStableId.Trim(),
                        Revision = Require(value.Revision, "ConceptCardSourceRevisionMissing:" + value.SourceStableId),
                        EvidenceAsOfUtc = value.EvidenceAsOfUtc,
                        QualityCode = Require(value.QualityCode, "ConceptCardSourceQualityMissing:" + value.SourceStableId),
                    };
                })
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ToArray();
            if (result.Length == 0)
                throw new InvalidOperationException("ConceptCardSourceLineageEmpty:" + cardStableId);
            var duplicate = result
                .GroupBy(value => value.SourceStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("DuplicateConceptCardSource:" + duplicate.Key);
            return result;
        }

        private static WorldStableId[] NormalizeWorldIds(
            IEnumerable<WorldStableId>? values,
            string cardStableId)
        {
            var result = (values ?? Array.Empty<WorldStableId>())
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            if (result.Length == 0 || result.Any(value => !value.IsDefined))
                throw new InvalidOperationException("ConceptCardSourceWorldMissing:" + cardStableId);
            return result;
        }

        private static string[] NormalizeStableIds(IEnumerable<string>? values)
        {
            var result = NormalizeText(values);
            foreach (var value in result)
                StableDataId.EnsureValid(value, nameof(values));
            return result;
        }

        private static HashSet<string> NormalizeCodes(IEnumerable<string>? values)
            => new HashSet<string>(NormalizeText(values), StringComparer.Ordinal);

        private static string[] NormalizeText(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static void EnsureAnchor(WorldObjectRef value)
        {
            if (string.IsNullOrWhiteSpace(value.WorldId.Value) || !value.ObjectId.IsDefined)
                throw new InvalidOperationException("ConceptCardAnchorMissing");
        }

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException(error)
                : value.Trim();
    }
}
