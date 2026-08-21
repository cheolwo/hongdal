using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationRealityContextSnapshot? realityContext;

        private void InitializeRealityContext(SimulationRealityContextSnapshot? snapshot)
        {
            var profileSelected = !string.IsNullOrWhiteSpace(RealityContextProfileStableId);
            if (profileSelected != (snapshot != null))
                throw new SimulationContractException("SimulationRealityContextAuthorityRequired");
            if (snapshot == null) return;
            ValidateRealityContext(snapshot);
            if (!string.Equals(snapshot.ProfileStableId, RealityContextProfileStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException("SimulationRealityContextProfileMismatch");
            realityContext = CloneRealityContext(snapshot);
        }

        public SimulationRealityContextSnapshot? RealityContextSnapshot()
        {
            lock (gate)
            {
                return realityContext == null ? null : CloneRealityContext(realityContext);
            }
        }

        internal static SimulationRealityContextSnapshot CloneRealityContext(
            SimulationRealityContextSnapshot source)
            => new SimulationRealityContextSnapshot
            {
                SchemaVersion = source.SchemaVersion,
                ContextSnapshotStableId = source.ContextSnapshotStableId,
                ProfileStableId = source.ProfileStableId,
                ProfileRevision = source.ProfileRevision,
                SignalRuleRevision = source.SignalRuleRevision,
                AreaSetStableId = source.AreaSetStableId,
                FrozenAtUtc = source.FrozenAtUtc,
                AvailabilityCode = source.AvailabilityCode,
                InputHashSha256 = source.InputHashSha256,
                SourceEvidence = source.SourceEvidence.Select(value =>
                    new SimulationRealitySourceEvidenceSnapshot
                    {
                        SourceEvidenceStableId = value.SourceEvidenceStableId,
                        SourceName = value.SourceName,
                        DatasetCode = value.DatasetCode,
                        AvailabilityCode = value.AvailabilityCode,
                        QualityCode = value.QualityCode,
                        FreshnessCode = value.FreshnessCode,
                        ObservedAtUtc = value.ObservedAtUtc,
                        RetrievedAtUtc = value.RetrievedAtUtc,
                        SpatialPrecisionCode = value.SpatialPrecisionCode,
                        UnitCodes = value.UnitCodes.ToArray(),
                        SourceHashSha256 = value.SourceHashSha256,
                        LicenseCode = value.LicenseCode,
                        SourceHref = value.SourceHref,
                        LimitationCodes = value.LimitationCodes.ToArray(),
                    }).ToArray(),
                SemanticSignals = source.SemanticSignals.Select(value =>
                    new SimulationRealitySemanticSignalSnapshot
                    {
                        SignalStableId = value.SignalStableId,
                        SignalCode = value.SignalCode,
                        SignalRuleRevision = value.SignalRuleRevision,
                        H3StableIds = value.H3StableIds.ToArray(),
                        AdvisoryCodes = value.AdvisoryCodes.ToArray(),
                        SourceEvidenceStableIds = value.SourceEvidenceStableIds.ToArray(),
                    }).ToArray(),
                ChangesSimulationRules = source.ChangesSimulationRules,
                MovesSpatialDefinitions = source.MovesSpatialDefinitions,
                CreatesIncidentOrEffect = source.CreatesIncidentOrEffect,
            };

        internal static void ValidateRealityContext(SimulationRealityContextSnapshot value)
        {
            if (value.SchemaVersion != SimulationRealityContextCodes.SchemaVersion
                || string.IsNullOrWhiteSpace(value.ContextSnapshotStableId)
                || string.IsNullOrWhiteSpace(value.ProfileStableId)
                || value.ProfileRevision <= 0
                || string.IsNullOrWhiteSpace(value.SignalRuleRevision)
                || string.IsNullOrWhiteSpace(value.AreaSetStableId)
                || value.FrozenAtUtc == default
                || !IsSha256(value.InputHashSha256)
                || value.SourceEvidence == null
                || value.SemanticSignals == null
                || value.ChangesSimulationRules
                || value.MovesSpatialDefinitions
                || value.CreatesIncidentOrEffect)
                throw new SimulationContractException("SimulationRealityContextInvalid");
            if (value.SourceEvidence.Any(item => item == null)
                || value.SemanticSignals.Any(item => item == null))
                throw new SimulationContractException("SimulationRealityContextInvalid");
            if (value.SourceEvidence.Select(item => item.SourceEvidenceStableId)
                    .Distinct(StringComparer.Ordinal).Count() != value.SourceEvidence.Length
                || value.SemanticSignals.Select(item => item.SignalStableId)
                    .Distinct(StringComparer.Ordinal).Count() != value.SemanticSignals.Length)
                throw new SimulationContractException("SimulationRealityContextDuplicate");
            if (value.SourceEvidence.Any(item =>
                    string.IsNullOrWhiteSpace(item.SourceEvidenceStableId)
                    || string.IsNullOrWhiteSpace(item.SourceName)
                    || string.IsNullOrWhiteSpace(item.DatasetCode)
                    || string.IsNullOrWhiteSpace(item.AvailabilityCode)
                    || string.IsNullOrWhiteSpace(item.QualityCode)
                    || string.IsNullOrWhiteSpace(item.FreshnessCode)
                    || string.IsNullOrWhiteSpace(item.SpatialPrecisionCode)
                    || string.IsNullOrWhiteSpace(item.LicenseCode)
                    || string.IsNullOrWhiteSpace(item.SourceHref)
                    || item.UnitCodes == null || item.LimitationCodes == null)
                || value.SemanticSignals.Any(item =>
                    string.IsNullOrWhiteSpace(item.SignalStableId)
                    || string.IsNullOrWhiteSpace(item.SignalCode)
                    || string.IsNullOrWhiteSpace(item.SignalRuleRevision)
                    || item.H3StableIds == null || item.AdvisoryCodes == null
                    || item.SourceEvidenceStableIds == null))
                throw new SimulationContractException("SimulationRealityContextInvalid");
        }

        private static bool IsSha256(string value) => value != null && value.Length == 64
            && value.All(Uri.IsHexDigit);
    }
}
