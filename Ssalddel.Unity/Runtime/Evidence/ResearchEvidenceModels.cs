using System;
using System.Collections.Generic;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Evidence
{
    public static class EvidenceConfidenceCodes
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string NotAssessed = "NotAssessed";
    }

    public sealed class 연구근거Card
    {
        public string EvidenceCardId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string[] SourceReferences { get; set; } = Array.Empty<string>();

        public string ResearchScope { get; set; } = string.Empty;

        public string SupportedClaim { get; set; } = string.Empty;

        public string ProductInterpretation { get; set; } = string.Empty;

        public string UnityVisualTranslation { get; set; } = string.Empty;

        public string[] Limitations { get; set; } = Array.Empty<string>();

        public string EvidenceVersion { get; set; } = string.Empty;

        public DateTimeOffset EffectiveAt { get; set; }
    }

    public sealed class ProjectionRule근거Reference
    {
        public string RuleKey { get; set; } = string.Empty;

        public string RuleVersion { get; set; } = string.Empty;

        public string[] EvidenceCardIds { get; set; } = Array.Empty<string>();

        public string ConfidenceCode { get; set; } = EvidenceConfidenceCodes.NotAssessed;

        public string LimitationSummary { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "Unity가 세계 의미·인과·현실 근거와 준비도 문맥을 표현한다.",
        Boundary = "표현된 근거와 Simulation 규칙 및 운영 사실을 분리한다.")]
    public sealed class 연구근거Validator
    {
        public string[] Validate(연구근거Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var errors = new List<string>();
            if (!StableDataId.IsValid(card.EvidenceCardId))
            {
                errors.Add("EvidenceCardIdInvalid");
            }

            Require(card.Title, "TitleMissing", errors);
            Require(card.ResearchScope, "ResearchScopeMissing", errors);
            Require(card.SupportedClaim, "SupportedClaimMissing", errors);
            Require(card.ProductInterpretation, "ProductInterpretationMissing", errors);
            Require(card.UnityVisualTranslation, "UnityVisualTranslationMissing", errors);
            Require(card.EvidenceVersion, "EvidenceVersionMissing", errors);

            if (card.SourceReferences.Length == 0)
            {
                errors.Add("SourceReferenceMissing");
            }

            if (card.Limitations.Length == 0)
            {
                errors.Add("LimitationMissing");
            }

            return errors.ToArray();
        }

        private static void Require(string value, string errorCode, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(errorCode);
            }
        }
    }
}
