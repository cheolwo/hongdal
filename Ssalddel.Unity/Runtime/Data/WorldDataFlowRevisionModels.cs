using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.Data
{
    public static class DataQualityCodes
    {
        public const string Observed = "Observed";
        public const string Cached = "Cached";
        public const string Stale = "Stale";
        public const string Missing = "Missing";
        public const string Suppressed = "Suppressed";
        public const string Estimated = "Estimated";
        public const string Noisy = "Noisy";
        public const string Invalid = "Invalid";
    }

    public static class InterpretationLimitationCodes
    {
        public const string MissingSource = "MissingSource";
        public const string NotComparable = "NotComparable";
        public const string SuppressedInput = "SuppressedInput";
        public const string StaleInput = "StaleInput";
    }

    public sealed class DataRevisionReference
    {
        public DataRevisionReference(
            string sourceStableId,
            string revision,
            DateTimeOffset? evidenceAsOfUtc = null,
            string qualityCode = DataQualityCodes.Observed)
        {
            SourceStableId = Require(sourceStableId, nameof(sourceStableId));
            Revision = Require(revision, nameof(revision));
            EvidenceAsOfUtc = evidenceAsOfUtc;
            QualityCode = Require(qualityCode, nameof(qualityCode));
        }

        public string SourceStableId { get; }
        public string Revision { get; }
        public DateTimeOffset? EvidenceAsOfUtc { get; }
        public string QualityCode { get; }

        private static string Require(string value, string parameterName)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", parameterName)
                : value.Trim();
    }

    public sealed class DataRevisionSet
    {
        public DataRevisionSet(IEnumerable<DataRevisionReference> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            var ordered = values
                .Select(value => value ?? throw new InvalidOperationException("DataRevisionReferenceMissing"))
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ThenBy(value => value.Revision, StringComparer.Ordinal)
                .ToArray();

            if (ordered.Length == 0)
                throw new InvalidOperationException("DataRevisionSetEmpty");

            var duplicate = ordered
                .GroupBy(value => value.SourceStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("DuplicateDataRevisionSource:" + duplicate.Key);

            Items = ordered;
        }

        public DataRevisionReference[] Items { get; }
    }

    public sealed class InterpretationLineage
    {
        public InterpretationLineage(
            DataRevisionSet inputs,
            string interpreterContractVersion,
            string ruleSetRevision,
            string interpretationRevision,
            IEnumerable<string>? evidenceCardIds = null,
            IEnumerable<string>? limitationCodes = null)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            InterpreterContractVersion = Require(interpreterContractVersion, nameof(interpreterContractVersion));
            RuleSetRevision = Require(ruleSetRevision, nameof(ruleSetRevision));
            InterpretationRevision = Require(interpretationRevision, nameof(interpretationRevision));
            EvidenceCardIds = Normalize(evidenceCardIds);
            LimitationCodes = Normalize(limitationCodes);
        }

        public DataRevisionSet Inputs { get; }
        public string InterpreterContractVersion { get; }
        public string RuleSetRevision { get; }
        public string InterpretationRevision { get; }
        public string[] EvidenceCardIds { get; }
        public string[] LimitationCodes { get; }

        private static string[] Normalize(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static string Require(string value, string parameterName)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", parameterName)
                : value.Trim();
    }

    public sealed class PresentationRevisionReference
    {
        public PresentationRevisionReference(
            string interpretationRevision,
            string perspectiveCode,
            string visualRuleRevision,
            string presentationContractVersion,
            string presentationRevision)
        {
            InterpretationRevision = Require(interpretationRevision, nameof(interpretationRevision));
            PerspectiveCode = Require(perspectiveCode, nameof(perspectiveCode));
            VisualRuleRevision = Require(visualRuleRevision, nameof(visualRuleRevision));
            PresentationContractVersion = Require(presentationContractVersion, nameof(presentationContractVersion));
            PresentationRevision = Require(presentationRevision, nameof(presentationRevision));
        }

        public string InterpretationRevision { get; }
        public string PerspectiveCode { get; }
        public string VisualRuleRevision { get; }
        public string PresentationContractVersion { get; }
        public string PresentationRevision { get; }

        private static string Require(string value, string parameterName)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", parameterName)
                : value.Trim();
    }

    public static class WorldDataFlowRevisionCalculator
    {
        public static string CalculateInterpretation(
            DataRevisionSet inputs,
            string interpreterContractVersion,
            string ruleSetRevision,
            string normalizedParameters = "")
        {
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));

            var parts = inputs.Items
                .SelectMany(value => new[]
                {
                    value.SourceStableId,
                    value.Revision,
                    value.EvidenceAsOfUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
                    value.QualityCode,
                })
                .Concat(new[]
                {
                    Require(interpreterContractVersion, nameof(interpreterContractVersion)),
                    Require(ruleSetRevision, nameof(ruleSetRevision)),
                    normalizedParameters?.Trim() ?? string.Empty,
                });

            return "interpretation:" + Hash(parts);
        }

        public static string CalculatePresentation(
            string interpretationRevision,
            string perspectiveCode,
            string visualRuleRevision,
            string presentationContractVersion)
            => "presentation:" + Hash(new[]
            {
                Require(interpretationRevision, nameof(interpretationRevision)),
                Require(perspectiveCode, nameof(perspectiveCode)),
                Require(visualRuleRevision, nameof(visualRuleRevision)),
                Require(presentationContractVersion, nameof(presentationContractVersion)),
            });

        private static string Hash(IEnumerable<string> values)
        {
            var canonical = new StringBuilder();
            foreach (var value in values)
            {
                var normalized = value ?? string.Empty;
                canonical.Append(normalized.Length).Append(':').Append(normalized).Append('|');
            }

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
            return string.Concat(bytes.Select(value => value.ToString("x2")));
        }

        private static string Require(string value, string parameterName)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", parameterName)
                : value.Trim();
    }
}
