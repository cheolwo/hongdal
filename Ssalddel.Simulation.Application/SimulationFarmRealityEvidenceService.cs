using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationFarmRealityOperationalReader
    {
        Task<SimulationFarmRealityEvidenceBundle> ReadApprovedAsync(
            string areaSetStableId, string canonicalProductStableId,
            CancellationToken cancellationToken);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "세계 의미·인과·근거와 플레이 준비도 책임을 제공한다.",
        Boundary = "근거 자료와 Simulation 규칙 및 E 승격을 분리한다.")]
    public interface ISimulationFarmRealityEvidenceStore
    {
        Task<SimulationFarmRealityEvidenceSyncResponse> UpsertAsync(
            SimulationFarmRealityEvidenceBundle bundle,
            CancellationToken cancellationToken);
        Task<SimulationFarmRealityEvidenceBundle> ReadLatestAsync(
            string areaSetStableId, string canonicalProductStableId,
            CancellationToken cancellationToken);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationFarmRealityEvidence,
        SsalddelCodeLayer.Application,
        "운영 승인 묶음을 검증·해시해 Simulation 파생 원장에 명시적으로 동기화한다.",
        StepKey = "application.farm-reality-sync",
        DependsOnStepKeys = new[] { "api.farm-reality-evidence" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        ReadsFrom = SsalddelCodeDataScope.SharedPublicData,
        WritesTo = SsalddelCodeDataScope.DerivedWorld,
        Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
        FlowOrder = 30,
        Boundary = "Provider를 호출하지 않고 승인된 운영 자료만 읽으며 ContextProposal 외 Simulation 효과를 만들지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "세계 의미·인과·근거와 플레이 준비도 책임을 제공한다.",
        Boundary = "근거 자료와 Simulation 규칙 및 E 승격을 분리한다.")]
    public sealed class SimulationFarmRealityEvidenceService
    {
        private readonly ISimulationFarmRealityOperationalReader operationalReader;
        private readonly ISimulationFarmRealityEvidenceStore store;

        public SimulationFarmRealityEvidenceService(
            ISimulationFarmRealityOperationalReader reader,
            ISimulationFarmRealityEvidenceStore evidenceStore)
        {
            operationalReader = reader ?? throw new ArgumentNullException(nameof(reader));
            store = evidenceStore ?? throw new ArgumentNullException(nameof(evidenceStore));
        }

        public async Task<SimulationFarmRealityEvidenceSyncResponse> SyncAsync(
            SimulationFarmRealityEvidenceSyncRequest request,
            CancellationToken cancellationToken)
        {
            ValidateIds(request.AreaSetStableId, request.CanonicalProductStableId);
            var bundle = await operationalReader.ReadApprovedAsync(
                request.AreaSetStableId, request.CanonicalProductStableId,
                cancellationToken);
            Validate(bundle);
            bundle.InputHashSha256 = ComputeHash(bundle);
            bundle.EvidenceRevision = "farm-potato-reality:"
                + bundle.InputHashSha256.Substring(0, 16);
            return await store.UpsertAsync(bundle, cancellationToken);
        }

        public Task<SimulationFarmRealityEvidenceBundle> ReadAsync(
            string areaSetStableId, string canonicalProductStableId,
            CancellationToken cancellationToken)
        {
            ValidateIds(areaSetStableId, canonicalProductStableId);
            return store.ReadLatestAsync(areaSetStableId,
                canonicalProductStableId, cancellationToken);
        }

        public static string ComputeHash(SimulationFarmRealityEvidenceBundle bundle)
        {
            var canonical = new
            {
                bundle.SchemaVersion,
                bundle.AreaSetStableId,
                bundle.CanonicalProductStableId,
                bundle.ProductDisplayName,
                bundle.ProductIdentityRevision,
                Sources = bundle.Sources.OrderBy(item => item.SourceEvidenceStableId,
                    StringComparer.Ordinal).Select(item => new
                    {
                        item.SourceEvidenceStableId, item.SourceId, item.DatasetId,
                        item.SourceName, item.CodeScheme, item.ExternalCode,
                        item.RelationStatusCode, item.AvailabilityCode, item.QualityCode,
                        item.ObservedAtUtc, item.RetrievedAtUtc,
                        item.SpatialPrecisionCode,
                        UnitCodes = item.UnitCodes.OrderBy(value => value, StringComparer.Ordinal),
                        item.MaxAgeHours,
                        item.SourceHashSha256, item.SourceHref,
                        Limitations = item.LimitationCodes.OrderBy(value => value, StringComparer.Ordinal),
                        Advisories = item.AdvisoryCodes.OrderBy(value => value, StringComparer.Ordinal),
                    }),
                bundle.ChangesSimulationRules,
                bundle.MovesSpatialDefinitions,
                bundle.CreatesIncidentOrEffect,
            };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical));
            using (var sha = SHA256.Create())
                return ToHex(sha.ComputeHash(bytes));
        }

        public static SimulationRealityContextSnapshot ToRealityContext(
            SimulationFarmRealityEvidenceBundle bundle,
            string contextSnapshotStableId, DateTimeOffset frozenAtUtc)
        {
            Validate(bundle);
            if (!HasValidInputHash(bundle))
                throw new InvalidOperationException("SimulationFarmRealityEvidenceHashMismatch");
            var currentSources = bundle.Sources.Where(item =>
                item.AvailabilityCode == SimulationRealityContextCodes.Available
                && item.QualityCode == SimulationRealityContextCodes.Valid
                && item.RetrievedAtUtc <= frozenAtUtc
                && frozenAtUtc - item.RetrievedAtUtc <= TimeSpan.FromHours(item.MaxAgeHours))
                .ToArray();
            var signals = currentSources.SelectMany(item => item.AdvisoryCodes.Select(code =>
                new { Source = item, Advisory = code }))
                .GroupBy(item => item.Advisory, StringComparer.Ordinal)
                .Select(group => new SimulationRealitySemanticSignalSnapshot
                {
                    SignalStableId = "reality-signal:farm-potato:" + group.Key,
                    SignalCode = SignalCode(group.Key),
                    SignalRuleRevision = "farm-potato-context-proposal.r1",
                    H3StableIds = new[] { "h3-candidate:highland-farm",
                        "h3-candidate:farm-seasonal-production-loop" },
                    AdvisoryCodes = new[] { group.Key },
                    SourceEvidenceStableIds = group.Select(item =>
                        item.Source.SourceEvidenceStableId).Distinct().OrderBy(value => value).ToArray(),
                }).ToArray();
            return new SimulationRealityContextSnapshot
            {
                ContextSnapshotStableId = contextSnapshotStableId,
                ProfileStableId = SimulationFarmRealityEvidenceCodes.RealityContextProfileStableId,
                ProfileRevision = 1,
                SignalRuleRevision = "farm-potato-context-proposal.r1",
                AreaSetStableId = bundle.AreaSetStableId,
                FrozenAtUtc = frozenAtUtc,
                AvailabilityCode = currentSources.Length == 0
                    ? SimulationRealityContextCodes.Unavailable
                    : currentSources.Length == bundle.Sources.Length
                        ? SimulationRealityContextCodes.Available
                        : SimulationRealityContextCodes.PartiallyAvailable,
                InputHashSha256 = bundle.InputHashSha256,
                SourceEvidence = bundle.Sources.Select(item =>
                    new SimulationRealitySourceEvidenceSnapshot
                    {
                        SourceEvidenceStableId = item.SourceEvidenceStableId,
                        SourceName = item.SourceName,
                        DatasetCode = item.SourceId + "/" + item.DatasetId,
                        AvailabilityCode = item.AvailabilityCode,
                        QualityCode = item.QualityCode,
                        FreshnessCode = item.AvailabilityCode != SimulationRealityContextCodes.Available
                            ? SimulationRealityContextCodes.Unknown
                            : item.RetrievedAtUtc <= frozenAtUtc
                                && frozenAtUtc - item.RetrievedAtUtc <= TimeSpan.FromHours(item.MaxAgeHours)
                                ? SimulationRealityContextCodes.Current
                                : SimulationRealityContextCodes.Stale,
                        ObservedAtUtc = item.ObservedAtUtc,
                        RetrievedAtUtc = item.RetrievedAtUtc,
                        SpatialPrecisionCode = item.SpatialPrecisionCode,
                        UnitCodes = item.UnitCodes,
                        SourceHashSha256 = item.SourceHashSha256,
                        LicenseCode = "SourceTermsApply",
                        SourceHref = item.SourceHref,
                        LimitationCodes = item.LimitationCodes,
                    }).ToArray(),
                SemanticSignals = signals,
                ChangesSimulationRules = false,
                MovesSpatialDefinitions = false,
                CreatesIncidentOrEffect = false,
            };
        }

        private static string SignalCode(string advisory) => advisory switch
        {
            SimulationRealityContextCodes.ReviewFieldWorkTiming =>
                SimulationRealityContextCodes.WetWorkContext,
            SimulationRealityContextCodes.InspectCropHealth =>
                SimulationRealityContextCodes.CropHealthAttentionContext,
            SimulationRealityContextCodes.ReviewShipmentTiming =>
                SimulationRealityContextCodes.MarketPressureContext,
            _ => SimulationRealityContextCodes.CropHealthAttentionContext,
        };

        private static void ValidateIds(string areaSetStableId, string productStableId)
        {
            if (areaSetStableId != SimulationFarmRealityEvidenceCodes.FarmAreaSetStableId
                || productStableId != SimulationFarmRealityEvidenceCodes.PotatoProductStableId)
                throw new InvalidOperationException("SimulationFarmRealityEvidenceTargetInvalid");
        }

        public static bool HasValidInputHash(SimulationFarmRealityEvidenceBundle bundle)
            => bundle.InputHashSha256.Length == 64
                && bundle.InputHashSha256.All(Uri.IsHexDigit)
                && string.Equals(bundle.InputHashSha256, ComputeHash(bundle),
                    StringComparison.OrdinalIgnoreCase);

        private static void Validate(SimulationFarmRealityEvidenceBundle bundle)
        {
            ValidateIds(bundle.AreaSetStableId, bundle.CanonicalProductStableId);
            if (bundle.SchemaVersion != SimulationFarmRealityEvidenceCodes.SchemaVersion
                || bundle.Sources.Length != 4
                || bundle.Sources.GroupBy(item => item.SourceEvidenceStableId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1)
                || bundle.ChangesSimulationRules || bundle.MovesSpatialDefinitions
                || bundle.CreatesIncidentOrEffect
                || bundle.Sources.Any(item => item.AvailabilityCode ==
                        SimulationRealityContextCodes.Available
                    && (item.ObservedAtUtc == null || item.RetrievedAtUtc == null
                        || item.ObservedAtUtc > item.RetrievedAtUtc
                        || item.RetrievedAtUtc > bundle.CreatedAtUtc
                        || item.SourceHashSha256.Length != 64
                        || item.SourceHashSha256.Any(value => !Uri.IsHexDigit(value))
                        || item.UnitCodes.Length == 0
                        || item.UnitCodes.Any(string.IsNullOrWhiteSpace)
                        || item.MaxAgeHours <= 0)))
                throw new InvalidOperationException("SimulationFarmRealityEvidenceInvalid");
        }

        private static string ToHex(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);
            foreach (var value in bytes) result.Append(value.ToString("x2"));
            return result.ToString();
        }
    }
}
