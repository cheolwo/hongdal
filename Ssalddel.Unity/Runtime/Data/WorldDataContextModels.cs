using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.Data
{
    public enum DataScopeKind
    {
        Global,
        World,
        AuthorizedUser,
        AuthorizedUserWorld,
    }

    public enum DataRuntimeMode
    {
        Operational,
        Simulation,
    }

    public enum WorldDataContextTransitionKind
    {
        Activated,
        Unchanged,
        SessionChanged,
        WorldChanged,
        AuthorizationChanged,
        ModeChanged,
        LoggedOut,
    }

    public readonly struct SessionScopeId : IEquatable<SessionScopeId>
    {
        public SessionScopeId(string value) => Value = Require(value, "SessionScopeIdMissing");
        public string Value { get; }
        public bool Equals(SessionScopeId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is SessionScopeId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(SessionScopeId left, SessionScopeId right) => left.Equals(right);
        public static bool operator !=(SessionScopeId left, SessionScopeId right) => !left.Equals(right);

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public readonly struct WorldContextId : IEquatable<WorldContextId>
    {
        public WorldContextId(string value) => Value = Require(value, "WorldContextIdMissing");
        public string Value { get; }
        public bool Equals(WorldContextId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is WorldContextId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(WorldContextId left, WorldContextId right) => left.Equals(right);
        public static bool operator !=(WorldContextId left, WorldContextId right) => !left.Equals(right);

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public readonly struct AuthorizationScopeId : IEquatable<AuthorizationScopeId>
    {
        public AuthorizationScopeId(string value) => Value = Require(value, "AuthorizationScopeIdMissing");
        public string Value { get; }
        public bool Equals(AuthorizationScopeId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is AuthorizationScopeId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(AuthorizationScopeId left, AuthorizationScopeId right) => left.Equals(right);
        public static bool operator !=(AuthorizationScopeId left, AuthorizationScopeId right) => !left.Equals(right);

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public sealed class UserSessionContext
    {
        public UserSessionContext(SessionScopeId sessionScopeId, string authorizedIdentityHandle)
        {
            SessionScopeId = sessionScopeId;
            AuthorizedIdentityHandle = Require(authorizedIdentityHandle, "AuthorizedIdentityHandleMissing");
        }

        public SessionScopeId SessionScopeId { get; }

        /// <summary>서버가 발급한 불투명 식별 handle이며 사용자 ID나 권한 증명이 아닙니다.</summary>
        public string AuthorizedIdentityHandle { get; }

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public sealed class WorldContext
    {
        public WorldContext(WorldContextId worldId, string worldRevision, DataRuntimeMode mode)
        {
            WorldId = worldId;
            WorldRevision = Require(worldRevision, "WorldRevisionMissing");
            Mode = mode;
        }

        public WorldContextId WorldId { get; }
        public string WorldRevision { get; }
        public DataRuntimeMode Mode { get; }

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public sealed class DataAuthorizationContext
    {
        private readonly HashSet<string> approvedRoles;
        private readonly HashSet<string> capabilities;

        public DataAuthorizationContext(
            AuthorizationScopeId scopeId,
            IEnumerable<string> approvedRoleCodes,
            IEnumerable<string> capabilityCodes,
            string authorizationRevision)
        {
            ScopeId = scopeId;
            approvedRoles = Normalize(approvedRoleCodes, "ApprovedRolesMissing");
            capabilities = Normalize(capabilityCodes, "CapabilitiesMissing", allowEmpty: true);
            AuthorizationRevision = Require(authorizationRevision, "AuthorizationRevisionMissing");
        }

        public AuthorizationScopeId ScopeId { get; }
        public IReadOnlyCollection<string> ApprovedRoleCodes => approvedRoles;
        public IReadOnlyCollection<string> CapabilityCodes => capabilities;
        public string AuthorizationRevision { get; }
        public bool HasRole(string roleCode) => approvedRoles.Contains(roleCode?.Trim() ?? string.Empty);
        public bool HasCapability(string capabilityCode) => capabilities.Contains(capabilityCode?.Trim() ?? string.Empty);

        private static HashSet<string> Normalize(IEnumerable<string> values, string error, bool allowEmpty = false)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var result = new HashSet<string>(
                values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()),
                StringComparer.Ordinal);
            if (!allowEmpty && result.Count == 0) throw new ArgumentException(error, nameof(values));
            return result;
        }

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    /// <summary>
    /// 로그인 사용자 ID를 신뢰 근거로 사용하지 않고 서버가 승인한 session, World와
    /// authorization scope를 하나의 Data access boundary로 묶습니다.
    /// </summary>
    public sealed class WorldDataContext
    {
        public WorldDataContext(
            UserSessionContext session,
            WorldContext world,
            DataAuthorizationContext authorization)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            World = world ?? throw new ArgumentNullException(nameof(world));
            Authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        }

        public UserSessionContext Session { get; }
        public WorldContext World { get; }
        public DataAuthorizationContext Authorization { get; }
        public string BoundaryKey => string.Join("|", new[]
        {
            Session.SessionScopeId.Value,
            World.WorldId.Value,
            Authorization.ScopeId.Value,
            Authorization.AuthorizationRevision,
            World.Mode.ToString(),
        });
    }

    public sealed class WorldDataQueryContext
    {
        private WorldDataQueryContext(
            DataScopeKind scopeKind,
            string datasetKey,
            DataRuntimeMode mode,
            UserSessionContext? session,
            WorldContext? world,
            DataAuthorizationContext? authorization)
        {
            ScopeKind = scopeKind;
            DatasetKey = Require(datasetKey, "DataSetKeyMissing");
            Mode = mode;
            Session = session;
            World = world;
            Authorization = authorization;
            Validate();
            CacheBoundaryKey = BuildBoundaryKey();
        }

        public DataScopeKind ScopeKind { get; }
        public string DatasetKey { get; }
        public DataRuntimeMode Mode { get; }
        public UserSessionContext? Session { get; }
        public WorldContext? World { get; }
        public DataAuthorizationContext? Authorization { get; }
        public string CacheBoundaryKey { get; }

        public static WorldDataQueryContext Global(string datasetKey, DataRuntimeMode mode)
            => new WorldDataQueryContext(DataScopeKind.Global, datasetKey, mode, null, null, null);

        public static WorldDataQueryContext ForWorld(string datasetKey, WorldContext world)
            => new WorldDataQueryContext(
                DataScopeKind.World,
                datasetKey,
                (world ?? throw new ArgumentNullException(nameof(world))).Mode,
                null,
                world,
                null);

        public static WorldDataQueryContext ForAuthorizedUser(
            string datasetKey,
            DataRuntimeMode mode,
            UserSessionContext session,
            DataAuthorizationContext authorization)
            => new WorldDataQueryContext(
                DataScopeKind.AuthorizedUser,
                datasetKey,
                mode,
                session ?? throw new ArgumentNullException(nameof(session)),
                null,
                authorization ?? throw new ArgumentNullException(nameof(authorization)));

        public static WorldDataQueryContext ForAuthorizedUserWorld(string datasetKey, WorldDataContext context)
            => new WorldDataQueryContext(
                DataScopeKind.AuthorizedUserWorld,
                datasetKey,
                (context ?? throw new ArgumentNullException(nameof(context))).World.Mode,
                context.Session,
                context.World,
                context.Authorization);

        private void Validate()
        {
            if ((ScopeKind == DataScopeKind.World || ScopeKind == DataScopeKind.AuthorizedUserWorld)
                && World == null)
                throw new InvalidOperationException("WorldDataQueryWorldMissing");
            if ((ScopeKind == DataScopeKind.AuthorizedUser || ScopeKind == DataScopeKind.AuthorizedUserWorld)
                && (Session == null || Authorization == null))
                throw new InvalidOperationException("WorldDataQueryAuthorizationMissing");
            if (World != null && World.Mode != Mode)
                throw new InvalidOperationException("WorldDataQueryModeMismatch");
        }

        private string BuildBoundaryKey()
        {
            var parts = new List<string> { ScopeKind.ToString(), Mode.ToString(), DatasetKey };
            if (Session != null) parts.Add(Session.SessionScopeId.Value);
            if (World != null) parts.Add(World.WorldId.Value);
            if (Authorization != null)
            {
                parts.Add(Authorization.ScopeId.Value);
                parts.Add(Authorization.AuthorizationRevision);
            }
            return string.Join("|", parts);
        }

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(error, nameof(value)) : value.Trim();
    }

    public readonly struct WorldDataCacheKey : IEquatable<WorldDataCacheKey>
    {
        public WorldDataCacheKey(WorldDataQueryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            ScopeKind = context.ScopeKind;
            Value = context.CacheBoundaryKey;
        }

        public DataScopeKind ScopeKind { get; }
        public string Value { get; }
        public bool Equals(WorldDataCacheKey other)
            => ScopeKind == other.ScopeKind && string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is WorldDataCacheKey other && Equals(other);
        public override int GetHashCode()
            => ((int)ScopeKind * 397) ^ StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
    }

    public sealed class WorldDataCacheEntry<TSnapshot> where TSnapshot : class
    {
        public TSnapshot Snapshot { get; set; } = null!;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset CachedAtUtc { get; set; }
    }

    public interface IWorldDataContextState
    {
        void HandleContextTransition(WorldDataContextTransition transition);
    }

    public sealed class ContextScopedSnapshotCache<TSnapshot> : IWorldDataContextState
        where TSnapshot : class
    {
        private readonly Dictionary<WorldDataCacheKey, WorldDataCacheEntry<TSnapshot>> entries =
            new Dictionary<WorldDataCacheKey, WorldDataCacheEntry<TSnapshot>>();

        public int Count => entries.Count;

        public void Store(
            WorldDataQueryContext context,
            TSnapshot snapshot,
            string revision,
            DateTimeOffset cachedAtUtc)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (string.IsNullOrWhiteSpace(revision)) throw new ArgumentException("DataRevisionMissing", nameof(revision));
            if (cachedAtUtc == default) throw new ArgumentException("CachedAtMissing", nameof(cachedAtUtc));
            entries[new WorldDataCacheKey(context)] = new WorldDataCacheEntry<TSnapshot>
            {
                Snapshot = snapshot,
                Revision = revision.Trim(),
                CachedAtUtc = cachedAtUtc,
            };
        }

        public bool TryGet(WorldDataQueryContext context, out WorldDataCacheEntry<TSnapshot>? entry)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return entries.TryGetValue(new WorldDataCacheKey(context), out entry);
        }

        public void HandleContextTransition(WorldDataContextTransition transition)
        {
            if (transition == null) throw new ArgumentNullException(nameof(transition));
            switch (transition.Kind)
            {
                case WorldDataContextTransitionKind.SessionChanged:
                case WorldDataContextTransitionKind.LoggedOut:
                    RemoveScopes(DataScopeKind.World, DataScopeKind.AuthorizedUser, DataScopeKind.AuthorizedUserWorld);
                    break;
                case WorldDataContextTransitionKind.WorldChanged:
                case WorldDataContextTransitionKind.ModeChanged:
                    RemoveScopes(DataScopeKind.World, DataScopeKind.AuthorizedUserWorld);
                    break;
                case WorldDataContextTransitionKind.AuthorizationChanged:
                    RemoveScopes(DataScopeKind.AuthorizedUser, DataScopeKind.AuthorizedUserWorld);
                    break;
            }
        }

        public void Clear() => entries.Clear();

        private void RemoveScopes(params DataScopeKind[] scopes)
        {
            var scopeSet = new HashSet<DataScopeKind>(scopes);
            foreach (var key in entries.Keys.Where(key => scopeSet.Contains(key.ScopeKind)).ToArray())
                entries.Remove(key);
        }
    }

    public sealed class WorldDataContextTransition
    {
        public WorldDataContextTransition(
            WorldDataContextTransitionKind kind,
            WorldDataContext? previous,
            WorldDataContext? current)
        {
            Kind = kind;
            Previous = previous;
            Current = current;
        }

        public WorldDataContextTransitionKind Kind { get; }
        public WorldDataContext? Previous { get; }
        public WorldDataContext? Current { get; }
    }

    /// <summary>
    /// 서버가 승인해 전달한 WorldDataContext의 수명만 관리합니다.
    /// 이 Runtime은 역할이나 capability를 새로 부여하지 않습니다.
    /// </summary>
    public sealed class WorldDataContextRuntime
    {
        private readonly IWorldDataContextState[] states;

        public WorldDataContextRuntime(params IWorldDataContextState[] states)
            => this.states = states ?? Array.Empty<IWorldDataContextState>();

        public WorldDataContext? Current { get; private set; }

        public WorldDataContextTransition Activate(WorldDataContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var previous = Current;
            var kind = Detect(previous, context);
            Current = context;
            var transition = new WorldDataContextTransition(kind, previous, context);
            Notify(transition);
            return transition;
        }

        public WorldDataContextTransition Logout()
        {
            var previous = Current;
            Current = null;
            var transition = new WorldDataContextTransition(
                WorldDataContextTransitionKind.LoggedOut,
                previous,
                null);
            Notify(transition);
            return transition;
        }

        private static WorldDataContextTransitionKind Detect(WorldDataContext? previous, WorldDataContext current)
        {
            if (previous == null) return WorldDataContextTransitionKind.Activated;
            if (previous.Session.SessionScopeId != current.Session.SessionScopeId)
                return WorldDataContextTransitionKind.SessionChanged;
            if (previous.World.WorldId != current.World.WorldId)
                return WorldDataContextTransitionKind.WorldChanged;
            if (previous.World.Mode != current.World.Mode)
                return WorldDataContextTransitionKind.ModeChanged;
            if (previous.Authorization.ScopeId != current.Authorization.ScopeId
                || !string.Equals(
                    previous.Authorization.AuthorizationRevision,
                    current.Authorization.AuthorizationRevision,
                    StringComparison.Ordinal))
                return WorldDataContextTransitionKind.AuthorizationChanged;
            return WorldDataContextTransitionKind.Unchanged;
        }

        private void Notify(WorldDataContextTransition transition)
        {
            foreach (var state in states) state.HandleContextTransition(transition);
        }
    }

    public interface IContextualWorldDataQuery<in TQuery, TData>
    {
        Task<TData> QueryAsync(
            TQuery query,
            WorldDataQueryContext context,
            CancellationToken cancellationToken = default);
    }
}
