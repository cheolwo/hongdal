using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.Farm
{
    public static class FarmProducerApiRoutes
    {
        public const string Producer =
            "api/v1/shipper/world/zones/farm/producer-perspective";
    }

    public static class FarmProducerRoleCodes
    {
        public const string Producer = "Producer";
    }

    public static class FarmSensorConditionCodes
    {
        public const string Normal = "Normal";
        public const string Dry = "Dry";
        public const string Critical = "Critical";
        public const string Waterlogged = "Waterlogged";
        public const string Unknown = "Unknown";
    }

    public sealed class FarmSensorObservationApiModel
    {
        public decimal Value { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public DateTimeOffset ObservedAt { get; set; }
        public string FreshnessStatusCode { get; set; } = string.Empty;
        public string ConditionCode { get; set; } = string.Empty;
        public string AssessmentRuleRevision { get; set; } = string.Empty;
        public string? EvidenceCardId { get; set; }
        public string? ConfidenceCode { get; set; }
        public string? Limitation { get; set; }
    }

    public sealed class FarmSensorApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SensorTypeCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public FarmSensorObservationApiModel? LatestObservation { get; set; }
    }

    public sealed class FarmCultivationApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string? CropReferenceStableId { get; set; }
        public string? CropReferenceSourceKey { get; set; }
        public string GrowthStatusCode { get; set; } = string.Empty;
        public string? PlantedOn { get; set; }
        public string? ExpectedHarvestOn { get; set; }
    }

    public sealed class FarmPlotApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PlotName { get; set; } = string.Empty;
        public string? SoilManagementProfileCode { get; set; }
        public FarmCultivationApiModel[] Cultivations { get; set; } =
            Array.Empty<FarmCultivationApiModel>();
        public FarmSensorApiModel[] Sensors { get; set; } =
            Array.Empty<FarmSensorApiModel>();
    }

    public sealed class FarmApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public FarmPlotApiModel[] Plots { get; set; } = Array.Empty<FarmPlotApiModel>();
    }

    public sealed class FarmProducerPerspectiveApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string WorldZoneCode { get; set; } = string.Empty;
        public string ViewerScopeCode { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string AuthorizationDecisionId { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public FarmApiModel[] Farms { get; set; } = Array.Empty<FarmApiModel>();
        public NpcMovementApiModel[] Workers { get; set; } = Array.Empty<NpcMovementApiModel>();
    }

    public sealed class FarmSensorObservationSnapshot
    {
        public decimal Value { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public DateTimeOffset ObservedAt { get; set; }
        public string FreshnessStatusCode { get; set; } = string.Empty;
        public string ConditionCode { get; set; } = string.Empty;
        public string AssessmentRuleRevision { get; set; } = string.Empty;
        public string? EvidenceCardId { get; set; }
        public string? ConfidenceCode { get; set; }
        public string? Limitation { get; set; }
    }

    public sealed class FarmSensorSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SensorTypeCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public FarmSensorObservationSnapshot? LatestObservation { get; set; }
    }

    public sealed class FarmCultivationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string? CropReferenceStableId { get; set; }
        public string? CropReferenceSourceKey { get; set; }
        public string GrowthStatusCode { get; set; } = string.Empty;
        public string? PlantedOn { get; set; }
        public string? ExpectedHarvestOn { get; set; }
    }

    public sealed class FarmPlotSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PlotName { get; set; } = string.Empty;
        public string? SoilManagementProfileCode { get; set; }
        public FarmCultivationSnapshot[] Cultivations { get; set; } =
            Array.Empty<FarmCultivationSnapshot>();
        public FarmSensorSnapshot[] Sensors { get; set; } = Array.Empty<FarmSensorSnapshot>();
    }

    public sealed class FarmSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public FarmPlotSnapshot[] Plots { get; set; } = Array.Empty<FarmPlotSnapshot>();
    }

    public sealed class FarmProducerPerspectiveSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public FarmSnapshot[] Farms { get; set; } = Array.Empty<FarmSnapshot>();
        public NpcMovementSnapshot[] Workers { get; set; } = Array.Empty<NpcMovementSnapshot>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface IFarmProducerPerspectiveApiClient
    {
        Task<FarmProducerPerspectiveApiModel> GetAsync(
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class FarmProducerPerspectiveMapper
    {
        private static readonly HashSet<string> Conditions = new HashSet<string>(StringComparer.Ordinal)
        {
            FarmSensorConditionCodes.Normal,
            FarmSensorConditionCodes.Dry,
            FarmSensorConditionCodes.Critical,
            FarmSensorConditionCodes.Waterlogged,
            FarmSensorConditionCodes.Unknown,
        };

        public FarmProducerPerspectiveSnapshot Map(FarmProducerPerspectiveApiModel source)
        {
            if (source == null
                || !StableDataId.IsValid(source.StableId)
                || source.Revision < 0
                || source.GeneratedAt == default
                || !string.Equals(source.AuthorizedRoleCode, FarmProducerRoleCodes.Producer, StringComparison.Ordinal)
                || !string.Equals(source.WorldZoneCode, "farm", StringComparison.Ordinal)
                || !string.Equals(source.ViewerScopeCode, "AuthorizedParty", StringComparison.Ordinal)
                || (source.SourceTypeCode != "OperationalProjection"
                    && source.SourceTypeCode != "SimulatedFixture")
                || string.IsNullOrWhiteSpace(source.AuthorizationDecisionId)
                || source.Farms == null || source.Workers == null)
            {
                throw new InvalidOperationException("FarmProducerPerspectiveInvalid");
            }

            EnsureUnique(source.Farms.Select(item => item?.StableId), "FarmStableIdDuplicate");
            return new FarmProducerPerspectiveSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                AuthorizedRoleCode = source.AuthorizedRoleCode,
                SourceTypeCode = source.SourceTypeCode,
                GeneratedAt = source.GeneratedAt,
                Farms = source.Farms.Select(MapFarm).ToArray(),
                Workers = source.Workers.Select(item => new NpcMovementMapper().Map(item)).ToArray(),
            };
        }

        private FarmSnapshot MapFarm(FarmApiModel source)
        {
            if (source == null || !StableDataId.IsValid(source.StableId)
                || string.IsNullOrWhiteSpace(source.FarmName) || source.Revision < 0
                || source.Plots == null)
            {
                throw new InvalidOperationException("FarmInvalid");
            }

            EnsureUnique(source.Plots.Select(item => item?.StableId), "FarmPlotStableIdDuplicate");
            return new FarmSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                FarmName = source.FarmName,
                StatusCode = source.StatusCode,
                Plots = source.Plots.Select(MapPlot).ToArray(),
            };
        }

        private FarmPlotSnapshot MapPlot(FarmPlotApiModel source)
        {
            if (source == null || !StableDataId.IsValid(source.StableId)
                || string.IsNullOrWhiteSpace(source.PlotName) || source.Revision < 0
                || source.Cultivations == null || source.Sensors == null)
            {
                throw new InvalidOperationException("FarmPlotInvalid");
            }

            EnsureUnique(source.Cultivations.Select(item => item?.StableId), "CultivationStableIdDuplicate");
            EnsureUnique(source.Sensors.Select(item => item?.StableId), "FarmSensorStableIdDuplicate");
            return new FarmPlotSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                PlotName = source.PlotName,
                SoilManagementProfileCode = source.SoilManagementProfileCode,
                Cultivations = source.Cultivations.Select(MapCultivation).ToArray(),
                Sensors = source.Sensors.Select(MapSensor).ToArray(),
            };
        }

        private static FarmCultivationSnapshot MapCultivation(FarmCultivationApiModel source)
        {
            if (source == null || !StableDataId.IsValid(source.StableId)
                || source.Revision < 0 || string.IsNullOrWhiteSpace(source.CropName)
                || string.IsNullOrWhiteSpace(source.GrowthStatusCode)
                || (string.IsNullOrWhiteSpace(source.CropReferenceStableId)
                    != string.IsNullOrWhiteSpace(source.CropReferenceSourceKey)))
            {
                throw new InvalidOperationException("FarmCultivationInvalid");
            }

            return new FarmCultivationSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                CropName = source.CropName,
                CropReferenceStableId = source.CropReferenceStableId,
                CropReferenceSourceKey = source.CropReferenceSourceKey,
                GrowthStatusCode = source.GrowthStatusCode,
                PlantedOn = source.PlantedOn,
                ExpectedHarvestOn = source.ExpectedHarvestOn,
            };
        }

        private FarmSensorSnapshot MapSensor(FarmSensorApiModel source)
        {
            if (source == null || !StableDataId.IsValid(source.StableId)
                || source.Revision < 0 || string.IsNullOrWhiteSpace(source.SensorTypeCode)
                || string.IsNullOrWhiteSpace(source.StatusCode))
            {
                throw new InvalidOperationException("FarmSensorInvalid");
            }

            return new FarmSensorSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                SensorTypeCode = source.SensorTypeCode,
                StatusCode = source.StatusCode,
                LatestObservation = MapObservation(source.LatestObservation),
            };
        }

        private FarmSensorObservationSnapshot? MapObservation(FarmSensorObservationApiModel? source)
        {
            if (source == null)
            {
                return null;
            }

            if (source.ObservedAt == default || string.IsNullOrWhiteSpace(source.UnitCode)
                || string.IsNullOrWhiteSpace(source.FreshnessStatusCode)
                || !Conditions.Contains(source.ConditionCode)
                || string.IsNullOrWhiteSpace(source.AssessmentRuleRevision))
            {
                throw new InvalidOperationException("FarmSensorObservationInvalid");
            }

            return new FarmSensorObservationSnapshot
            {
                Value = source.Value,
                UnitCode = source.UnitCode,
                ObservedAt = source.ObservedAt,
                FreshnessStatusCode = source.FreshnessStatusCode,
                ConditionCode = source.ConditionCode,
                AssessmentRuleRevision = source.AssessmentRuleRevision,
                EvidenceCardId = source.EvidenceCardId,
                ConfidenceCode = source.ConfidenceCode,
                Limitation = source.Limitation,
            };
        }

        private static void EnsureUnique(IEnumerable<string?> values, string errorCode)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (!StableDataId.IsValid(value) || !seen.Add(value!))
                {
                    throw new InvalidOperationException(errorCode);
                }
            }
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface IFarmProducerPerspectiveRepository
    {
        Task<FarmProducerPerspectiveSnapshot> LoadAsync(
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class FarmProducerPerspectiveApiRepository : IFarmProducerPerspectiveRepository
    {
        private readonly IFarmProducerPerspectiveApiClient apiClient;
        private readonly FarmProducerPerspectiveMapper mapper;

        public FarmProducerPerspectiveApiRepository(
            IFarmProducerPerspectiveApiClient client,
            FarmProducerPerspectiveMapper modelMapper)
        {
            apiClient = client;
            mapper = modelMapper;
        }

        public async Task<FarmProducerPerspectiveSnapshot> LoadAsync(
            CancellationToken cancellationToken = default)
            => mapper.Map(await apiClient.GetAsync(cancellationToken).ConfigureAwait(false));
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class FarmProducerPerspectiveQueryUseCase
    {
        private readonly IFarmProducerPerspectiveRepository repository;

        public FarmProducerPerspectiveQueryUseCase(IFarmProducerPerspectiveRepository value)
        {
            repository = value;
        }

        public Task<FarmProducerPerspectiveSnapshot> 실행Async(
            CancellationToken cancellationToken = default)
            => repository.LoadAsync(cancellationToken);
    }

    public interface IFarmPlotTarget
    {
        string StableId { get; }
        void Apply(FarmPlotSnapshot plot);
        void Hide();
    }

    public interface IFarmCultivationTarget
    {
        string StableId { get; }
        void Apply(FarmCultivationSnapshot cultivation);
        void Hide();
    }

    public interface IFarmSensorTarget
    {
        string StableId { get; }
        void Apply(FarmSensorSnapshot sensor);
        void Hide();
    }

    public sealed class FarmProducerPerspectiveApplicator
    {
        private long lastRevision = -1;

        public string[] Apply(
            FarmProducerPerspectiveSnapshot snapshot,
            IReadOnlyCollection<IFarmPlotTarget> plots,
            IReadOnlyCollection<IFarmCultivationTarget> cultivations,
            IReadOnlyCollection<IFarmSensorTarget> sensors,
            IReadOnlyList<INpcMovementTarget> workers)
        {
            if (snapshot.Revision < lastRevision)
            {
                return Array.Empty<string>();
            }

            var unresolved = new List<string>();
            ApplyTargets(
                snapshot.Farms.SelectMany(farm => farm.Plots),
                plots,
                item => item.StableId,
                target => target.StableId,
                (target, item) => target.Apply(item),
                target => target.Hide(),
                unresolved);
            unresolved.AddRange(new NpcMovementApplicator().Apply(snapshot.Workers, workers));
            ApplyTargets(
                snapshot.Farms.SelectMany(farm => farm.Plots).SelectMany(plot => plot.Cultivations),
                cultivations,
                item => item.StableId,
                target => target.StableId,
                (target, item) => target.Apply(item),
                target => target.Hide(),
                unresolved);
            ApplyTargets(
                snapshot.Farms.SelectMany(farm => farm.Plots).SelectMany(plot => plot.Sensors),
                sensors,
                item => item.StableId,
                target => target.StableId,
                (target, item) => target.Apply(item),
                target => target.Hide(),
                unresolved);
            lastRevision = snapshot.Revision;
            return unresolved.ToArray();
        }

        private static void ApplyTargets<TData, TTarget>(
            IEnumerable<TData> data,
            IReadOnlyCollection<TTarget> targets,
            Func<TData, string> id,
            Func<TTarget, string> targetId,
            Action<TTarget, TData> apply,
            Action<TTarget> hide,
            ICollection<string> unresolved)
        {
            var targetById = targets.ToDictionary(
                targetId,
                StringComparer.Ordinal);
            var visible = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in data)
            {
                var stableId = id(item);
                visible.Add(stableId);
                if (targetById.TryGetValue(stableId, out var target))
                {
                    apply(target, item);
                }
                else
                {
                    unresolved.Add(stableId);
                }
            }

            foreach (var target in targets)
            {
                var stableId = targetId(target);
                if (!visible.Contains(stableId))
                {
                    hide(target);
                }
            }
        }
    }
}
