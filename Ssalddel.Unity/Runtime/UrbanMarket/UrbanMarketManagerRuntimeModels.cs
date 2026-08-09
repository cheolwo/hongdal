using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.UrbanMarket
{
    public sealed class 도심마트ManagerRefreshRequest
    {
        public static readonly 도심마트ManagerRefreshRequest Instance = new 도심마트ManagerRefreshRequest();

        private 도심마트ManagerRefreshRequest()
        {
        }
    }

    public sealed class 도심마트ManagerSurfaceChangeSet
    {
        public StableIdChangeSet<도심마트ManagerSummarySurface> ManagerSummary { get; set; } = new StableIdChangeSet<도심마트ManagerSummarySurface>();
        public StableIdChangeSet<도심마트PriorityQueueSurfaceItem> PriorityQueue { get; set; } = new StableIdChangeSet<도심마트PriorityQueueSurfaceItem>();
        public StableIdChangeSet<도심마트ShelfSurfaceItem> Shelves { get; set; } = new StableIdChangeSet<도심마트ShelfSurfaceItem>();
        public StableIdChangeSet<도심마트TaskMarkerSurfaceItem> TaskMarkers { get; set; } = new StableIdChangeSet<도심마트TaskMarkerSurfaceItem>();
        public StableIdChangeSet<도심마트SourcePlanSurfaceItem> SourcePlans { get; set; } = new StableIdChangeSet<도심마트SourcePlanSurfaceItem>();
        public StableIdChangeSet<도심마트DetailPanelSurfaceItem> Details { get; set; } = new StableIdChangeSet<도심마트DetailPanelSurfaceItem>();
    }

    public sealed class 도심마트ManagerRuntimeResult
    {
        public ZoneRuntimeStatus Status { get; set; } = new ZoneRuntimeStatus();
        public 도심마트운영DataSnapshot? Data { get; set; }
        public 도심마트운영업무WorldState? SharedWorld { get; set; }
        public 마트관리자PerspectiveWorldState? PerspectiveWorld { get; set; }
        public 도심마트PresentationSnapshot? Presentation { get; set; }
        public 도심마트ManagerSurfaceChangeSet? Changes { get; set; }
        public WorldStableId? SelectedWorldId { get; set; }
    }

    public sealed class 도심마트PresentationChangeSetCalculator :
        IPresentationChangeSetCalculator<도심마트PresentationSnapshot, 도심마트ManagerSurfaceChangeSet>
    {
        public 도심마트ManagerSurfaceChangeSet Calculate(
            도심마트PresentationSnapshot? current,
            도심마트PresentationSnapshot incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));
            return new 도심마트ManagerSurfaceChangeSet
            {
                ManagerSummary = Reconcile(
                    current?.ManagerSummary == null
                        ? Array.Empty<도심마트ManagerSummarySurface>()
                        : new[] { current.ManagerSummary },
                    new[] { incoming.ManagerSummary },
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
                PriorityQueue = Reconcile(
                    current?.PriorityQueue ?? Array.Empty<도심마트PriorityQueueSurfaceItem>(),
                    incoming.PriorityQueue,
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
                Shelves = Reconcile(
                    current?.Shelves ?? Array.Empty<도심마트ShelfSurfaceItem>(),
                    incoming.Shelves,
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
                TaskMarkers = Reconcile(
                    current?.TaskMarkers ?? Array.Empty<도심마트TaskMarkerSurfaceItem>(),
                    incoming.TaskMarkers,
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
                SourcePlans = Reconcile(
                    current?.SourcePlans ?? Array.Empty<도심마트SourcePlanSurfaceItem>(),
                    incoming.SourcePlans,
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
                Details = Reconcile(
                    current?.Details ?? Array.Empty<도심마트DetailPanelSurfaceItem>(),
                    incoming.Details,
                    value => value.StableId.Value,
                    value => value.PresentationRevision),
            };
        }

        private static StableIdChangeSet<T> Reconcile<T>(
            IEnumerable<T> current,
            IEnumerable<T> incoming,
            Func<T, string> stableId,
            Func<T, string> revision)
            => new StableIdReconciler<T>(new StableIdReconciliationPolicy<T>(
                    stableId,
                    presentationRevision: revision))
                .Reconcile(current, incoming);
    }

    public sealed class 도심마트ManagerRuntime
    {
        private readonly SelectionStateStore selection;
        private readonly 도심마트ManagerPresentationContext presentationContext;
        private readonly WorldReadRuntime<
            도심마트ManagerRefreshRequest,
            도심마트운영DataSnapshot,
            도심마트SharedInterpretationContext,
            도심마트운영업무WorldState,
            InterpretationPerspectiveContext,
            마트관리자PerspectiveWorldState,
            도심마트ManagerPresentationContext,
            도심마트PresentationSnapshot,
            도심마트ManagerSurfaceChangeSet> runtime;
        private HashSet<WorldStableId> availableWorldIds = new HashSet<WorldStableId>();
        private 도심마트운영업무WorldState? lastSharedWorld;

        public 도심마트ManagerRuntime(
            I도심마트운영DataQuery query,
            도심마트운영업무SharedWorldInterpreter sharedInterpreter,
            마트관리자PerspectiveInterpreter perspectiveInterpreter,
            도심마트PresentationProjector projector,
            도심마트PresentationChangeSetCalculator changeSetCalculator,
            SelectionStateStore selection,
            도심마트ManagerPresentationContext? presentationContext = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (sharedInterpreter == null) throw new ArgumentNullException(nameof(sharedInterpreter));
            this.selection = selection ?? throw new ArgumentNullException(nameof(selection));
            this.presentationContext = presentationContext ?? new 도심마트ManagerPresentationContext();
            runtime = new WorldReadRuntime<
                도심마트ManagerRefreshRequest,
                도심마트운영DataSnapshot,
                도심마트SharedInterpretationContext,
                도심마트운영업무WorldState,
                InterpretationPerspectiveContext,
                마트관리자PerspectiveWorldState,
                도심마트ManagerPresentationContext,
                도심마트PresentationSnapshot,
                도심마트ManagerSurfaceChangeSet>(
                new ContextualDataQueryAdapter(query),
                new SharedInterpreterAdapter(sharedInterpreter),
                perspectiveInterpreter ?? throw new ArgumentNullException(nameof(perspectiveInterpreter)),
                projector ?? throw new ArgumentNullException(nameof(projector)),
                changeSetCalculator ?? throw new ArgumentNullException(nameof(changeSetCalculator)));
        }

        public ZoneRuntimeStatus CurrentStatus => runtime.CurrentStatus;
        public WorldStableId? SelectedWorldId => selection.SelectedWorldId;

        public async Task<도심마트ManagerRuntimeResult> RefreshAsync(
            WorldDataQueryContext dataContext,
            CancellationToken cancellationToken = default)
        {
            if (dataContext == null) throw new ArgumentNullException(nameof(dataContext));
            selection.SetAuthorizationScope(dataContext.CacheBoundaryKey);
            var selectedBeforeRefresh = selection.SelectedWorldId;
            var result = await runtime.RefreshDataAsync(
                    도심마트ManagerRefreshRequest.Instance,
                    도심마트SharedInterpretationContext.Operations(),
                    world => PerspectiveContext(world, FocusIfPresent(world, selectedBeforeRefresh)),
                    presentationContext,
                    dataContext,
                    cancellationToken)
                .ConfigureAwait(false);

            if (result.Status.StateCode == ZoneRuntimeStateCode.Ready && result.SharedWorld != null)
            {
                lastSharedWorld = result.SharedWorld;
                availableWorldIds = result.SharedWorld.SharedWorld.Graph.NodesById.Keys.ToHashSet();
                if (selection.SelectedWorldId.HasValue
                    && !availableWorldIds.Contains(selection.SelectedWorldId.Value))
                {
                    selection.Clear();
                }
            }

            return Map(result);
        }

        public 도심마트ManagerRuntimeResult Select(WorldStableId worldId)
        {
            if (lastSharedWorld == null) throw new InvalidOperationException("UrbanMarketManagerRuntimeNotReady");
            if (!availableWorldIds.Contains(worldId))
                throw new InvalidOperationException("UrbanMarketManagerSelectionUnknown:" + worldId.Value);
            var previous = selection.SelectedWorldId;
            selection.Select(worldId);
            var result = runtime.ReinterpretPerspective(
                PerspectiveContext(lastSharedWorld, worldId),
                presentationContext);
            RestoreSelectionAfterFailure(result.Status, previous);
            return Map(result);
        }

        public 도심마트ManagerRuntimeResult ClearSelection()
        {
            if (lastSharedWorld == null) throw new InvalidOperationException("UrbanMarketManagerRuntimeNotReady");
            var previous = selection.SelectedWorldId;
            selection.Clear();
            var result = runtime.ReinterpretPerspective(
                PerspectiveContext(lastSharedWorld, null),
                presentationContext);
            RestoreSelectionAfterFailure(result.Status, previous);
            return Map(result);
        }

        private void RestoreSelectionAfterFailure(ZoneRuntimeStatus status, WorldStableId? previous)
        {
            if (status.StateCode == ZoneRuntimeStateCode.Ready) return;
            selection.Clear();
            if (previous.HasValue) selection.Select(previous.Value);
        }

        private WorldStableId? FocusIfPresent(
            도심마트운영업무WorldState world,
            WorldStableId? selected)
            => selected.HasValue && world.SharedWorld.Graph.NodesById.ContainsKey(selected.Value)
                ? selected
                : null;

        private static InterpretationPerspectiveContext PerspectiveContext(
            도심마트운영업무WorldState world,
            WorldStableId? focus)
            => new InterpretationPerspectiveContext(
                마트관리자PerspectiveCodes.Role,
                마트관리자PerspectiveCodes.ReviewReplenishment,
                마트관리자PerspectiveCodes.Zone,
                world.SharedWorld.Mode == DataRuntimeMode.Operational
                    ? WorldInterpretationMode.Operational
                    : WorldInterpretationMode.Simulation,
                focus);

        private 도심마트ManagerRuntimeResult Map(
            WorldReadRuntimeResult<
                도심마트운영DataSnapshot,
                도심마트운영업무WorldState,
                마트관리자PerspectiveWorldState,
                도심마트PresentationSnapshot,
                도심마트ManagerSurfaceChangeSet> source)
            => new 도심마트ManagerRuntimeResult
            {
                Status = source.Status,
                Data = source.Data,
                SharedWorld = source.SharedWorld,
                PerspectiveWorld = source.PerspectiveWorld,
                Presentation = source.Presentation,
                Changes = source.Changes,
                SelectedWorldId = selection.SelectedWorldId,
            };

        private sealed class ContextualDataQueryAdapter :
            IContextualWorldDataQuery<도심마트ManagerRefreshRequest, 도심마트운영DataSnapshot>
        {
            private readonly I도심마트운영DataQuery query;

            public ContextualDataQueryAdapter(I도심마트운영DataQuery query)
                => this.query = query;

            public Task<도심마트운영DataSnapshot> QueryAsync(
                도심마트ManagerRefreshRequest request,
                WorldDataQueryContext context,
                CancellationToken cancellationToken = default)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                if (context.ScopeKind != DataScopeKind.AuthorizedUserWorld)
                    throw new InvalidOperationException("UrbanMarketManagerAuthorizedWorldScopeRequired");
                if (!string.Equals(context.DatasetKey, 도심마트DataSetKeys.ManagerOperations, StringComparison.Ordinal))
                    throw new InvalidOperationException("UrbanMarketManagerDatasetMismatch");
                if (context.Authorization == null
                    || !context.Authorization.HasRole(마트관리자PerspectiveCodes.Role))
                {
                    throw new InvalidOperationException("UrbanMarketManagerRoleNotApproved");
                }
                return query.조회Async(cancellationToken);
            }
        }

        private sealed class SharedInterpreterAdapter :
            ISharedWorldInterpreter<
                도심마트운영DataSnapshot,
                도심마트SharedInterpretationContext,
                도심마트운영업무WorldState>
        {
            private readonly 도심마트운영업무SharedWorldInterpreter interpreter;

            public SharedInterpreterAdapter(도심마트운영업무SharedWorldInterpreter interpreter)
                => this.interpreter = interpreter;

            public 도심마트운영업무WorldState Interpret(
                도심마트운영DataSnapshot data,
                도심마트SharedInterpretationContext context)
                => interpreter.Interpret(data, context);
        }
    }
}
