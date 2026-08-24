using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.PublicData
{
    public static class PublicWorldSurfaceVersions
    {
        public const string SharedInterpreter = "public-world-shared-v2";
        public const string SharedRule = "public-observation-semantics-v2";
        public const string PerspectiveInterpreter = "public-world-perspective-v1";
        public const string PerspectiveRule = "public-observer-focus-v1";
        public const string PresentationContract = "public-data-surfaces-v1";
        public const string VisualRule = "public-data-surface-visual-v1";
    }

    public sealed class PublicWorldInterpretationContext
    {
        public DateTimeOffset EvaluationTimeUtc { get; set; }
    }

    public sealed class PublicLayerWorldState
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed class PublicMetricWorldState
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public sealed class PublicObservationWorldState : IWorldNode
    {
        public WorldStableId StableId { get; set; }
        public WorldIdentityLineage Identity { get; set; } = null!;
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
        public string SemanticStatusCode { get; set; } = string.Empty;
        public string FreshnessCode { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public PublicMetricWorldState[] Metrics { get; set; } = Array.Empty<PublicMetricWorldState>();
    }

    public sealed class PublicWorldState
    {
        public WorldStableId StableId { get; set; }
        public string DatasetCode { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public PublicLayerWorldState[] Layers { get; set; } = Array.Empty<PublicLayerWorldState>();
        public PublicObservationWorldState[] Observations { get; set; } = Array.Empty<PublicObservationWorldState>();
        public InterpretationLineage Lineage { get; set; } = null!;
    }

    public sealed class PublicSharedWorldInterpreter :
        ISharedWorldInterpreter<PublicWorldMapDataSnapshot, PublicWorldInterpretationContext, PublicWorldState>
    {
        public PublicWorldState Interpret(
            PublicWorldMapDataSnapshot source,
            PublicWorldInterpretationContext context)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var evaluationTime = context.EvaluationTimeUtc == default
                ? source.GeneratedAtUtc
                : context.EvaluationTimeUtc;
            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(source.StableId, source.DataRevision, source.GeneratedAtUtc),
            });
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                PublicWorldSurfaceVersions.SharedInterpreter,
                PublicWorldSurfaceVersions.SharedRule,
                source.DatasetCode + "|" + evaluationTime.ToUniversalTime().ToString("O"));

            return new PublicWorldState
            {
                StableId = new WorldStableId(source.StableId),
                DatasetCode = source.DatasetCode,
                GeneratedAtUtc = source.GeneratedAtUtc,
                Layers = source.Layers.Select(value => new PublicLayerWorldState
                {
                    Code = value.Code,
                    DisplayName = value.DisplayName,
                    Description = value.Description,
                }).ToArray(),
                Observations = source.Observations.Select(value => Observation(value)).ToArray(),
                Lineage = new InterpretationLineage(
                    inputs,
                    PublicWorldSurfaceVersions.SharedInterpreter,
                    PublicWorldSurfaceVersions.SharedRule,
                    revision),
            };
        }

        private static PublicObservationWorldState Observation(PublicWorldMapObservationData source)
        {
            var sourceId = new SourceStableId(source.StableId);
            var worldId = new WorldStableId(source.StableId);
            return new PublicObservationWorldState
            {
                StableId = worldId,
                Identity = new WorldIdentityLineage(worldId, new[] { sourceId }),
                LayerCode = source.LayerCode,
                CountryCode = source.CountryCode,
                CountryName = source.CountryName,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                Title = source.Title,
                Summary = source.Summary,
                SourceName = source.SourceName,
                EvidenceAsOfUtc = source.EvidenceAsOfUtc,
                EvidenceStatusCode = source.EvidenceStatusCode,
                DetailHref = source.DetailHref,
                SourceHref = source.SourceHref,
                LocationPrecisionCode = source.LocationPrecisionCode,
                SemanticStatusCode = string.IsNullOrWhiteSpace(source.MarkerStatusCode)
                    ? source.FreshnessCode
                    : source.MarkerStatusCode,
                FreshnessCode = source.FreshnessCode,
                BoundaryNotice = source.BoundaryNotice,
                SourceVersion = source.SourceVersion,
                Metrics = source.Metrics.Select(value => new PublicMetricWorldState
                {
                    Code = value.Code,
                    DisplayName = value.DisplayName,
                    Value = value.Value,
                    Unit = value.Unit,
                }).ToArray(),
            };
        }
    }

    public sealed class PublicWorldPerspectiveState
    {
        public PublicWorldState SharedWorld { get; set; } = null!;
        public InterpretationPerspectiveContext Context { get; set; } = null!;
        public WorldStableId? FocusWorldId { get; set; }
        public string PerspectiveInterpretationRevision { get; set; } = string.Empty;
    }

    public sealed class PublicWorldPerspectiveInterpreter :
        IPerspectiveInterpreter<PublicWorldState, InterpretationPerspectiveContext, PublicWorldPerspectiveState>
    {
        public PublicWorldPerspectiveState Interpret(
            PublicWorldState world,
            InterpretationPerspectiveContext context)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.FocusWorldId.HasValue
                && world.Observations.All(value => value.StableId != context.FocusWorldId.Value))
                throw new InvalidOperationException("PublicPerspectiveFocusUnknown:" + context.FocusWorldId.Value.Value);

            var parameters = string.Join("|", new[]
            {
                context.RoleCode,
                context.IntentCode,
                context.ZoneCode,
                context.FocusWorldId?.Value ?? string.Empty,
                context.Mode.ToString(),
            });
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                world.Lineage.Inputs,
                PublicWorldSurfaceVersions.PerspectiveInterpreter,
                PublicWorldSurfaceVersions.PerspectiveRule,
                world.Lineage.InterpretationRevision + "|" + parameters);
            return new PublicWorldPerspectiveState
            {
                SharedWorld = world,
                Context = context,
                FocusWorldId = context.FocusWorldId,
                PerspectiveInterpretationRevision = revision,
            };
        }
    }

    public sealed class PublicDataHallPresentationContext
    {
        public string LocaleCode { get; set; } = "ko-KR";
        public string QualityTierCode { get; set; } = "Primitive";
    }

    public sealed class PublicMapMarkerPresentationItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LabelText { get; set; } = string.Empty;
        public string VisualStateCode { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public string ShapeCode { get; set; } = string.Empty;
    }

    public sealed class PublicMapLegendPresentationItem
    {
        public PresentationStableId StableId { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string DescriptionText { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public string ShapeCode { get; set; } = string.Empty;
    }

    public sealed class PublicMapHeatmapPresentationItem
    {
        public PresentationStableId StableId { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string LimitationCode { get; set; } = string.Empty;
    }

    public sealed class PublicMapDetailPresentationItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string MetricText { get; set; } = string.Empty;
        public string SourceText { get; set; } = string.Empty;
        public string AsOfText { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
    }

    public sealed class PublicDataHallSurfaceSnapshot
    {
        public string PresentationRevision { get; set; } = string.Empty;
        public PublicMapMarkerPresentationItem[] Markers { get; set; } = Array.Empty<PublicMapMarkerPresentationItem>();
        public PublicMapLegendPresentationItem[] Legends { get; set; } = Array.Empty<PublicMapLegendPresentationItem>();
        public PublicMapHeatmapPresentationItem[] Heatmaps { get; set; } = Array.Empty<PublicMapHeatmapPresentationItem>();
        public PublicMapDetailPresentationItem[] Details { get; set; } = Array.Empty<PublicMapDetailPresentationItem>();
    }

    public sealed class PublicDataHallSurfaceChangeSet
    {
        public StableIdChangeSet<PublicMapMarkerPresentationItem> Markers { get; set; } = new();
        public StableIdChangeSet<PublicMapLegendPresentationItem> Legends { get; set; } = new();
        public StableIdChangeSet<PublicMapHeatmapPresentationItem> Heatmaps { get; set; } = new();
        public StableIdChangeSet<PublicMapDetailPresentationItem> Details { get; set; } = new();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PublicDataHallVisualPolicy
    {
        private static readonly string[] Colors = { "Green", "Orange", "Blue", "Purple", "Gray" };
        private static readonly string[] Shapes = { "Circle", "Diamond", "Square", "Triangle" };

        public (string Color, string Shape) Resolve(string layerCode, IReadOnlyList<string> orderedLayerCodes)
        {
            var index = 0;
            for (var candidate = 0; candidate < orderedLayerCodes.Count; candidate++)
            {
                if (!string.Equals(orderedLayerCodes[candidate], layerCode, StringComparison.Ordinal)) continue;
                index = candidate;
                break;
            }
            return (Colors[index % Colors.Length], Shapes[index % Shapes.Length]);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PublicDataHallSurfaceProjector :
        IPresentationProjector<PublicWorldPerspectiveState, PublicDataHallPresentationContext, PublicDataHallSurfaceSnapshot>
    {
        private readonly PublicDataHallVisualPolicy visualPolicy;

        public PublicDataHallSurfaceProjector(PublicDataHallVisualPolicy visualPolicy)
            => this.visualPolicy = visualPolicy ?? throw new ArgumentNullException(nameof(visualPolicy));

        public PublicDataHallSurfaceSnapshot Project(
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context)
        {
            if (perspective == null) throw new ArgumentNullException(nameof(perspective));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var world = perspective.SharedWorld;
            var layers = world.Layers.OrderBy(value => value.Code, StringComparer.Ordinal).ToArray();
            var layerCodes = layers.Select(value => value.Code).ToArray();
            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                perspective.PerspectiveInterpretationRevision,
                perspective.Context.RoleCode + ":" + perspective.Context.IntentCode,
                PublicWorldSurfaceVersions.VisualRule + ":" + context.QualityTierCode,
                PublicWorldSurfaceVersions.PresentationContract + ":" + context.LocaleCode);

            var markers = world.Observations.Select(value => Marker(value, layerCodes, perspective, context)).ToArray();
            var legends = layers.Select(value => Legend(value, world.DatasetCode, layerCodes, perspective, context)).ToArray();
            var heatmap = Heatmap(world, perspective, context);
            var details = perspective.FocusWorldId.HasValue
                ? new[] { Detail(world.Observations.Single(value => value.StableId == perspective.FocusWorldId.Value), perspective, context) }
                : Array.Empty<PublicMapDetailPresentationItem>();
            return new PublicDataHallSurfaceSnapshot
            {
                PresentationRevision = presentationRevision,
                Markers = markers,
                Legends = legends,
                Heatmaps = new[] { heatmap },
                Details = details,
            };
        }

        private PublicMapMarkerPresentationItem Marker(
            PublicObservationWorldState source,
            IReadOnlyList<string> layerCodes,
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context)
        {
            var style = visualPolicy.Resolve(source.LayerCode, layerCodes);
            var id = new PresentationStableId("public-map-marker:" + source.StableId.Value);
            return new PublicMapMarkerPresentationItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, new[] { source.StableId }),
                PresentationRevision = ItemRevision(
                    "marker", perspective, context,
                    source.StableId.Value, source.Title, source.SourceName, source.SemanticStatusCode,
                    source.Latitude.ToString("R"), source.Longitude.ToString("R"), style.Color, style.Shape),
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                LabelText = source.Title + "\n" + source.SourceName,
                VisualStateCode = source.SemanticStatusCode,
                ColorCode = style.Color,
                ShapeCode = style.Shape,
            };
        }

        private PublicMapLegendPresentationItem Legend(
            PublicLayerWorldState source,
            string datasetCode,
            IReadOnlyList<string> layerCodes,
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context)
        {
            var style = visualPolicy.Resolve(source.Code, layerCodes);
            var id = new PresentationStableId("public-map-legend:" + datasetCode + ":" + source.Code);
            return new PublicMapLegendPresentationItem
            {
                StableId = id,
                PresentationRevision = ItemRevision(
                    "legend", perspective, context,
                    source.Code, source.DisplayName, source.Description, style.Color, style.Shape),
                LayerCode = source.Code,
                LabelText = source.DisplayName,
                DescriptionText = source.Description,
                ColorCode = style.Color,
                ShapeCode = style.Shape,
            };
        }

        private static PublicMapHeatmapPresentationItem Heatmap(
            PublicWorldState world,
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context)
        {
            var id = new PresentationStableId("public-map-heatmap:" + world.DatasetCode);
            return new PublicMapHeatmapPresentationItem
            {
                StableId = id,
                PresentationRevision = ItemRevision(
                    "heatmap", perspective, context, world.DatasetCode, "RegionGeometryMissing"),
                IsAvailable = false,
                LimitationCode = "RegionGeometryMissing",
            };
        }

        private static PublicMapDetailPresentationItem Detail(
            PublicObservationWorldState source,
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context)
        {
            var id = new PresentationStableId("public-map-detail:" + source.StableId.Value);
            var metrics = string.Join("\n", source.Metrics.Select(value =>
                value.DisplayName + " " + value.Value + " " + value.Unit));
            var asOf = source.EvidenceAsOfUtc?.ToString("O") ?? "기준시각 없음";
            return new PublicMapDetailPresentationItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, new[] { source.StableId }),
                PresentationRevision = ItemRevision(
                    "detail", perspective, context,
                    source.StableId.Value, source.Title, source.Summary, metrics,
                    source.SourceName, asOf, source.BoundaryNotice),
                TitleText = source.Title,
                SummaryText = source.Summary,
                MetricText = metrics,
                SourceText = source.SourceName,
                AsOfText = asOf,
                DetailHref = source.DetailHref,
                BoundaryNotice = source.BoundaryNotice,
            };
        }

        private static string ItemRevision(
            string surface,
            PublicWorldPerspectiveState perspective,
            PublicDataHallPresentationContext context,
            params string[] values)
            => WorldDataFlowRevisionCalculator.CalculatePresentation(
                "interpretation-item:" + string.Join("|", values),
                perspective.Context.RoleCode + ":" + perspective.Context.IntentCode,
                PublicWorldSurfaceVersions.VisualRule + ":" + surface + ":" + context.QualityTierCode,
                PublicWorldSurfaceVersions.PresentationContract + ":" + context.LocaleCode);
    }

    public sealed class PublicDataHallSurfaceChangeSetCalculator :
        IPresentationChangeSetCalculator<PublicDataHallSurfaceSnapshot, PublicDataHallSurfaceChangeSet>
    {
        public PublicDataHallSurfaceChangeSet Calculate(
            PublicDataHallSurfaceSnapshot? current,
            PublicDataHallSurfaceSnapshot incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));
            return new PublicDataHallSurfaceChangeSet
            {
                Markers = Reconcile(current?.Markers, incoming.Markers, value => value.StableId.Value, value => value.PresentationRevision),
                Legends = Reconcile(current?.Legends, incoming.Legends, value => value.StableId.Value, value => value.PresentationRevision),
                Heatmaps = Reconcile(current?.Heatmaps, incoming.Heatmaps, value => value.StableId.Value, value => value.PresentationRevision),
                Details = Reconcile(current?.Details, incoming.Details, value => value.StableId.Value, value => value.PresentationRevision),
            };
        }

        private static StableIdChangeSet<T> Reconcile<T>(
            IEnumerable<T>? current,
            IEnumerable<T> incoming,
            Func<T, string> id,
            Func<T, string> revision)
            => new StableIdReconciler<T>(new StableIdReconciliationPolicy<T>(
                    id,
                    presentationRevision: revision))
                .Reconcile(current ?? Array.Empty<T>(), incoming);
    }

    public sealed class PublicWorldMapRuntimeDataQuery :
        IWorldDataQuery<PublicWorldMapQuery, PublicWorldMapDataSnapshot>,
        IContextualWorldDataQuery<PublicWorldMapQuery, PublicWorldMapDataSnapshot>
    {
        private readonly IPublicWorldMapDataRepository repository;
        public PublicWorldMapRuntimeDataQuery(IPublicWorldMapDataRepository repository)
            => this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        public Task<PublicWorldMapDataSnapshot> QueryAsync(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default)
            => repository.조회Async(query, cancellationToken);

        public Task<PublicWorldMapDataSnapshot> QueryAsync(
            PublicWorldMapQuery query,
            WorldDataQueryContext context,
            CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.ScopeKind != DataScopeKind.Global)
                throw new InvalidOperationException("PublicWorldMapDataScopeMustBeGlobal");
            if (query == null || !string.Equals(context.DatasetKey, query.DatasetCode, StringComparison.Ordinal))
                throw new InvalidOperationException("PublicWorldMapDataSetContextMismatch");
            return repository.조회Async(query, cancellationToken);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PublicDataHallSurfaceRuntimeCoordinator
    {
        private readonly WorldReadRuntime<
            PublicWorldMapQuery,
            PublicWorldMapDataSnapshot,
            PublicWorldInterpretationContext,
            PublicWorldState,
            InterpretationPerspectiveContext,
            PublicWorldPerspectiveState,
            PublicDataHallPresentationContext,
            PublicDataHallSurfaceSnapshot,
            PublicDataHallSurfaceChangeSet> runtime;

        public PublicDataHallSurfaceRuntimeCoordinator(
            PublicWorldMapRuntimeDataQuery query,
            PublicSharedWorldInterpreter sharedInterpreter,
            PublicWorldPerspectiveInterpreter perspectiveInterpreter,
            PublicDataHallSurfaceProjector projector,
            PublicDataHallSurfaceChangeSetCalculator changeSetCalculator)
        {
            runtime = new WorldReadRuntime<
                PublicWorldMapQuery,
                PublicWorldMapDataSnapshot,
                PublicWorldInterpretationContext,
                PublicWorldState,
                InterpretationPerspectiveContext,
                PublicWorldPerspectiveState,
                PublicDataHallPresentationContext,
                PublicDataHallSurfaceSnapshot,
                PublicDataHallSurfaceChangeSet>(
                    (IContextualWorldDataQuery<PublicWorldMapQuery, PublicWorldMapDataSnapshot>)query,
                    sharedInterpreter,
                    perspectiveInterpreter,
                    projector,
                    changeSetCalculator);
        }

        public ZoneRuntimeStatus CurrentStatus => runtime.CurrentStatus;

        public Task<WorldReadRuntimeResult<
            PublicWorldMapDataSnapshot,
            PublicWorldState,
            PublicWorldPerspectiveState,
            PublicDataHallSurfaceSnapshot,
            PublicDataHallSurfaceChangeSet>> RefreshDataAsync(
                PublicWorldMapQuery query,
                PublicWorldInterpretationContext sharedContext,
                InterpretationPerspectiveContext perspectiveContext,
                PublicDataHallPresentationContext presentationContext,
            string authorizationScopeKey,
            CancellationToken cancellationToken = default)
            => runtime.RefreshDataAsync(
                query,
                sharedContext,
                perspectiveContext,
                presentationContext,
                WorldDataQueryContext.Global(
                    query.DatasetCode,
                    perspectiveContext.Mode == WorldInterpretationMode.Operational
                        ? DataRuntimeMode.Operational
                        : DataRuntimeMode.Simulation),
                cancellationToken);

        public Task<WorldReadRuntimeResult<
            PublicWorldMapDataSnapshot,
            PublicWorldState,
            PublicWorldPerspectiveState,
            PublicDataHallSurfaceSnapshot,
            PublicDataHallSurfaceChangeSet>> RefreshDataAsync(
                PublicWorldMapQuery query,
                PublicWorldInterpretationContext sharedContext,
                InterpretationPerspectiveContext perspectiveContext,
                PublicDataHallPresentationContext presentationContext,
                WorldDataQueryContext dataContext,
                CancellationToken cancellationToken = default)
            => runtime.RefreshDataAsync(
                query,
                sharedContext,
                perspectiveContext,
                presentationContext,
                dataContext,
                cancellationToken);
    }
}
