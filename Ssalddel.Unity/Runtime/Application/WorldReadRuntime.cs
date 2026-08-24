using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.Application
{
    public enum ZoneRuntimeStateCode
    {
        Idle,
        InitialLoading,
        Ready,
        Refreshing,
        Reinterpreting,
        PerspectiveInterpreting,
        Reprojecting,
        InitialError,
        RefreshError,
    }

    public enum WorldInterpretationMode
    {
        Operational,
        Simulation,
    }

    /// <summary>
    /// 서버 authorization을 대신하지 않고, 이미 허용된 SharedWorldState를
    /// 현재 역할·목적·Zone·대상의 의미로 축약하는 입력입니다.
    /// </summary>
    public sealed class InterpretationPerspectiveContext
    {
        public InterpretationPerspectiveContext(
            string roleCode,
            string intentCode,
            string zoneCode,
            WorldInterpretationMode mode,
            WorldStableId? focusWorldId = null)
        {
            RoleCode = Require(roleCode, nameof(roleCode));
            IntentCode = Require(intentCode, nameof(intentCode));
            ZoneCode = Require(zoneCode, nameof(zoneCode));
            Mode = mode;
            if (focusWorldId.HasValue && !focusWorldId.Value.IsDefined)
                throw new ArgumentException("FocusWorldStableIdMissing", nameof(focusWorldId));
            FocusWorldId = focusWorldId;
        }

        public string RoleCode { get; }
        public string IntentCode { get; }
        public string ZoneCode { get; }
        public WorldStableId? FocusWorldId { get; }
        public WorldInterpretationMode Mode { get; }

        private static string Require(string value, string name)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", name)
                : value.Trim();
    }

    public sealed class ZoneRuntimeStatus
    {
        public ZoneRuntimeStateCode StateCode { get; set; } = ZoneRuntimeStateCode.Idle;
        public string SafeErrorCode { get; set; } = string.Empty;
        public bool IsShowingLastSuccess { get; set; }
        public string AuthorizationScopeKey { get; set; } = string.Empty;
    }

    public interface IWorldDataQuery<in TQuery, TData>
    {
        Task<TData> QueryAsync(TQuery query, CancellationToken cancellationToken = default);
    }

    public interface ISharedWorldInterpreter<in TData, in TContext, out TSharedWorld>
    {
        TSharedWorld Interpret(TData data, TContext context);
    }

    public interface IPerspectiveInterpreter<in TSharedWorld, in TContext, out TPerspectiveWorld>
    {
        TPerspectiveWorld Interpret(TSharedWorld world, TContext context);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface IPresentationProjector<in TPerspectiveWorld, in TContext, out TPresentation>
    {
        TPresentation Project(TPerspectiveWorld world, TContext context);
    }

    public interface IPresentationChangeSetCalculator<TPresentation, out TChangeSet>
    {
        TChangeSet Calculate(TPresentation? current, TPresentation incoming);
    }

    public interface IRuntimeErrorClassifier
    {
        string Classify(Exception error);
    }

    public sealed class SafeRuntimeErrorClassifier : IRuntimeErrorClassifier
    {
        public string Classify(Exception error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            return error is TimeoutException ? "Timeout" : "UnexpectedError";
        }
    }

    public sealed class WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>
        where TData : class
        where TSharedWorld : class
        where TPerspectiveWorld : class
        where TPresentation : class
    {
        public ZoneRuntimeStatus Status { get; set; } = new ZoneRuntimeStatus();
        public TData? Data { get; set; }
        public TSharedWorld? SharedWorld { get; set; }
        public TPerspectiveWorld? PerspectiveWorld { get; set; }
        public TPresentation? Presentation { get; set; }
        public TChangeSet? Changes { get; set; }
    }

    /// <summary>
    /// Authorized Data 조회, 공통 World 해석, 관점 해석과 Presentation 투영의
    /// 생명주기만 조율합니다. 각 단계의 의미·시각 정책은 해당 port가 담당합니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class WorldReadRuntime<
        TQuery,
        TData,
        TSharedContext,
        TSharedWorld,
        TPerspectiveContext,
        TPerspectiveWorld,
        TPresentationContext,
        TPresentation,
        TChangeSet>
        where TData : class
        where TSharedWorld : class
        where TPerspectiveWorld : class
        where TPresentation : class
    {
        private readonly IWorldDataQuery<TQuery, TData>? query;
        private readonly IContextualWorldDataQuery<TQuery, TData>? contextualQuery;
        private readonly ISharedWorldInterpreter<TData, TSharedContext, TSharedWorld> sharedInterpreter;
        private readonly IPerspectiveInterpreter<TSharedWorld, TPerspectiveContext, TPerspectiveWorld> perspectiveInterpreter;
        private readonly IPresentationProjector<TPerspectiveWorld, TPresentationContext, TPresentation> projector;
        private readonly IPresentationChangeSetCalculator<TPresentation, TChangeSet> changeSetCalculator;
        private readonly IRuntimeErrorClassifier errorClassifier;
        private TData? lastData;
        private TSharedWorld? lastSharedWorld;
        private TPerspectiveWorld? lastPerspectiveWorld;
        private TPresentation? lastPresentation;
        private string authorizationScopeKey = string.Empty;

        public WorldReadRuntime(
            IWorldDataQuery<TQuery, TData> query,
            ISharedWorldInterpreter<TData, TSharedContext, TSharedWorld> sharedInterpreter,
            IPerspectiveInterpreter<TSharedWorld, TPerspectiveContext, TPerspectiveWorld> perspectiveInterpreter,
            IPresentationProjector<TPerspectiveWorld, TPresentationContext, TPresentation> projector,
            IPresentationChangeSetCalculator<TPresentation, TChangeSet> changeSetCalculator,
            IRuntimeErrorClassifier? errorClassifier = null)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
            this.sharedInterpreter = sharedInterpreter ?? throw new ArgumentNullException(nameof(sharedInterpreter));
            this.perspectiveInterpreter = perspectiveInterpreter ?? throw new ArgumentNullException(nameof(perspectiveInterpreter));
            this.projector = projector ?? throw new ArgumentNullException(nameof(projector));
            this.changeSetCalculator = changeSetCalculator ?? throw new ArgumentNullException(nameof(changeSetCalculator));
            this.errorClassifier = errorClassifier ?? new SafeRuntimeErrorClassifier();
            CurrentStatus = Status(ZoneRuntimeStateCode.Idle, false, string.Empty);
        }

        public WorldReadRuntime(
            IContextualWorldDataQuery<TQuery, TData> query,
            ISharedWorldInterpreter<TData, TSharedContext, TSharedWorld> sharedInterpreter,
            IPerspectiveInterpreter<TSharedWorld, TPerspectiveContext, TPerspectiveWorld> perspectiveInterpreter,
            IPresentationProjector<TPerspectiveWorld, TPresentationContext, TPresentation> projector,
            IPresentationChangeSetCalculator<TPresentation, TChangeSet> changeSetCalculator,
            IRuntimeErrorClassifier? errorClassifier = null)
        {
            contextualQuery = query ?? throw new ArgumentNullException(nameof(query));
            this.sharedInterpreter = sharedInterpreter ?? throw new ArgumentNullException(nameof(sharedInterpreter));
            this.perspectiveInterpreter = perspectiveInterpreter ?? throw new ArgumentNullException(nameof(perspectiveInterpreter));
            this.projector = projector ?? throw new ArgumentNullException(nameof(projector));
            this.changeSetCalculator = changeSetCalculator ?? throw new ArgumentNullException(nameof(changeSetCalculator));
            this.errorClassifier = errorClassifier ?? new SafeRuntimeErrorClassifier();
            CurrentStatus = Status(ZoneRuntimeStateCode.Idle, false, string.Empty);
        }

        public ZoneRuntimeStatus CurrentStatus { get; private set; }

        public async Task<WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>> RefreshDataAsync(
            TQuery request,
            TSharedContext sharedContext,
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext,
            string authorizationScopeKey,
            CancellationToken cancellationToken = default)
        {
            var normalizedScope = RequireScope(authorizationScopeKey);
            if (!string.Equals(this.authorizationScopeKey, normalizedScope, StringComparison.Ordinal))
            {
                ClearLastSuccess();
                this.authorizationScopeKey = normalizedScope;
            }

            var refreshing = lastPresentation != null;
            CurrentStatus = Status(
                refreshing ? ZoneRuntimeStateCode.Refreshing : ZoneRuntimeStateCode.InitialLoading,
                refreshing,
                normalizedScope);

            try
            {
                if (query == null) throw new InvalidOperationException("LegacyWorldDataQueryMissing");
                var incomingData = await query.QueryAsync(request, cancellationToken).ConfigureAwait(false);
                return CommitAll(incomingData, sharedContext, perspectiveContext, presentationContext);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = lastPresentation == null
                    ? Status(ZoneRuntimeStateCode.Idle, false, normalizedScope)
                    : Status(ZoneRuntimeStateCode.Ready, true, normalizedScope);
                throw;
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public async Task<WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>> RefreshDataAsync(
            TQuery request,
            TSharedContext sharedContext,
            Func<TSharedWorld, TPerspectiveContext> perspectiveContextResolver,
            TPresentationContext presentationContext,
            WorldDataQueryContext dataContext,
            CancellationToken cancellationToken = default)
        {
            if (perspectiveContextResolver == null)
                throw new ArgumentNullException(nameof(perspectiveContextResolver));
            if (dataContext == null) throw new ArgumentNullException(nameof(dataContext));
            var normalizedScope = RequireScope(dataContext.CacheBoundaryKey);
            if (!string.Equals(authorizationScopeKey, normalizedScope, StringComparison.Ordinal))
            {
                ClearLastSuccess();
                authorizationScopeKey = normalizedScope;
            }

            var refreshing = lastPresentation != null;
            CurrentStatus = Status(
                refreshing ? ZoneRuntimeStateCode.Refreshing : ZoneRuntimeStateCode.InitialLoading,
                refreshing,
                normalizedScope);

            try
            {
                if (contextualQuery == null) throw new InvalidOperationException("ContextualWorldDataQueryMissing");
                var incomingData = await contextualQuery
                    .QueryAsync(request, dataContext, cancellationToken)
                    .ConfigureAwait(false);
                if (incomingData == null) throw new InvalidOperationException("WorldDataMissing");
                var incomingSharedWorld = sharedInterpreter.Interpret(incomingData, sharedContext)
                    ?? throw new InvalidOperationException("SharedWorldInterpretationMissing");
                var perspectiveContext = perspectiveContextResolver(incomingSharedWorld);
                if (perspectiveContext == null) throw new InvalidOperationException("PerspectiveContextMissing");
                return CommitAll(
                    incomingData,
                    incomingSharedWorld,
                    perspectiveContext,
                    presentationContext);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = lastPresentation == null
                    ? Status(ZoneRuntimeStateCode.Idle, false, normalizedScope)
                    : Status(ZoneRuntimeStateCode.Ready, true, normalizedScope);
                throw;
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public async Task<WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>> RefreshDataAsync(
            TQuery request,
            TSharedContext sharedContext,
            Func<TSharedWorld, TPerspectiveContext> perspectiveContextResolver,
            TPresentationContext presentationContext,
            string authorizationScopeKey,
            CancellationToken cancellationToken = default)
        {
            if (perspectiveContextResolver == null)
                throw new ArgumentNullException(nameof(perspectiveContextResolver));
            var normalizedScope = RequireScope(authorizationScopeKey);
            if (!string.Equals(this.authorizationScopeKey, normalizedScope, StringComparison.Ordinal))
            {
                ClearLastSuccess();
                this.authorizationScopeKey = normalizedScope;
            }

            var refreshing = lastPresentation != null;
            CurrentStatus = Status(
                refreshing ? ZoneRuntimeStateCode.Refreshing : ZoneRuntimeStateCode.InitialLoading,
                refreshing,
                normalizedScope);

            try
            {
                if (query == null) throw new InvalidOperationException("LegacyWorldDataQueryMissing");
                var incomingData = await query.QueryAsync(request, cancellationToken).ConfigureAwait(false);
                if (incomingData == null) throw new InvalidOperationException("WorldDataMissing");
                var incomingSharedWorld = sharedInterpreter.Interpret(incomingData, sharedContext)
                    ?? throw new InvalidOperationException("SharedWorldInterpretationMissing");
                var perspectiveContext = perspectiveContextResolver(incomingSharedWorld);
                if (perspectiveContext == null) throw new InvalidOperationException("PerspectiveContextMissing");
                return CommitAll(
                    incomingData,
                    incomingSharedWorld,
                    perspectiveContext,
                    presentationContext);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = lastPresentation == null
                    ? Status(ZoneRuntimeStateCode.Idle, false, normalizedScope)
                    : Status(ZoneRuntimeStateCode.Ready, true, normalizedScope);
                throw;
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public async Task<WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>> RefreshDataAsync(
            TQuery request,
            TSharedContext sharedContext,
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext,
            WorldDataQueryContext dataContext,
            CancellationToken cancellationToken = default)
        {
            if (dataContext == null) throw new ArgumentNullException(nameof(dataContext));
            var normalizedScope = RequireScope(dataContext.CacheBoundaryKey);
            if (!string.Equals(authorizationScopeKey, normalizedScope, StringComparison.Ordinal))
            {
                ClearLastSuccess();
                authorizationScopeKey = normalizedScope;
            }

            var refreshing = lastPresentation != null;
            CurrentStatus = Status(
                refreshing ? ZoneRuntimeStateCode.Refreshing : ZoneRuntimeStateCode.InitialLoading,
                refreshing,
                normalizedScope);

            try
            {
                if (contextualQuery == null) throw new InvalidOperationException("ContextualWorldDataQueryMissing");
                var incomingData = await contextualQuery
                    .QueryAsync(request, dataContext, cancellationToken)
                    .ConfigureAwait(false);
                return CommitAll(incomingData, sharedContext, perspectiveContext, presentationContext);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = lastPresentation == null
                    ? Status(ZoneRuntimeStateCode.Idle, false, normalizedScope)
                    : Status(ZoneRuntimeStateCode.Ready, true, normalizedScope);
                throw;
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> ReinterpretShared(
            TSharedContext sharedContext,
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext)
        {
            if (lastData == null) throw new InvalidOperationException("WorldReadRuntimeDataMissing");
            CurrentStatus = Status(ZoneRuntimeStateCode.Reinterpreting, true, authorizationScopeKey);
            try
            {
                return CommitAll(lastData, sharedContext, perspectiveContext, presentationContext);
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> ReinterpretPerspective(
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext)
        {
            if (lastSharedWorld == null || lastData == null)
                throw new InvalidOperationException("WorldReadRuntimeSharedWorldMissing");
            CurrentStatus = Status(ZoneRuntimeStateCode.PerspectiveInterpreting, true, authorizationScopeKey);
            try
            {
                var incomingPerspective = perspectiveInterpreter.Interpret(lastSharedWorld, perspectiveContext)
                    ?? throw new InvalidOperationException("PerspectiveWorldInterpretationMissing");
                var incomingPresentation = projector.Project(incomingPerspective, presentationContext)
                    ?? throw new InvalidOperationException("PresentationProjectionMissing");
                var changes = changeSetCalculator.Calculate(lastPresentation, incomingPresentation);
                lastPerspectiveWorld = incomingPerspective;
                lastPresentation = incomingPresentation;
                CurrentStatus = Status(ZoneRuntimeStateCode.Ready, true, authorizationScopeKey);
                return Result(changes);
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        public WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> Reproject(
            TPresentationContext presentationContext)
        {
            if (lastPerspectiveWorld == null || lastSharedWorld == null || lastData == null)
                throw new InvalidOperationException("WorldReadRuntimePerspectiveWorldMissing");
            CurrentStatus = Status(ZoneRuntimeStateCode.Reprojecting, true, authorizationScopeKey);
            try
            {
                var incomingPresentation = projector.Project(lastPerspectiveWorld, presentationContext)
                    ?? throw new InvalidOperationException("PresentationProjectionMissing");
                var changes = changeSetCalculator.Calculate(lastPresentation, incomingPresentation);
                lastPresentation = incomingPresentation;
                CurrentStatus = Status(ZoneRuntimeStateCode.Ready, true, authorizationScopeKey);
                return Result(changes);
            }
            catch (Exception error)
            {
                return Failure(error);
            }
        }

        private WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> CommitAll(
            TData incomingData,
            TSharedContext sharedContext,
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext)
        {
            if (incomingData == null) throw new InvalidOperationException("WorldDataMissing");
            var incomingSharedWorld = sharedInterpreter.Interpret(incomingData, sharedContext)
                ?? throw new InvalidOperationException("SharedWorldInterpretationMissing");
            return CommitAll(
                incomingData,
                incomingSharedWorld,
                perspectiveContext,
                presentationContext);
        }

        private WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> CommitAll(
            TData incomingData,
            TSharedWorld incomingSharedWorld,
            TPerspectiveContext perspectiveContext,
            TPresentationContext presentationContext)
        {
            var incomingPerspectiveWorld = perspectiveInterpreter.Interpret(incomingSharedWorld, perspectiveContext)
                ?? throw new InvalidOperationException("PerspectiveWorldInterpretationMissing");
            var incomingPresentation = projector.Project(incomingPerspectiveWorld, presentationContext)
                ?? throw new InvalidOperationException("PresentationProjectionMissing");
            var changes = changeSetCalculator.Calculate(lastPresentation, incomingPresentation);

            // 모든 단계가 성공한 뒤에만 last-success를 교체합니다.
            lastData = incomingData;
            lastSharedWorld = incomingSharedWorld;
            lastPerspectiveWorld = incomingPerspectiveWorld;
            lastPresentation = incomingPresentation;
            CurrentStatus = Status(ZoneRuntimeStateCode.Ready, true, authorizationScopeKey);
            return Result(changes);
        }

        private WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> Failure(Exception error)
        {
            var hasLastSuccess = lastPresentation != null;
            CurrentStatus = new ZoneRuntimeStatus
            {
                StateCode = hasLastSuccess ? ZoneRuntimeStateCode.RefreshError : ZoneRuntimeStateCode.InitialError,
                SafeErrorCode = errorClassifier.Classify(error),
                IsShowingLastSuccess = hasLastSuccess,
                AuthorizationScopeKey = authorizationScopeKey,
            };
            return Result(default);
        }

        private WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet> Result(TChangeSet? changes)
            => new WorldReadRuntimeResult<TData, TSharedWorld, TPerspectiveWorld, TPresentation, TChangeSet>
            {
                Status = CurrentStatus,
                Data = lastData,
                SharedWorld = lastSharedWorld,
                PerspectiveWorld = lastPerspectiveWorld,
                Presentation = lastPresentation,
                Changes = changes,
            };

        private void ClearLastSuccess()
        {
            lastData = null;
            lastSharedWorld = null;
            lastPerspectiveWorld = null;
            lastPresentation = null;
        }

        private static ZoneRuntimeStatus Status(
            ZoneRuntimeStateCode state,
            bool isShowingLastSuccess,
            string scope)
            => new ZoneRuntimeStatus
            {
                StateCode = state,
                IsShowingLastSuccess = isShowingLastSuccess,
                AuthorizationScopeKey = scope,
            };

        private static string RequireScope(string value)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("AuthorizationScopeKeyMissing", nameof(value))
                : value.Trim();
    }
}
