using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.ResidentialPickup
{
    public static class ResidentialPickupApiRoutes
    {
        public const string Orderer =
            "api/v1/orderer/world/zones/residential-pickup/perspective";

        public const string Transporter =
            "api/v1/driver/world/zones/residential-pickup/perspective";
    }

    public static class ResidentialPickupRoleCodes
    {
        public const string Orderer = "Orderer";
        public const string Transporter = "Transporter";
    }

    public static class ResidentialPickupStatusCodes
    {
        public const string Waiting = "Waiting";
        public const string Arrived = "Arrived";
        public const string Completed = "Completed";
    }

    public sealed class ResidentialPickupPointApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string PickupPointLabel { get; set; } = string.Empty;
        public string ProductLabel { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string RoleLabel { get; set; } = string.Empty;
        public bool CanInspect { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class ResidentialPickupPerspectiveApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string WorldZoneCode { get; set; } = string.Empty;
        public string ViewerScopeCode { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string AuthorizationDecisionId { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public ResidentialPickupPointApiModel[] PickupPoints { get; set; } =
            Array.Empty<ResidentialPickupPointApiModel>();
    }

    public sealed class ResidentialPickupPointSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string PickupPointLabel { get; set; } = string.Empty;
        public string ProductLabel { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string RoleLabel { get; set; } = string.Empty;
        public bool CanInspect { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class ResidentialPickupPerspectiveSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string AuthorizationDecisionId { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public ResidentialPickupPointSnapshot[] PickupPoints { get; set; } =
            Array.Empty<ResidentialPickupPointSnapshot>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface IResidentialPickupPerspectiveApiClient
    {
        Task<ResidentialPickupPerspectiveApiModel> GetAsync(
            string requestedRoleCode,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class ResidentialPickupPerspectiveMapper
    {
        private static readonly HashSet<string> Roles = new HashSet<string>(StringComparer.Ordinal)
        {
            ResidentialPickupRoleCodes.Orderer,
            ResidentialPickupRoleCodes.Transporter,
        };

        private static readonly HashSet<string> Statuses = new HashSet<string>(StringComparer.Ordinal)
        {
            ResidentialPickupStatusCodes.Waiting,
            ResidentialPickupStatusCodes.Arrived,
            ResidentialPickupStatusCodes.Completed,
        };

        public ResidentialPickupPerspectiveSnapshot Map(
            ResidentialPickupPerspectiveApiModel source,
            string requestedRoleCode)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!Roles.Contains(requestedRoleCode)
                || !string.Equals(source.AuthorizedRoleCode, requestedRoleCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("ResidentialPickupAuthorizedRoleMismatch");
            }

            if (!StableDataId.IsValid(source.StableId)
                || source.Revision < 0
                || source.GeneratedAt == default
                || source.PickupPoints == null)
            {
                throw new InvalidOperationException("ResidentialPickupPerspectiveInvalid");
            }

            var duplicate = source.PickupPoints
                .GroupBy(item => item.StableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException(
                    "ResidentialPickupStableIdDuplicate:" + duplicate.Key);
            }

            return new ResidentialPickupPerspectiveSnapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                AuthorizedRoleCode = source.AuthorizedRoleCode,
                AuthorizationDecisionId = source.AuthorizationDecisionId,
                GeneratedAt = source.GeneratedAt,
                PickupPoints = source.PickupPoints.Select(MapPoint).ToArray(),
            };
        }

        private static ResidentialPickupPointSnapshot MapPoint(
            ResidentialPickupPointApiModel source)
        {
            if (source == null
                || !StableDataId.IsValid(source.StableId)
                || !StableDataId.IsValid(source.CanonicalTaskStableId)
                || !Statuses.Contains(source.StatusCode)
                || source.Quantity < 0
                || source.UpdatedAt == default)
            {
                throw new InvalidOperationException("ResidentialPickupPointInvalid");
            }

            return new ResidentialPickupPointSnapshot
            {
                StableId = source.StableId,
                CanonicalTaskStableId = source.CanonicalTaskStableId,
                PickupPointLabel = source.PickupPointLabel,
                ProductLabel = source.ProductLabel,
                Quantity = source.Quantity,
                StatusCode = source.StatusCode,
                RoleLabel = source.RoleLabel,
                CanInspect = source.CanInspect,
                UpdatedAt = source.UpdatedAt,
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface IResidentialPickupPerspectiveRepository
    {
        Task<ResidentialPickupPerspectiveSnapshot> LoadAsync(
            string requestedRoleCode,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class ResidentialPickupPerspectiveApiRepository
        : IResidentialPickupPerspectiveRepository
    {
        private readonly IResidentialPickupPerspectiveApiClient apiClient;
        private readonly ResidentialPickupPerspectiveMapper mapper;

        public ResidentialPickupPerspectiveApiRepository(
            IResidentialPickupPerspectiveApiClient client,
            ResidentialPickupPerspectiveMapper modelMapper)
        {
            apiClient = client;
            mapper = modelMapper;
        }

        public async Task<ResidentialPickupPerspectiveSnapshot> LoadAsync(
            string requestedRoleCode,
            CancellationToken cancellationToken = default)
        {
            var response = await apiClient.GetAsync(requestedRoleCode, cancellationToken)
                .ConfigureAwait(false);
            return mapper.Map(response, requestedRoleCode);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class ResidentialPickupPerspectiveQueryUseCase
    {
        private readonly IResidentialPickupPerspectiveRepository repository;

        public ResidentialPickupPerspectiveQueryUseCase(
            IResidentialPickupPerspectiveRepository perspectiveRepository)
        {
            repository = perspectiveRepository;
        }

        public Task<ResidentialPickupPerspectiveSnapshot> 실행Async(
            string requestedRoleCode,
            CancellationToken cancellationToken = default)
        {
            return repository.LoadAsync(requestedRoleCode, cancellationToken);
        }
    }

    public interface IResidentialPickupPointTarget
    {
        string StableId { get; }
        void Apply(ResidentialPickupPointSnapshot point, string authorizedRoleCode);
        void Hide();
    }

    public sealed class ResidentialPickupPerspectiveApplicator
    {
        private readonly Dictionary<string, long> lastRevisionByPerspective =
            new Dictionary<string, long>(StringComparer.Ordinal);

        public string[] Apply(
            ResidentialPickupPerspectiveSnapshot snapshot,
            IReadOnlyCollection<IResidentialPickupPointTarget> targets)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (lastRevisionByPerspective.TryGetValue(snapshot.StableId, out var lastRevision)
                && snapshot.Revision < lastRevision)
            {
                return Array.Empty<string>();
            }

            var targetById = targets.ToDictionary(target => target.StableId, StringComparer.Ordinal);
            var visible = new HashSet<string>(StringComparer.Ordinal);
            var unresolved = new List<string>();
            foreach (var point in snapshot.PickupPoints)
            {
                visible.Add(point.StableId);
                if (targetById.TryGetValue(point.StableId, out var target))
                {
                    target.Apply(point, snapshot.AuthorizedRoleCode);
                }
                else
                {
                    unresolved.Add(point.StableId);
                }
            }

            foreach (var target in targets)
            {
                if (!visible.Contains(target.StableId))
                {
                    target.Hide();
                }
            }

            lastRevisionByPerspective[snapshot.StableId] = snapshot.Revision;
            return unresolved.ToArray();
        }
    }
}
