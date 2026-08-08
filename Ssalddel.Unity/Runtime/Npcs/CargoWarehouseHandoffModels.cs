using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Npcs
{
    public static class CargoWarehouseHandoffApiRoutes
    {
        public const string DriverWarehouseHandoff =
            "api/v1/driver/world/workflows/warehouse-handoff";
    }

    public static class CargoHandoffStateCodes
    {
        public const string InTransit = "InTransit";
        public const string ArrivedAtWarehouse = "ArrivedAtWarehouse";
        public const string ReceivingCompleted = "ReceivingCompleted";
    }

    public sealed class CargoWarehouseHandoffApiModel
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string HandoffStateCode { get; set; } = string.Empty;

        public string CargoStableId { get; set; } = string.Empty;

        public string TransportTaskStableId { get; set; } = string.Empty;

        public string InboundTaskStableId { get; set; } = string.Empty;

        public NpcMovementApiModel[] Movements { get; set; } = Array.Empty<NpcMovementApiModel>();

        public DateTimeOffset GeneratedAt { get; set; }
    }

    public sealed class CargoWarehouseHandoffSnapshot
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string HandoffStateCode { get; set; } = string.Empty;

        public string CargoStableId { get; set; } = string.Empty;

        public string TransportTaskStableId { get; set; } = string.Empty;

        public string InboundTaskStableId { get; set; } = string.Empty;

        public NpcMovementSnapshot[] Movements { get; set; } = Array.Empty<NpcMovementSnapshot>();

        public DateTimeOffset GeneratedAt { get; set; }
    }

    public sealed class CargoWarehouseHandoffMapper
    {
        private static readonly HashSet<string> States = new HashSet<string>(StringComparer.Ordinal)
        {
            CargoHandoffStateCodes.InTransit,
            CargoHandoffStateCodes.ArrivedAtWarehouse,
            CargoHandoffStateCodes.ReceivingCompleted,
        };

        private readonly NpcMovementMapper movementMapper;

        public CargoWarehouseHandoffMapper(NpcMovementMapper movementMapper)
        {
            this.movementMapper = movementMapper ?? throw new ArgumentNullException(nameof(movementMapper));
        }

        public CargoWarehouseHandoffSnapshot Map(CargoWarehouseHandoffApiModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            RequireStableId(source.StableId, "CargoHandoffStableIdInvalid");
            RequireStableId(source.CargoStableId, "CargoStableIdInvalid");
            RequireStableId(source.TransportTaskStableId, "TransportTaskStableIdInvalid");
            RequireStableId(source.InboundTaskStableId, "InboundTaskStableIdInvalid");
            if (source.Revision < 0)
            {
                throw new InvalidOperationException("CargoHandoffRevisionInvalid");
            }

            if (!States.Contains(source.HandoffStateCode))
            {
                throw new InvalidOperationException("CargoHandoffStateInvalid");
            }

            if (source.GeneratedAt == default)
            {
                throw new InvalidOperationException("CargoHandoffGeneratedAtMissing");
            }

            if (source.Movements == null || source.Movements.Length == 0)
            {
                throw new InvalidOperationException("CargoHandoffMovementsMissing");
            }

            var movements = source.Movements.Select(movementMapper.Map).ToArray();
            if (movements.Select(item => item.NpcStableId).Distinct(StringComparer.Ordinal).Count()
                != movements.Length)
            {
                throw new InvalidOperationException("CargoHandoffDuplicateNpc");
            }

            ValidatePhase(source.HandoffStateCode, movements);

            return new CargoWarehouseHandoffSnapshot
            {
                StableId = source.StableId.Trim(),
                Revision = source.Revision,
                HandoffStateCode = source.HandoffStateCode.Trim(),
                CargoStableId = source.CargoStableId.Trim(),
                TransportTaskStableId = source.TransportTaskStableId.Trim(),
                InboundTaskStableId = source.InboundTaskStableId.Trim(),
                Movements = movements,
                GeneratedAt = source.GeneratedAt,
            };
        }

        private static void ValidatePhase(
            string stateCode,
            IReadOnlyList<NpcMovementSnapshot> movements)
        {
            if (string.Equals(stateCode, CargoHandoffStateCodes.InTransit, StringComparison.Ordinal))
            {
                if (movements.Count != 1
                    || !string.Equals(movements[0].WorldZoneCode, "transport-network", StringComparison.Ordinal)
                    || !string.Equals(movements[0].ActorRoleCode, "Transporter", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("CargoHandoffTransitMovementInvalid");
                }

                return;
            }

            var roles = movements.Select(item => item.ActorRoleCode).ToHashSet(StringComparer.Ordinal);
            if (movements.Any(item => !string.Equals(item.WorldZoneCode, "warehouse", StringComparison.Ordinal))
                || !roles.Contains("Transporter")
                || !roles.Contains("WarehouseInboundWorker"))
            {
                throw new InvalidOperationException("CargoHandoffWarehouseMovementsInvalid");
            }
        }

        private static void RequireStableId(string value, string error)
        {
            if (!StableDataId.IsValid(value))
            {
                throw new InvalidOperationException(error + ":" + value);
            }
        }
    }

    public interface ICargoWarehouseHandoffApiClient
    {
        Task<CargoWarehouseHandoffApiModel?> GetAsync(
            CancellationToken cancellationToken = default);
    }

    public interface ICargoWarehouseHandoffRepository
    {
        Task<CargoWarehouseHandoffSnapshot?> 조회Async(
            CancellationToken cancellationToken = default);
    }

    public sealed class CargoWarehouseHandoffApiRepository : ICargoWarehouseHandoffRepository
    {
        private readonly ICargoWarehouseHandoffApiClient apiClient;
        private readonly CargoWarehouseHandoffMapper mapper;

        public CargoWarehouseHandoffApiRepository(
            ICargoWarehouseHandoffApiClient apiClient,
            CargoWarehouseHandoffMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CargoWarehouseHandoffSnapshot?> 조회Async(
            CancellationToken cancellationToken = default)
        {
            var source = await apiClient.GetAsync(cancellationToken).ConfigureAwait(false);
            return source == null ? null : mapper.Map(source);
        }
    }

    public sealed class CargoWarehouseHandoffQueryUseCase
    {
        private readonly ICargoWarehouseHandoffRepository repository;

        public CargoWarehouseHandoffQueryUseCase(ICargoWarehouseHandoffRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<CargoWarehouseHandoffSnapshot?> 실행Async(
            CancellationToken cancellationToken = default)
        {
            return repository.조회Async(cancellationToken);
        }
    }

    public interface ICargoWarehouseHandoffTarget
    {
        void ApplyHandoff(CargoWarehouseHandoffSnapshot snapshot);
    }

    public sealed class CargoWarehouseHandoffApplicator
    {
        private long lastRevision = -1;

        public bool Apply(
            CargoWarehouseHandoffSnapshot snapshot,
            ICargoWarehouseHandoffTarget target)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (snapshot.Revision < lastRevision)
            {
                return false;
            }

            target.ApplyHandoff(snapshot);
            lastRevision = snapshot.Revision;
            return true;
        }
    }
}
