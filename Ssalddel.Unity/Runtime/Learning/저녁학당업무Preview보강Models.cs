using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Learning
{
    public static class 저녁학당업무Preview보강Versions
    {
        public const string Rule = "evening-business-preview-enrichment-v1";
    }

    public sealed class 업무Preview미확인사항
    {
        public string StableId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 업무PreviewMilestone
    {
        public string StableId { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string TitleText { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당업무Preview보강Input
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string BusinessStageCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] CanonicalSourceStableIds { get; set; } = Array.Empty<string>();
        public 플레이어내면상태Snapshot InnerState { get; set; } = new 플레이어내면상태Snapshot();
        public string FocusedRuleCode { get; set; } = string.Empty;
        public 업무Preview미확인사항[] Unknowns { get; set; } = Array.Empty<업무Preview미확인사항>();
        public 업무PreviewMilestone[] Milestones { get; set; } = Array.Empty<업무PreviewMilestone>();
    }

    public sealed class 저녁학당업무Preview보강
    {
        public string StableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string PreviewStableId { get; set; } = string.Empty;
        public long ExpectedDataRevision { get; set; }
        public string BusinessStageCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] CanonicalSourceStableIds { get; set; } = Array.Empty<string>();
        public string AppliedRuleCode { get; set; } = string.Empty;
        public 업무Preview미확인사항[] RevealedUnknowns { get; set; } = Array.Empty<업무Preview미확인사항>();
        public 업무PreviewMilestone[] MilestoneEvidence { get; set; } = Array.Empty<업무PreviewMilestone>();
        public bool MayMutateCanonicalState { get; set; }
        public bool MayChangeAllowedIntents { get; set; }
    }

    /// <summary>
    /// 저녁 학당에서 고른 한 규칙을 다음 날 업무 Preview의 정보 보강으로만 투영합니다.
    /// 원본 Preview의 revision, 상품, 수량, 단위, source lineage와 허용 intent는 변경하지 않습니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 저녁학당업무Preview보강Projector
    {
        public 저녁학당업무Preview보강 Project(저녁학당업무Preview보강Input input)
        {
            Validate(input);

            var focusedRule = input.FocusedRuleCode.Trim();
            if (focusedRule.Length > 0
                && !input.InnerState.ActiveRuleCodes.Contains(focusedRule, StringComparer.Ordinal))
                throw new InvalidOperationException("EveningBusinessPreviewFocusedRuleNotActive:" + focusedRule);
            if (focusedRule.Length > 0 && focusedRule != 내면규칙Codes.BeginnerMind
                && focusedRule != 내면규칙Codes.IntegratedProgress)
                throw new InvalidOperationException("EveningBusinessPreviewFocusedRuleUnknown:" + focusedRule);

            var unknowns = focusedRule == 내면규칙Codes.BeginnerMind
                ? input.Unknowns.Select(Clone).OrderBy(value => value.StableId, StringComparer.Ordinal).ToArray()
                : Array.Empty<업무Preview미확인사항>();
            var milestones = focusedRule == 내면규칙Codes.IntegratedProgress
                ? input.Milestones.Select(Clone).OrderBy(value => value.Sequence)
                    .ThenBy(value => value.StableId, StringComparer.Ordinal).ToArray()
                : Array.Empty<업무PreviewMilestone>();

            return new 저녁학당업무Preview보강
            {
                StableId = "evening-business-preview-enrichment:"
                    + StableSuffix(input.PreviewStableId) + ".r"
                    + input.ExpectedDataRevision.ToString(CultureInfo.InvariantCulture),
                RuleRevision = 저녁학당업무Preview보강Versions.Rule,
                PreviewStableId = input.PreviewStableId,
                ExpectedDataRevision = input.ExpectedDataRevision,
                BusinessStageCode = input.BusinessStageCode.Trim(),
                ProductStableId = input.ProductStableId,
                Quantity = input.Quantity,
                UnitCode = input.UnitCode.Trim(),
                CanonicalSourceStableIds = input.CanonicalSourceStableIds.ToArray(),
                AppliedRuleCode = focusedRule,
                RevealedUnknowns = unknowns,
                MilestoneEvidence = milestones,
                MayMutateCanonicalState = false,
                MayChangeAllowedIntents = false,
            };
        }

        private static void Validate(저녁학당업무Preview보강Input input)
        {
            if (input == null || !StableDataId.IsValid(input.PreviewStableId)
                || input.ExpectedDataRevision <= 0
                || string.IsNullOrWhiteSpace(input.BusinessStageCode)
                || !StableDataId.IsValid(input.ProductStableId)
                || input.Quantity <= 0 || string.IsNullOrWhiteSpace(input.UnitCode)
                || input.CanonicalSourceStableIds == null
                || input.CanonicalSourceStableIds.Length == 0
                || input.CanonicalSourceStableIds.Any(value => !StableDataId.IsValid(value))
                || input.CanonicalSourceStableIds.Distinct(StringComparer.Ordinal).Count()
                    != input.CanonicalSourceStableIds.Length
                || input.InnerState == null || input.InnerState.ActiveRuleCodes == null
                || input.InnerState.ActiveRuleCodes.Any(string.IsNullOrWhiteSpace)
                || input.InnerState.ActiveRuleCodes.Distinct(StringComparer.Ordinal).Count()
                    != input.InnerState.ActiveRuleCodes.Length
                || input.FocusedRuleCode == null
                || input.Unknowns == null || input.Milestones == null)
                throw new InvalidOperationException("EveningBusinessPreviewInputInvalid");

            foreach (var unknown in input.Unknowns) Validate(unknown);
            foreach (var milestone in input.Milestones) Validate(milestone);
            if (input.Unknowns.Select(value => value.StableId).Distinct(StringComparer.Ordinal).Count()
                != input.Unknowns.Length)
                throw new InvalidOperationException("EveningBusinessPreviewUnknownDuplicate");
            if (input.Milestones.Select(value => value.StableId).Distinct(StringComparer.Ordinal).Count()
                != input.Milestones.Length)
                throw new InvalidOperationException("EveningBusinessPreviewMilestoneDuplicate");
        }

        private static void Validate(업무Preview미확인사항 value)
        {
            if (value == null || !StableDataId.IsValid(value.StableId)
                || string.IsNullOrWhiteSpace(value.QuestionText)
                || string.IsNullOrWhiteSpace(value.ReasonText)
                || value.SourceStableIds == null || value.SourceStableIds.Length == 0
                || value.SourceStableIds.Any(source => !StableDataId.IsValid(source)))
                throw new InvalidOperationException("EveningBusinessPreviewUnknownInvalid");
        }

        private static void Validate(업무PreviewMilestone value)
        {
            if (value == null || !StableDataId.IsValid(value.StableId) || value.Sequence < 0
                || string.IsNullOrWhiteSpace(value.TitleText)
                || string.IsNullOrWhiteSpace(value.StateCode)
                || value.SourceStableIds == null || value.SourceStableIds.Length == 0
                || value.SourceStableIds.Any(source => !StableDataId.IsValid(source)))
                throw new InvalidOperationException("EveningBusinessPreviewMilestoneInvalid");
        }

        private static 업무Preview미확인사항 Clone(업무Preview미확인사항 value)
            => new 업무Preview미확인사항
            {
                StableId = value.StableId,
                QuestionText = value.QuestionText.Trim(),
                ReasonText = value.ReasonText.Trim(),
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static 업무PreviewMilestone Clone(업무PreviewMilestone value)
            => new 업무PreviewMilestone
            {
                StableId = value.StableId,
                Sequence = value.Sequence,
                TitleText = value.TitleText.Trim(),
                StateCode = value.StateCode.Trim(),
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static string StableSuffix(string stableId)
            => stableId.Substring(stableId.IndexOf(':') + 1).Replace(':', '.');
    }
}
