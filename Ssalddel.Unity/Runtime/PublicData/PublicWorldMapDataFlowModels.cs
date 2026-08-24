using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.PublicData
{
    public static class PublicWorldMapDataFlowVersions
    {
        public const string InterpreterContract = "public-world-map-interpretation-v1";
        public const string RuleSet = "public-observation-layer-v1";
        public const string VisualRule = "public-marker-visual-v1";
        public const string PresentationContract = "public-data-hall-presentation-v1";
        public const string Perspective = "PublicObserver";
    }

    public sealed class PublicWorldMapLayerData
    {
        public string Code { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string MarkerShape { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapMetricData
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapObservationData
    {
        public string StableId { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceAsOfUtc { get; set; }
        public string EvidenceStatusCode { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string LocationPrecisionCode { get; set; } = string.Empty;
        public string MarkerStatusCode { get; set; } = string.Empty;
        public string FreshnessCode { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public PublicWorldMapMetricData[] Metrics { get; set; } = Array.Empty<PublicWorldMapMetricData>();
    }

    public sealed class PublicWorldMapDataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public PublicWorldMapLayerData[] Layers { get; set; } = Array.Empty<PublicWorldMapLayerData>();
        public PublicWorldMapObservationData[] Observations { get; set; } = Array.Empty<PublicWorldMapObservationData>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PublicWorldMapDataMapper
    {
        public PublicWorldMapDataSnapshot Map(PublicWorldMapSnapshotApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Require(source.DatasetCode, "PublicWorldMapDatasetMissing");
            Require(source.Revision, "PublicWorldMapRevisionMissing");
            if (source.GeneratedAtUtc == default) throw new InvalidOperationException("PublicWorldMapGeneratedAtMissing");
            if (source.Layers == null || source.Observations == null)
                throw new InvalidOperationException("PublicWorldMapCollectionsMissing");

            var layers = source.Layers.Select(layer =>
            {
                if (layer == null) throw new InvalidOperationException("PublicWorldMapLayerMissing");
                Require(layer.Code, "PublicWorldMapLayerCodeMissing");
                if (!string.Equals(layer.DatasetCode, source.DatasetCode, StringComparison.Ordinal))
                    throw new InvalidOperationException("PublicWorldMapLayerDatasetMismatch:" + layer.Code);
                return new PublicWorldMapLayerData
                {
                    Code = layer.Code.Trim(), DatasetCode = layer.DatasetCode.Trim(),
                    DisplayName = layer.DisplayName?.Trim() ?? string.Empty,
                    Description = layer.Description?.Trim() ?? string.Empty,
                    Color = layer.Color?.Trim() ?? string.Empty,
                    MarkerShape = layer.MarkerShape?.Trim() ?? string.Empty,
                };
            }).ToArray();
            RejectDuplicates(layers.Select(value => value.Code), "DuplicatePublicWorldMapLayer:");
            var layerCodes = new HashSet<string>(layers.Select(value => value.Code), StringComparer.Ordinal);

            RejectDuplicates(source.Observations.Select(value => value?.StableId), "DuplicatePublicWorldMapObservation:");
            var observations = source.Observations.Select(value => MapObservation(source.DatasetCode, layerCodes, value)).ToArray();
            return new PublicWorldMapDataSnapshot
            {
                StableId = "public-world-map:" + source.DatasetCode.Trim(),
                DatasetCode = source.DatasetCode.Trim(),
                DataRevision = source.Revision.Trim(),
                GeneratedAtUtc = source.GeneratedAtUtc,
                Layers = layers,
                Observations = observations,
            };
        }

        private static PublicWorldMapObservationData MapObservation(
            string datasetCode, ISet<string> layerCodes, PublicWorldMapObservationApiModel source)
        {
            if (source == null) throw new InvalidOperationException("PublicWorldMapObservationMissing");
            if (!StableDataId.IsValid(source.StableId))
                throw new InvalidOperationException("PublicWorldMapStableIdInvalid:" + source.StableId);
            if (!string.Equals(source.DatasetCode, datasetCode, StringComparison.Ordinal))
                throw new InvalidOperationException("PublicWorldMapObservationDatasetMismatch:" + source.StableId);
            if (!layerCodes.Contains(source.LayerCode))
                throw new InvalidOperationException("PublicWorldMapObservationLayerUnknown:" + source.StableId);
            if (source.Latitude < -90d || source.Latitude > 90d || source.Longitude < -180d || source.Longitude > 180d)
                throw new InvalidOperationException("PublicWorldMapCoordinatesInvalid:" + source.StableId);
            Require(source.Title, "PublicWorldMapTitleMissing");
            Require(source.SourceName, "PublicWorldMapSourceMissing");
            Require(source.EvidenceStatusCode, "PublicWorldMapEvidenceStatusMissing");
            Require(source.DetailHref, "PublicWorldMapDetailHrefMissing");

            return new PublicWorldMapObservationData
            {
                StableId = source.StableId.Trim(), DatasetCode = source.DatasetCode.Trim(), LayerCode = source.LayerCode.Trim(),
                CountryCode = source.CountryCode?.Trim() ?? string.Empty, CountryName = source.CountryName?.Trim() ?? string.Empty,
                Latitude = source.Latitude, Longitude = source.Longitude, Title = source.Title.Trim(),
                Summary = source.Summary?.Trim() ?? string.Empty, SourceName = source.SourceName.Trim(),
                EvidenceAsOfUtc = source.EvidenceAsOfUtc, EvidenceStatusCode = source.EvidenceStatusCode.Trim(),
                DetailHref = source.DetailHref.Trim(), SourceHref = source.SourceHref?.Trim() ?? string.Empty,
                LocationPrecisionCode = source.LocationPrecisionCode?.Trim() ?? string.Empty,
                MarkerStatusCode = source.MarkerStatusCode?.Trim() ?? string.Empty,
                FreshnessCode = source.FreshnessCode?.Trim() ?? string.Empty,
                BoundaryNotice = source.BoundaryNotice?.Trim() ?? string.Empty,
                SourceVersion = source.SourceVersion?.Trim() ?? string.Empty,
                Metrics = (source.Metrics ?? Array.Empty<PublicWorldMapMetricApiModel>()).Select(metric => new PublicWorldMapMetricData
                {
                    Code = metric.Code?.Trim() ?? string.Empty, DisplayName = metric.DisplayName?.Trim() ?? string.Empty,
                    Value = metric.Value, Unit = metric.Unit?.Trim() ?? string.Empty,
                }).ToArray(),
            };
        }

        private static void RejectDuplicates(IEnumerable<string?> values, string prefix)
        {
            var duplicate = values.Where(value => !string.IsNullOrWhiteSpace(value))
                .GroupBy(value => value!, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException(prefix + duplicate.Key);
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface IPublicWorldMapDataRepository
    {
        Task<PublicWorldMapDataSnapshot> 조회Async(PublicWorldMapQuery query, CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class PublicWorldMapApiDataRepository : IPublicWorldMapDataRepository
    {
        private readonly IPublicWorldMapApiClient apiClient;
        private readonly PublicWorldMapDataMapper mapper;
        public PublicWorldMapApiDataRepository(IPublicWorldMapApiClient apiClient, PublicWorldMapDataMapper mapper)
        { this.apiClient = apiClient; this.mapper = mapper; }

        public async Task<PublicWorldMapDataSnapshot> 조회Async(PublicWorldMapQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.DatasetCode))
                throw new ArgumentException("PublicWorldMapQueryInvalid", nameof(query));
            var snapshot = mapper.Map(await apiClient.GetAsync(query, cancellationToken).ConfigureAwait(false));
            if (!string.Equals(snapshot.DatasetCode, query.DatasetCode, StringComparison.Ordinal))
                throw new InvalidOperationException("PublicWorldMapQueryDatasetMismatch");
            return snapshot;
        }
    }

    public sealed class PublicWorldMapInterpreter
    {
        public PublicWorldMapSnapshot Interpret(PublicWorldMapDataSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(source.StableId, source.DataRevision, source.GeneratedAtUtc),
            });
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs, PublicWorldMapDataFlowVersions.InterpreterContract,
                PublicWorldMapDataFlowVersions.RuleSet, source.DatasetCode);
            return new PublicWorldMapSnapshot
            {
                DatasetCode = source.DatasetCode, Revision = source.DataRevision,
                GeneratedAtUtc = source.GeneratedAtUtc,
                Layers = source.Layers.Select(value => new PublicWorldMapLayerApiModel
                {
                    Code = value.Code, DatasetCode = value.DatasetCode, DisplayName = value.DisplayName,
                    Description = value.Description, Color = value.Color, MarkerShape = value.MarkerShape,
                }).ToArray(),
                Observations = source.Observations.Select(value => new PublicWorldMapObservation
                {
                    StableId = value.StableId, DatasetCode = value.DatasetCode, LayerCode = value.LayerCode,
                    CountryCode = value.CountryCode, CountryName = value.CountryName,
                    Latitude = value.Latitude, Longitude = value.Longitude, Title = value.Title, Summary = value.Summary,
                    SourceName = value.SourceName, EvidenceAsOfUtc = value.EvidenceAsOfUtc,
                    EvidenceStatusCode = value.EvidenceStatusCode, DetailHref = value.DetailHref,
                    SourceHref = value.SourceHref, LocationPrecisionCode = value.LocationPrecisionCode,
                    MarkerStatusCode = value.MarkerStatusCode, FreshnessCode = value.FreshnessCode,
                    BoundaryNotice = value.BoundaryNotice, SourceVersion = value.SourceVersion,
                }).ToArray(),
                Lineage = new InterpretationLineage(
                    inputs, PublicWorldMapDataFlowVersions.InterpreterContract,
                    PublicWorldMapDataFlowVersions.RuleSet, interpretationRevision),
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PublicWorldMapDataFlowQueryUseCase
    {
        private readonly IPublicWorldMapDataRepository repository;
        private readonly PublicWorldMapInterpreter interpreter;
        public PublicWorldMapDataFlowQueryUseCase(IPublicWorldMapDataRepository repository, PublicWorldMapInterpreter interpreter)
        { this.repository = repository; this.interpreter = interpreter; }
        public async Task<PublicWorldMapSnapshot> 실행Async(PublicWorldMapQuery query, CancellationToken cancellationToken = default)
            => interpreter.Interpret(await repository.조회Async(query, cancellationToken).ConfigureAwait(false));
    }
}
