using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Npcs
{
    public static class NpcMovementApiRoutes
    {
        public const string DriverUrbanLogisticsCenter =
            "api/v1/driver/world/zones/urban-logistics-center/perspective/npc-movement";
    }

    public static class NpcMovementSourceTypeCodes
    {
        public const string OperationalProjection = "OperationalProjection";
        public const string SimulatedFixture = "SimulatedFixture";
    }

    public static class NpcMovementStateCodes
    {
        public const string Idle = "Idle";
        public const string Moving = "Moving";
        public const string PerformingAction = "PerformingAction";
        public const string Waiting = "Waiting";
        public const string Stale = "Stale";
    }

    public sealed class NpcMovementApiModel
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string NpcStableId { get; set; } = string.Empty;

        public string ActorRoleCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;

        public string RouteCode { get; set; } = string.Empty;

        public string CurrentWaypointKey { get; set; } = string.Empty;

        public string DestinationWaypointKey { get; set; } = string.Empty;

        public string MovementStateCode { get; set; } = string.Empty;

        public string ArrivalActionCode { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public string CanonicalTaskStableId { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }
    }

    public sealed class NpcMovementSnapshot
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string NpcStableId { get; set; } = string.Empty;

        public string ActorRoleCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;

        public string RouteCode { get; set; } = string.Empty;

        public string CurrentWaypointKey { get; set; } = string.Empty;

        public string DestinationWaypointKey { get; set; } = string.Empty;

        public string MovementStateCode { get; set; } = string.Empty;

        public string ArrivalActionCode { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public string CanonicalTaskStableId { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }
    }

    public sealed class NpcMovementMapper
    {
        private static readonly HashSet<string> States = new HashSet<string>(StringComparer.Ordinal)
        {
            NpcMovementStateCodes.Idle,
            NpcMovementStateCodes.Moving,
            NpcMovementStateCodes.PerformingAction,
            NpcMovementStateCodes.Waiting,
            NpcMovementStateCodes.Stale,
        };

        public NpcMovementSnapshot Map(NpcMovementApiModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            RequireStableId(source.StableId, "MovementStableIdInvalid");
            RequireStableId(source.NpcStableId, "NpcStableIdInvalid");
            if (source.Revision < 0)
            {
                throw new InvalidOperationException("MovementRevisionInvalid");
            }

            Require(source.ActorRoleCode, "ActorRoleMissing");
            Require(source.WorldZoneCode, "WorldZoneMissing");
            Require(source.RouteCode, "RouteCodeMissing");
            Require(source.CurrentWaypointKey, "CurrentWaypointMissing");
            Require(source.DestinationWaypointKey, "DestinationWaypointMissing");
            if (!States.Contains(source.MovementStateCode))
            {
                throw new InvalidOperationException("MovementStateInvalid");
            }

            if (source.GeneratedAt == default)
            {
                throw new InvalidOperationException("MovementGeneratedAtMissing");
            }

            var route = ZoneNpcRouteCatalog.Find(source.RouteCode)
                ?? throw new InvalidOperationException("NpcRouteUnknown:" + source.RouteCode);
            if (!string.Equals(route.WorldZoneCode, source.WorldZoneCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("NpcRouteZoneMismatch");
            }

            if (!string.Equals(route.ActorRoleCode, source.ActorRoleCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("NpcRouteActorRoleMismatch");
            }

            if (!route.WaypointKeys.Contains(source.CurrentWaypointKey, StringComparer.Ordinal)
                || !route.WaypointKeys.Contains(source.DestinationWaypointKey, StringComparer.Ordinal))
            {
                throw new InvalidOperationException("NpcWaypointOutsideRoute");
            }

            if (string.Equals(source.MovementStateCode, NpcMovementStateCodes.Moving, StringComparison.Ordinal)
                && string.Equals(source.CurrentWaypointKey, source.DestinationWaypointKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MovingNpcDestinationUnchanged");
            }

            var operational = string.Equals(
                source.SourceTypeCode,
                NpcMovementSourceTypeCodes.OperationalProjection,
                StringComparison.Ordinal);
            var simulated = string.Equals(
                source.SourceTypeCode,
                NpcMovementSourceTypeCodes.SimulatedFixture,
                StringComparison.Ordinal);
            if (!operational && !simulated)
            {
                throw new InvalidOperationException("MovementSourceTypeInvalid");
            }

            if (operational && !StableDataId.IsValid(source.CanonicalTaskStableId))
            {
                throw new InvalidOperationException("OperationalNpcCanonicalTaskMissing");
            }

            if (simulated && !string.IsNullOrWhiteSpace(source.CanonicalTaskStableId))
            {
                throw new InvalidOperationException("SimulatedNpcMustNotClaimCanonicalTask");
            }

            return new NpcMovementSnapshot
            {
                StableId = source.StableId.Trim(),
                Revision = source.Revision,
                NpcStableId = source.NpcStableId.Trim(),
                ActorRoleCode = source.ActorRoleCode.Trim(),
                WorldZoneCode = source.WorldZoneCode.Trim(),
                RouteCode = source.RouteCode.Trim(),
                CurrentWaypointKey = source.CurrentWaypointKey.Trim(),
                DestinationWaypointKey = source.DestinationWaypointKey.Trim(),
                MovementStateCode = source.MovementStateCode.Trim(),
                ArrivalActionCode = source.ArrivalActionCode?.Trim() ?? string.Empty,
                SourceTypeCode = source.SourceTypeCode.Trim(),
                CanonicalTaskStableId = source.CanonicalTaskStableId?.Trim() ?? string.Empty,
                GeneratedAt = source.GeneratedAt,
            };
        }

        private static void RequireStableId(string value, string error)
        {
            if (!StableDataId.IsValid(value))
            {
                throw new InvalidOperationException(error + ":" + value);
            }
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(error);
            }
        }
    }

    public interface INpcMovementTarget
    {
        string NpcStableId { get; }

        void ApplyMovement(NpcMovementSnapshot snapshot);
    }

    public sealed class NpcMovementApplicator
    {
        public string[] Apply(
            IReadOnlyList<NpcMovementSnapshot> snapshots,
            IReadOnlyList<INpcMovementTarget> targets)
        {
            if (snapshots == null)
            {
                throw new ArgumentNullException(nameof(snapshots));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            var targetMap = new Dictionary<string, INpcMovementTarget>(StringComparer.Ordinal);
            foreach (var target in targets)
            {
                if (target == null || !StableDataId.IsValid(target.NpcStableId))
                {
                    throw new InvalidOperationException("NpcMovementTargetInvalid");
                }

                if (!targetMap.TryAdd(target.NpcStableId, target))
                {
                    throw new InvalidOperationException("DuplicateNpcMovementTarget:" + target.NpcStableId);
                }
            }

            var unresolved = new List<string>();
            var appliedNpcIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var snapshot in snapshots)
            {
                if (snapshot == null)
                {
                    throw new InvalidOperationException("NpcMovementSnapshotMissing");
                }

                if (!appliedNpcIds.Add(snapshot.NpcStableId))
                {
                    throw new InvalidOperationException(
                        "DuplicateNpcMovementSnapshot:" + snapshot.NpcStableId);
                }

                if (targetMap.TryGetValue(snapshot.NpcStableId, out var target))
                {
                    target.ApplyMovement(snapshot);
                }
                else
                {
                    unresolved.Add(snapshot.NpcStableId);
                }
            }

            return unresolved.ToArray();
        }
    }

    public sealed class NpcMovementQuery
    {
        public string WorldZoneCode { get; set; } = string.Empty;
    }

    public interface INpcMovementApiClient
    {
        Task<NpcMovementApiModel?> GetAsync(
            NpcMovementQuery query,
            CancellationToken cancellationToken = default);
    }

    public interface INpcMovementRepository
    {
        Task<NpcMovementSnapshot?> 조회Async(
            NpcMovementQuery query,
            CancellationToken cancellationToken = default);
    }

    public sealed class NpcMovementApiRepository : INpcMovementRepository
    {
        private readonly INpcMovementApiClient apiClient;
        private readonly NpcMovementMapper mapper;

        public NpcMovementApiRepository(
            INpcMovementApiClient apiClient,
            NpcMovementMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<NpcMovementSnapshot?> 조회Async(
            NpcMovementQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrWhiteSpace(query.WorldZoneCode))
            {
                throw new ArgumentException("WorldZoneMissing", nameof(query));
            }

            var source = await apiClient.GetAsync(query, cancellationToken).ConfigureAwait(false);
            if (source == null)
            {
                return null;
            }

            var snapshot = mapper.Map(source);
            if (!string.Equals(snapshot.WorldZoneCode, query.WorldZoneCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("NpcMovementZoneMismatch");
            }

            return snapshot;
        }
    }

    public sealed class NpcMovementQueryUseCase
    {
        private readonly INpcMovementRepository repository;

        public NpcMovementQueryUseCase(INpcMovementRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<NpcMovementSnapshot?> 실행Async(
            NpcMovementQuery query,
            CancellationToken cancellationToken = default)
        {
            return repository.조회Async(query, cancellationToken);
        }
    }
}
