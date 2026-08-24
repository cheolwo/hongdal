using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationRealityContextClock
    {
        DateTimeOffset UtcNow { get; }
    }

    public sealed class SystemSimulationRealityContextClock : ISimulationRealityContextClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    public interface ISimulationRealityContextCatalogReader
    {
        bool TryFreeze(string profileStableId, string areaSetStableId,
            string contextSnapshotStableId, DateTimeOffset frozenAtUtc,
            out SimulationRealityContextSnapshot snapshot, out string errorCode);
    }

    /// <summary>
    /// 수집·승인을 끝낸 정규화 자료만 읽는다. Provider 호출은 세션 생성·Tick·Unity 조회에서 하지 않는다.
    /// </summary>
    public sealed class FileSimulationRealityContextCatalogReader :
        ISimulationRealityContextCatalogReader
    {
        private const string CatalogSchemaVersion = "simulation-reality-context-catalog.v1";
        private readonly string path;

        public FileSimulationRealityContextCatalogReader(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath))
                throw new ArgumentException("RealityContextCatalogPathMissing", nameof(catalogPath));
            path = ResolvePath(catalogPath);
        }

        public bool TryFreeze(string profileStableId, string areaSetStableId,
            string contextSnapshotStableId, DateTimeOffset frozenAtUtc,
            out SimulationRealityContextSnapshot snapshot, out string errorCode)
        {
            snapshot = new SimulationRealityContextSnapshot();
            if (!File.Exists(path))
            {
                errorCode = "RealityContextCatalogUnavailable";
                return false;
            }

            try
            {
                var catalog = JsonSerializer.Deserialize<RealityContextCatalog>(
                    File.ReadAllBytes(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("RealityContextCatalogInvalid");
                Require(catalog.SchemaVersion == CatalogSchemaVersion,
                    "RealityContextCatalogSchemaMismatch");
                Require(catalog.Profiles != null, "RealityContextCatalogInvalid");
                var matches = catalog.Profiles.Where(value => string.Equals(
                    value.ProfileStableId, profileStableId, StringComparison.Ordinal)).ToArray();
                Require(matches.Length == 1, matches.Length == 0
                    ? "RealityContextProfileNotFound" : "RealityContextProfileDuplicate");
                var profile = matches[0];
                Require(string.Equals(profile.AreaSetStableId, areaSetStableId,
                    StringComparison.Ordinal), "RealityContextAreaSetMismatch");
                snapshot = Freeze(profile, contextSnapshotStableId, frozenAtUtc);
                errorCode = string.Empty;
                return true;
            }
            catch (Exception error) when (error is IOException or JsonException
                or InvalidOperationException or ArgumentException)
            {
                errorCode = error.Message;
                return false;
            }
        }

        private static SimulationRealityContextSnapshot Freeze(
            RealityContextProfile profile, string contextSnapshotStableId,
            DateTimeOffset frozenAtUtc)
        {
            Require(!string.IsNullOrWhiteSpace(contextSnapshotStableId)
                    && !string.IsNullOrWhiteSpace(profile.ProfileStableId)
                    && profile.ProfileRevision > 0
                    && !string.IsNullOrWhiteSpace(profile.SignalRuleRevision)
                    && !string.IsNullOrWhiteSpace(profile.AreaSetStableId)
                    && profile.MaxAgeHours > 0
                    && profile.H3StableIds != null
                    && profile.SourceSnapshots != null,
                "RealityContextProfileInvalid");
            Require(!profile.PublicDataChangesSimulationRules
                    && !profile.PublicDataMovesSpatialDefinitions
                    && !profile.ContextProposalCreatesIncidentOrEffect,
                "RealityContextAuthorityBoundaryInvalid");

            var sourceEvidence = new List<SimulationRealitySourceEvidenceSnapshot>();
            var usableSources = new List<RealityContextSource>();
            foreach (var source in profile.SourceSnapshots.OrderBy(
                         value => value.SourceEvidenceStableId, StringComparer.Ordinal))
            {
                ValidateSource(source);
                if (source.AvailabilityCode == SimulationRealityContextCodes.Available)
                    Require(source.RetrievedAtUtc <= frozenAtUtc,
                        "RealityContextSourceFromFuture");
                var freshness = ResolveFreshness(source, profile.MaxAgeHours, frozenAtUtc);
                var usable = source.AvailabilityCode == SimulationRealityContextCodes.Available
                    && source.QualityCode == SimulationRealityContextCodes.Valid
                    && freshness == SimulationRealityContextCodes.Current;
                if (usable) usableSources.Add(source);
                sourceEvidence.Add(new SimulationRealitySourceEvidenceSnapshot
                {
                    SourceEvidenceStableId = source.SourceEvidenceStableId,
                    SourceName = source.SourceName,
                    DatasetCode = source.DatasetCode,
                    AvailabilityCode = source.AvailabilityCode,
                    QualityCode = source.QualityCode,
                    FreshnessCode = freshness,
                    ObservedAtUtc = source.ObservedAtUtc,
                    RetrievedAtUtc = source.RetrievedAtUtc,
                    SpatialPrecisionCode = source.SpatialPrecisionCode,
                    UnitCodes = source.Measurements.Select(value => value.UnitCode)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                    SourceHashSha256 = source.SourceHashSha256,
                    LicenseCode = source.LicenseCode,
                    SourceHref = source.SourceHref,
                    LimitationCodes = source.LimitationCodes
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                });
            }

            var inputHash = CalculateInputHash(profile, sourceEvidence, frozenAtUtc);
            var signals = DeriveSignals(profile, usableSources, contextSnapshotStableId);
            var availableCount = sourceEvidence.Count(value =>
                value.AvailabilityCode == SimulationRealityContextCodes.Available
                && value.QualityCode == SimulationRealityContextCodes.Valid
                && value.FreshnessCode == SimulationRealityContextCodes.Current);
            var availability = availableCount == 0
                ? SimulationRealityContextCodes.Unavailable
                : availableCount == sourceEvidence.Count
                    ? SimulationRealityContextCodes.Available
                    : SimulationRealityContextCodes.PartiallyAvailable;

            return new SimulationRealityContextSnapshot
            {
                ContextSnapshotStableId = contextSnapshotStableId,
                ProfileStableId = profile.ProfileStableId,
                ProfileRevision = profile.ProfileRevision,
                SignalRuleRevision = profile.SignalRuleRevision,
                AreaSetStableId = profile.AreaSetStableId,
                FrozenAtUtc = frozenAtUtc,
                AvailabilityCode = availability,
                InputHashSha256 = inputHash,
                SourceEvidence = sourceEvidence.ToArray(),
                SemanticSignals = signals,
                ChangesSimulationRules = false,
                MovesSpatialDefinitions = false,
                CreatesIncidentOrEffect = false,
            };
        }

        private static SimulationRealitySemanticSignalSnapshot[] DeriveSignals(
            RealityContextProfile profile, IReadOnlyCollection<RealityContextSource> sources,
            string contextSnapshotStableId)
        {
            var candidates = new List<(string Code, string[] Advisory, string[] SourceIds)>();
            AddIf(candidates, sources, "DailyPrecipitationMm", value => value >= 10m,
                SimulationRealityContextCodes.WetWorkContext,
                new[] { SimulationRealityContextCodes.InspectDrainage,
                    SimulationRealityContextCodes.ReviewFieldWorkTiming });
            AddIf(candidates, sources, "ForecastPrecipitationProbabilityPercent",
                value => value >= 60m,
                SimulationRealityContextCodes.WetWorkContext,
                new[] { SimulationRealityContextCodes.InspectDrainage,
                    SimulationRealityContextCodes.ReviewFieldWorkTiming });
            AddIf(candidates, sources, "MinimumTemperatureCelsius", value => value <= 3m,
                SimulationRealityContextCodes.ColdStressContext,
                new[] { SimulationRealityContextCodes.ProtectColdSensitiveWork });
            AddIf(candidates, sources, "ForecastMinimumTemperatureCelsius",
                value => value <= 3m,
                SimulationRealityContextCodes.ColdStressContext,
                new[] { SimulationRealityContextCodes.ProtectColdSensitiveWork });
            AddIf(candidates, sources, "RelativeHumidityPercent", value => value >= 80m,
                SimulationRealityContextCodes.CropHealthAttentionContext,
                new[] { SimulationRealityContextCodes.InspectCropHealth });
            AddIf(candidates, sources, "MarketPriceChangePercent",
                value => Math.Abs(value) >= 10m,
                SimulationRealityContextCodes.MarketPressureContext,
                new[] { SimulationRealityContextCodes.ReviewShipmentTiming });
            AddIf(candidates, sources, "ForestFireRiskIndex", value => value >= 60m,
                SimulationRealityContextCodes.DryForestContext,
                new[] { SimulationRealityContextCodes.ReviewNatureFireReadiness });

            return candidates.GroupBy(value => value.Code, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new SimulationRealitySemanticSignalSnapshot
                {
                    SignalStableId = contextSnapshotStableId + ":signal:"
                        + group.Key.ToLowerInvariant(),
                    SignalCode = group.Key,
                    SignalRuleRevision = profile.SignalRuleRevision,
                    H3StableIds = profile.H3StableIds.Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                    AdvisoryCodes = group.SelectMany(value => value.Advisory)
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                    SourceEvidenceStableIds = group.SelectMany(value => value.SourceIds)
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                }).ToArray();
        }

        private static void AddIf(
            ICollection<(string Code, string[] Advisory, string[] SourceIds)> target,
            IEnumerable<RealityContextSource> sources, string measurementCode,
            Func<decimal, bool> predicate, string signalCode, string[] advisoryCodes)
        {
            var matches = sources.SelectMany(source => source.Measurements
                    .Where(value => value.MeasurementCode == measurementCode)
                    .Select(value => new { Source = source, Measurement = value }))
                .Where(value => predicate(value.Measurement.Value))
                .Select(value => value.Source.SourceEvidenceStableId)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            if (matches.Length > 0) target.Add((signalCode, advisoryCodes, matches));
        }

        private static string ResolveFreshness(RealityContextSource source,
            int maxAgeHours, DateTimeOffset frozenAtUtc)
        {
            if (source.AvailabilityCode != SimulationRealityContextCodes.Available
                || source.ObservedAtUtc == null)
                return SimulationRealityContextCodes.Unknown;
            return frozenAtUtc - source.ObservedAtUtc.Value <= TimeSpan.FromHours(maxAgeHours)
                ? SimulationRealityContextCodes.Current
                : SimulationRealityContextCodes.Stale;
        }

        private static void ValidateSource(RealityContextSource source)
        {
            Require(!string.IsNullOrWhiteSpace(source.SourceEvidenceStableId)
                    && !string.IsNullOrWhiteSpace(source.SourceName)
                    && !string.IsNullOrWhiteSpace(source.DatasetCode)
                    && (source.AvailabilityCode == SimulationRealityContextCodes.Available
                        || source.AvailabilityCode == SimulationRealityContextCodes.Unavailable)
                    && (source.QualityCode == SimulationRealityContextCodes.Valid
                        || source.QualityCode == SimulationRealityContextCodes.Incomplete
                        || source.QualityCode == SimulationRealityContextCodes.Unavailable)
                    && !string.IsNullOrWhiteSpace(source.SpatialPrecisionCode)
                    && !string.IsNullOrWhiteSpace(source.LicenseCode)
                    && !string.IsNullOrWhiteSpace(source.SourceHref)
                    && source.Measurements != null
                    && source.LimitationCodes != null,
                "RealityContextSourceInvalid");
            if (source.AvailabilityCode == SimulationRealityContextCodes.Available)
            {
                Require(source.ObservedAtUtc != null && source.RetrievedAtUtc != null
                        && source.RetrievedAtUtc >= source.ObservedAtUtc
                        && IsHash(source.SourceHashSha256)
                        && source.Measurements!.Length > 0
                        && source.Measurements.All(value =>
                            !string.IsNullOrWhiteSpace(value.MeasurementCode)
                            && !string.IsNullOrWhiteSpace(value.UnitCode)),
                    "RealityContextAvailableSourceInvalid");
            }
        }

        private static string CalculateInputHash(RealityContextProfile profile,
            IEnumerable<SimulationRealitySourceEvidenceSnapshot> evidence,
            DateTimeOffset frozenAtUtc)
        {
            var canonical = new StringBuilder();
            Add(canonical, profile.ProfileStableId);
            Add(canonical, profile.ProfileRevision.ToString(CultureInfo.InvariantCulture));
            Add(canonical, profile.SignalRuleRevision);
            Add(canonical, profile.AreaSetStableId);
            Add(canonical, frozenAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            foreach (var source in profile.SourceSnapshots.OrderBy(
                         value => value.SourceEvidenceStableId, StringComparer.Ordinal))
            {
                Add(canonical, source.SourceEvidenceStableId);
                Add(canonical, source.AvailabilityCode);
                Add(canonical, source.QualityCode);
                Add(canonical, source.ObservedAtUtc?.ToUniversalTime().ToString("O",
                    CultureInfo.InvariantCulture));
                Add(canonical, source.RetrievedAtUtc?.ToUniversalTime().ToString("O",
                    CultureInfo.InvariantCulture));
                Add(canonical, source.SourceHashSha256);
                foreach (var measurement in source.Measurements.OrderBy(
                             value => value.MeasurementCode, StringComparer.Ordinal))
                {
                    Add(canonical, measurement.MeasurementCode);
                    Add(canonical, measurement.Value.ToString(CultureInfo.InvariantCulture));
                    Add(canonical, measurement.UnitCode);
                }
            }
            foreach (var item in evidence.OrderBy(value => value.SourceEvidenceStableId,
                         StringComparer.Ordinal))
                Add(canonical, item.FreshnessCode);
            using var sha = SHA256.Create();
            return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())));
        }

        private static void Add(StringBuilder target, string? value)
            => target.Append(value ?? string.Empty).Append('\n');

        private static string ToHex(byte[] value)
        {
            var text = new StringBuilder(value.Length * 2);
            foreach (var item in value) text.Append(item.ToString("x2", CultureInfo.InvariantCulture));
            return text.ToString();
        }

        private static bool IsHash(string value) => value != null && value.Length == 64
            && value.All(Uri.IsHexDigit);

        private static void Require(bool condition, string errorCode)
        {
            if (!condition) throw new InvalidOperationException(errorCode);
        }

        private static string ResolvePath(string value)
        {
            if (Path.IsPathRooted(value)) return Path.GetFullPath(value);
            var direct = Path.GetFullPath(value);
            if (File.Exists(direct)) return direct;
            for (var current = new DirectoryInfo(AppContext.BaseDirectory);
                 current != null; current = current.Parent)
            {
                var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
                if (File.Exists(candidate)) return candidate;
            }
            return direct;
        }

        private sealed class RealityContextCatalog
        {
            public string SchemaVersion { get; set; } = string.Empty;
            public RealityContextProfile[] Profiles { get; set; } = Array.Empty<RealityContextProfile>();
        }

        private sealed class RealityContextProfile
        {
            public string ProfileStableId { get; set; } = string.Empty;
            public int ProfileRevision { get; set; }
            public string SignalRuleRevision { get; set; } = string.Empty;
            public string AreaSetStableId { get; set; } = string.Empty;
            public int MaxAgeHours { get; set; } = 48;
            public string[] H3StableIds { get; set; } = Array.Empty<string>();
            public RealityContextSource[] SourceSnapshots { get; set; } = Array.Empty<RealityContextSource>();
            public bool PublicDataChangesSimulationRules { get; set; }
            public bool PublicDataMovesSpatialDefinitions { get; set; }
            public bool ContextProposalCreatesIncidentOrEffect { get; set; }
        }

        private sealed class RealityContextSource
        {
            public string SourceEvidenceStableId { get; set; } = string.Empty;
            public string SourceName { get; set; } = string.Empty;
            public string DatasetCode { get; set; } = string.Empty;
            public string AvailabilityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
            public string QualityCode { get; set; } = SimulationRealityContextCodes.Unavailable;
            public DateTimeOffset? ObservedAtUtc { get; set; }
            public DateTimeOffset? RetrievedAtUtc { get; set; }
            public string SpatialPrecisionCode { get; set; } = string.Empty;
            public string SourceHashSha256 { get; set; } = string.Empty;
            public string LicenseCode { get; set; } = string.Empty;
            public string SourceHref { get; set; } = string.Empty;
            public string[] LimitationCodes { get; set; } = Array.Empty<string>();
            public RealityContextMeasurement[] Measurements { get; set; } = Array.Empty<RealityContextMeasurement>();
        }

        private sealed class RealityContextMeasurement
        {
            public string MeasurementCode { get; set; } = string.Empty;
            public decimal Value { get; set; }
            public string UnitCode { get; set; } = string.Empty;
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "세계 의미·인과·근거와 플레이 준비도 책임을 제공한다.",
        Boundary = "근거 자료와 Simulation 규칙 및 E 승격을 분리한다.")]
    public sealed class SimulationRealityContextService
    {
        private readonly ISimulationRealityContextCatalogReader catalog;
        private readonly 경영SimulationSessionAccessor sessions;
        private readonly ISimulationRealityContextClock clock;

        public SimulationRealityContextService(ISimulationRealityContextCatalogReader catalogReader,
            경영SimulationSessionAccessor sessionAccessor,
            ISimulationRealityContextClock realityClock)
        {
            catalog = catalogReader ?? throw new ArgumentNullException(nameof(catalogReader));
            sessions = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));
            clock = realityClock ?? throw new ArgumentNullException(nameof(realityClock));
        }

        public SimulationRealityContextSnapshot FreezeForSession(string profileStableId,
            string areaSetStableId, Guid clientRequestId)
        {
            if (string.IsNullOrWhiteSpace(profileStableId))
                throw new InvalidOperationException("RealityContextProfileStableIdMissing");
            var sessionStableId = "simulation-session:" + clientRequestId.ToString("N");
            try
            {
                var existing = sessions.Require(sessionStableId).RealityContextSnapshot();
                if (existing != null)
                {
                    if (!string.Equals(existing.ProfileStableId, profileStableId.Trim(),
                            StringComparison.Ordinal)
                        || !string.Equals(existing.AreaSetStableId, areaSetStableId.Trim(),
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationCreateRequestPayloadConflict");
                    return existing;
                }
            }
            catch (SimulationNotFoundException)
            {
                // 첫 생성만 승인 대장을 읽고 동결한다.
            }
            var snapshotId = "reality-context:session:"
                + clientRequestId.ToString("N") + ":v1";
            if (!catalog.TryFreeze(profileStableId.Trim(), areaSetStableId.Trim(), snapshotId,
                    clock.UtcNow, out var snapshot, out var errorCode))
                throw new InvalidOperationException(errorCode);
            return snapshot;
        }

        public SimulationRealityContextPlayerProjectionResponse ReadPlayerProjection(
            string sessionStableId, bool includeSourceDetails)
        {
            var snapshot = sessions.Require(sessionStableId).RealityContextSnapshot()
                ?? throw new SimulationNotFoundException("SimulationRealityContextNotFound");
            return Project(snapshot, includeSourceDetails);
        }

        public static SimulationRealityContextPlayerProjectionResponse Project(
            SimulationRealityContextSnapshot snapshot, bool includeSourceDetails)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var phenomena = snapshot.SemanticSignals.Select(signal =>
            {
                var text = Describe(signal.SignalCode);
                return new SimulationRealityPhenomenonProjection
                {
                    PhenomenonStableId = signal.SignalStableId + ":phenomenon",
                    PhenomenonCode = signal.SignalCode,
                    TitleKorean = text.Title,
                    SummaryKorean = text.Summary,
                    H3StableIds = signal.H3StableIds.ToArray(),
                    AdvisoryCodes = signal.AdvisoryCodes.ToArray(),
                };
            }).ToArray();
            var sources = includeSourceDetails
                ? snapshot.SourceEvidence.Select(value =>
                    new SimulationRealitySourceInformationProjection
                    {
                        InformationStableId = value.SourceEvidenceStableId + ":information",
                        SourceName = value.SourceName,
                        ReferenceTimeUtc = value.ObservedAtUtc,
                        SpatialPrecisionCode = value.SpatialPrecisionCode,
                        SourceHref = value.SourceHref,
                        LimitationCodes = value.LimitationCodes.ToArray(),
                        LimitationSummariesKorean = value.LimitationCodes
                            .Select(DescribeLimitation).ToArray(),
                    }).ToArray()
                : Array.Empty<SimulationRealitySourceInformationProjection>();
            return new SimulationRealityContextPlayerProjectionResponse
            {
                ContextSnapshotStableId = snapshot.ContextSnapshotStableId,
                AvailabilityCode = snapshot.AvailabilityCode,
                FrozenAtUtc = snapshot.FrozenAtUtc,
                Phenomena = phenomena,
                SourceInformation = sources,
                SourceDetailsIncluded = includeSourceDetails,
                PresentationOnly = true,
            };
        }

        private static (string Title, string Summary) Describe(string signalCode)
            => signalCode switch
            {
                SimulationRealityContextCodes.WetWorkContext =>
                    ("젖은 작업 환경", "배수와 오늘의 밭 작업 순서를 살펴볼 만합니다."),
                SimulationRealityContextCodes.ColdStressContext =>
                    ("찬 기운 주의", "저온에 민감한 작업과 작물 상태를 먼저 확인해 보세요."),
                SimulationRealityContextCodes.CropHealthAttentionContext =>
                    ("작물 상태 확인", "습한 조건이 이어져 생육 상태를 살펴볼 만합니다."),
                SimulationRealityContextCodes.MarketPressureContext =>
                    ("시장 흐름 변화", "출하 시점과 물량을 다시 검토할 참고 문맥이 있습니다."),
                SimulationRealityContextCodes.DryForestContext =>
                    ("마른 숲 주의", "Nature 이동과 화재 대비 상태를 확인해 보세요."),
                _ => ("주변 상황", "현재 지역의 현실 문맥을 참고할 수 있습니다."),
            };

        private static string DescribeLimitation(string code)
            => code switch
            {
                "ApprovedObservationNotCollected" => "승인된 관측 자료가 아직 적재되지 않았습니다.",
                "ApprovedForecastNotCollected" => "승인된 예보 자료가 아직 적재되지 않았습니다.",
                "StationObservationIsNotParcelObservation" => "관측소 값은 개별 농지의 직접 관측이 아닙니다.",
                "FiveKilometerGridIsNotParcelForecast" => "5km 격자 예보는 개별 농지의 직접 예보가 아닙니다.",
                "NoScenarioFallback" => "자료가 없을 때 시나리오 값으로 대신하지 않습니다.",
                "NotAutomaticWorkOrIncidentRule" => "업무나 사건을 자동으로 확정하는 규칙이 아닙니다.",
                "MarketContextOnly" => "시장 흐름을 이해하기 위한 참고 문맥입니다.",
                "UnitAndMarketStageAlignmentRequired" => "단위와 유통 단계가 맞아야 비교할 수 있습니다.",
                "NotProductionProfitOrSalePriceRule" => "생산량·수익·판매가를 자동 계산하지 않습니다.",
                _ => code,
            };
    }
}
