using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Unity.Perspectives
{
    public static class RolePerspectiveApiRoutes
    {
        public const string DriverUrbanLogisticsCenter =
            "api/v1/driver/world/zones/urban-logistics-center/perspective";
    }

    public static class RolePerspectiveCodes
    {
        public const string Producer = "Producer";
        public const string Orderer = "Orderer";
        public const string Transporter = "Transporter";

        public static bool IsSupported(string value)
        {
            return string.Equals(value, Producer, StringComparison.Ordinal)
                || string.Equals(value, Orderer, StringComparison.Ordinal)
                || string.Equals(value, Transporter, StringComparison.Ordinal);
        }
    }

    public static class RolePerspectiveSourceTypeCodes
    {
        public const string OperationalProjection = "OperationalProjection";
        public const string SimulatedFixture = "SimulatedFixture";
    }

    public static class RoleObjectEmphasisCodes
    {
        public const string Primary = "Primary";
        public const string Related = "Related";
        public const string Destination = "Destination";
        public const string Muted = "Muted";

        public static bool IsSupported(string value)
        {
            return string.Equals(value, Primary, StringComparison.Ordinal)
                || string.Equals(value, Related, StringComparison.Ordinal)
                || string.Equals(value, Destination, StringComparison.Ordinal)
                || string.Equals(value, Muted, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 역할 선택은 서버에 projection을 요청하기 위한 입력일 뿐 권한 증명이 아니다.
    /// 서버는 인증 session과 실제 역할 할당을 검증한 뒤 허용된 snapshot만 반환해야 한다.
    /// </summary>
    public sealed class 역할관점조회Request
    {
        public string RequestedRoleCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;
    }

    public sealed class RoleObjectEmphasisApiModel
    {
        public string TargetStableId { get; set; } = string.Empty;

        public string EmphasisCode { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string DetailPanelCode { get; set; } = string.Empty;
    }

    public sealed class RoleAllowedInteractionApiModel
    {
        public string InteractionCode { get; set; } = string.Empty;

        public string TargetStableId { get; set; } = string.Empty;

        public string EffectCode { get; set; } = string.Empty;

        public bool RequiresExplicitConfirmation { get; set; }

        public bool RequiresCanonicalStateRefresh { get; set; }
    }

    /// <summary>
    /// 서버가 인증 사용자, 활성 역할과 Zone 범위를 검증한 뒤 반환하는 wire model이다.
    /// Unity는 이 객체에 없는 개인정보나 행동 권한을 추론하지 않는다.
    /// </summary>
    public sealed class RolePerspectiveApiModel
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string AuthorizedRoleCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;

        public string ViewerScopeCode { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public string AuthorizationDecisionId { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }

        public RoleObjectEmphasisApiModel[] ObjectEmphases { get; set; } =
            Array.Empty<RoleObjectEmphasisApiModel>();

        public RoleAllowedInteractionApiModel[] AllowedInteractions { get; set; } =
            Array.Empty<RoleAllowedInteractionApiModel>();
    }

    public sealed class 역할Object관점
    {
        public string TargetStableId { get; set; } = string.Empty;

        public string EmphasisCode { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string DetailPanelCode { get; set; } = string.Empty;
    }

    public sealed class 역할허용Interaction
    {
        public string InteractionCode { get; set; } = string.Empty;

        public string TargetStableId { get; set; } = string.Empty;

        public string EffectCode { get; set; } = string.Empty;

        public bool RequiresExplicitConfirmation { get; set; }

        public bool RequiresCanonicalStateRefresh { get; set; }
    }

    public sealed class 역할관점Snapshot
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string AuthorizedRoleCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;

        public string ViewerScopeCode { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public string AuthorizationDecisionId { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }

        public 역할Object관점[] ObjectEmphases { get; set; } = Array.Empty<역할Object관점>();

        public 역할허용Interaction[] AllowedInteractions { get; set; } =
            Array.Empty<역할허용Interaction>();
    }

    public sealed class RolePerspectiveMapper
    {
        private static readonly HashSet<string> ViewerScopes = new HashSet<string>(StringComparer.Ordinal)
        {
            WorldViewerScopeCodes.Public,
            WorldViewerScopeCodes.Personal,
            WorldViewerScopeCodes.Organization,
            WorldViewerScopeCodes.AuthorizedParty,
            WorldViewerScopeCodes.Operator,
        };

        public 역할관점Snapshot Map(RolePerspectiveApiModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Validate(source);

            return new 역할관점Snapshot
            {
                StableId = source.StableId.Trim(),
                Revision = source.Revision,
                AuthorizedRoleCode = source.AuthorizedRoleCode.Trim(),
                WorldZoneCode = source.WorldZoneCode.Trim(),
                ViewerScopeCode = source.ViewerScopeCode.Trim(),
                SourceTypeCode = source.SourceTypeCode.Trim(),
                AuthorizationDecisionId = source.AuthorizationDecisionId.Trim(),
                GeneratedAt = source.GeneratedAt,
                ObjectEmphases = source.ObjectEmphases.Select(MapObject).ToArray(),
                AllowedInteractions = source.AllowedInteractions.Select(MapInteraction).ToArray(),
            };
        }

        private static void Validate(RolePerspectiveApiModel source)
        {
            RequireStableId(source.StableId, "PerspectiveStableIdInvalid");
            if (source.Revision < 0)
            {
                throw new InvalidOperationException("PerspectiveRevisionInvalid");
            }

            if (!RolePerspectiveCodes.IsSupported(source.AuthorizedRoleCode))
            {
                throw new InvalidOperationException("AuthorizedRoleInvalid");
            }

            Require(source.WorldZoneCode, "WorldZoneMissing");
            if (!ViewerScopes.Contains(source.ViewerScopeCode))
            {
                throw new InvalidOperationException("ViewerScopeInvalid");
            }

            if (!string.Equals(source.SourceTypeCode, RolePerspectiveSourceTypeCodes.OperationalProjection, StringComparison.Ordinal)
                && !string.Equals(source.SourceTypeCode, RolePerspectiveSourceTypeCodes.SimulatedFixture, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("PerspectiveSourceTypeInvalid");
            }

            Require(source.AuthorizationDecisionId, "AuthorizationDecisionMissing");
            if (source.GeneratedAt == default)
            {
                throw new InvalidOperationException("PerspectiveGeneratedAtMissing");
            }

            if (source.ObjectEmphases == null)
            {
                throw new InvalidOperationException("ObjectEmphasesMissing");
            }

            if (source.AllowedInteractions == null)
            {
                throw new InvalidOperationException("AllowedInteractionsMissing");
            }

            RejectDuplicate(
                source.ObjectEmphases.Where(item => item != null).Select(item => item.TargetStableId),
                "DuplicatePerspectiveTarget:");
            RejectDuplicate(
                source.AllowedInteractions.Where(item => item != null)
                    .Select(item => item.InteractionCode + "@" + item.TargetStableId),
                "DuplicateAllowedInteraction:");

            foreach (var item in source.ObjectEmphases)
            {
                if (item == null)
                {
                    throw new InvalidOperationException("PerspectiveTargetMissing");
                }

                RequireStableId(item.TargetStableId, "PerspectiveTargetStableIdInvalid");
                if (!RoleObjectEmphasisCodes.IsSupported(item.EmphasisCode))
                {
                    throw new InvalidOperationException("PerspectiveEmphasisInvalid:" + item.TargetStableId);
                }
            }

            foreach (var interaction in source.AllowedInteractions)
            {
                if (interaction == null)
                {
                    throw new InvalidOperationException("AllowedInteractionMissing");
                }

                Require(interaction.InteractionCode, "InteractionCodeMissing");
                RequireStableId(interaction.TargetStableId, "InteractionTargetStableIdInvalid");
                if (!IsKnownEffect(interaction.EffectCode))
                {
                    throw new InvalidOperationException("InteractionEffectInvalid:" + interaction.InteractionCode);
                }

                if (string.Equals(interaction.EffectCode, WorldInteractionEffectCodes.ServerCommand, StringComparison.Ordinal)
                    && (!interaction.RequiresExplicitConfirmation || !interaction.RequiresCanonicalStateRefresh))
                {
                    throw new InvalidOperationException("UnsafeServerCommandBoundary:" + interaction.InteractionCode);
                }
            }
        }

        private static 역할Object관점 MapObject(RoleObjectEmphasisApiModel source)
        {
            return new 역할Object관점
            {
                TargetStableId = source.TargetStableId.Trim(),
                EmphasisCode = source.EmphasisCode.Trim(),
                Label = source.Label?.Trim() ?? string.Empty,
                DetailPanelCode = source.DetailPanelCode?.Trim() ?? string.Empty,
            };
        }

        private static 역할허용Interaction MapInteraction(RoleAllowedInteractionApiModel source)
        {
            return new 역할허용Interaction
            {
                InteractionCode = source.InteractionCode.Trim(),
                TargetStableId = source.TargetStableId.Trim(),
                EffectCode = source.EffectCode.Trim(),
                RequiresExplicitConfirmation = source.RequiresExplicitConfirmation,
                RequiresCanonicalStateRefresh = source.RequiresCanonicalStateRefresh,
            };
        }

        private static bool IsKnownEffect(string value)
        {
            return string.Equals(value, WorldInteractionEffectCodes.ReadOnly, StringComparison.Ordinal)
                || string.Equals(value, WorldInteractionEffectCodes.LocalSimulation, StringComparison.Ordinal)
                || string.Equals(value, WorldInteractionEffectCodes.ServerCommand, StringComparison.Ordinal)
                || string.Equals(value, WorldInteractionEffectCodes.WebHandoff, StringComparison.Ordinal);
        }

        private static void RejectDuplicate(IEnumerable<string> values, string errorPrefix)
        {
            var duplicate = values
                .GroupBy(value => value, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException(errorPrefix + duplicate.Key);
            }
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

    public interface IRolePerspectiveApiClient
    {
        Task<RolePerspectiveApiModel> GetAsync(
            역할관점조회Request request,
            CancellationToken cancellationToken = default);
    }

    public interface I역할관점Repository
    {
        Task<역할관점Snapshot> 조회Async(
            역할관점조회Request request,
            CancellationToken cancellationToken = default);
    }

    public sealed class RolePerspectiveApiRepository : I역할관점Repository
    {
        private readonly IRolePerspectiveApiClient apiClient;
        private readonly RolePerspectiveMapper mapper;

        public RolePerspectiveApiRepository(
            IRolePerspectiveApiClient apiClient,
            RolePerspectiveMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<역할관점Snapshot> 조회Async(
            역할관점조회Request request,
            CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            var source = await apiClient.GetAsync(request, cancellationToken).ConfigureAwait(false);
            var snapshot = mapper.Map(source);

            if (!string.Equals(snapshot.AuthorizedRoleCode, request.RequestedRoleCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("RequestedRoleWasNotAuthorized");
            }

            if (!string.Equals(snapshot.WorldZoneCode, request.WorldZoneCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("PerspectiveZoneMismatch");
            }

            return snapshot;
        }

        private static void ValidateRequest(역할관점조회Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!RolePerspectiveCodes.IsSupported(request.RequestedRoleCode))
            {
                throw new ArgumentException("RequestedRoleInvalid", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.WorldZoneCode))
            {
                throw new ArgumentException("WorldZoneMissing", nameof(request));
            }
        }
    }

    public sealed class 역할관점조회UseCase
    {
        private readonly I역할관점Repository repository;

        public 역할관점조회UseCase(I역할관점Repository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<역할관점Snapshot> 실행Async(
            역할관점조회Request request,
            CancellationToken cancellationToken = default)
        {
            return repository.조회Async(request, cancellationToken);
        }
    }

    /// <summary>
    /// World View를 소유하지 않고 Role View socket만 갱신하는 대상 계약이다.
    /// </summary>
    public interface IRolePerspectiveTarget
    {
        string StableId { get; }

        void ClearRolePerspective();

        void ApplyRolePerspective(역할Object관점 perspective);
    }

    public interface IRoleInteractionSink
    {
        void ReplaceAllowedInteractions(IReadOnlyList<역할허용Interaction> interactions);
    }

    public sealed class 역할관점적용Result
    {
        public int AppliedTargetCount { get; set; }

        public string[] UnresolvedTargetStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class RolePerspectiveApplicator
    {
        public 역할관점적용Result Apply(
            역할관점Snapshot snapshot,
            IReadOnlyList<IRolePerspectiveTarget> targets,
            IRoleInteractionSink interactionSink)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (interactionSink == null)
            {
                throw new ArgumentNullException(nameof(interactionSink));
            }

            var targetMap = new Dictionary<string, IRolePerspectiveTarget>(StringComparer.Ordinal);
            foreach (var target in targets)
            {
                if (target == null)
                {
                    throw new InvalidOperationException("RolePerspectiveTargetMissing");
                }

                if (!StableDataId.IsValid(target.StableId))
                {
                    throw new InvalidOperationException("RolePerspectiveTargetStableIdInvalid:" + target.StableId);
                }

                if (!targetMap.TryAdd(target.StableId, target))
                {
                    throw new InvalidOperationException("DuplicateRolePerspectiveTarget:" + target.StableId);
                }

                target.ClearRolePerspective();
            }

            var applied = 0;
            var unresolved = new List<string>();
            foreach (var perspective in snapshot.ObjectEmphases)
            {
                if (targetMap.TryGetValue(perspective.TargetStableId, out var target))
                {
                    target.ApplyRolePerspective(perspective);
                    applied++;
                }
                else
                {
                    unresolved.Add(perspective.TargetStableId);
                }
            }

            interactionSink.ReplaceAllowedInteractions(snapshot.AllowedInteractions);
            return new 역할관점적용Result
            {
                AppliedTargetCount = applied,
                UnresolvedTargetStableIds = unresolved.ToArray(),
            };
        }
    }

    public sealed class 역할관점전환Result
    {
        public 역할관점Snapshot Snapshot { get; set; } = null!;

        public 역할관점적용Result Application { get; set; } = null!;
    }

    /// <summary>
    /// Presentation의 Role Experience Controller가 호출하는 engine-independent coordinator다.
    /// 조회와 stable-ID 적용 순서만 조율하며 권한 또는 interaction을 새로 계산하지 않는다.
    /// </summary>
    public sealed class RoleExperienceCoordinator
    {
        private readonly 역할관점조회UseCase query;
        private readonly RolePerspectiveApplicator applicator;

        public RoleExperienceCoordinator(
            역할관점조회UseCase query,
            RolePerspectiveApplicator applicator)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
            this.applicator = applicator ?? throw new ArgumentNullException(nameof(applicator));
        }

        public async Task<역할관점전환Result> SwitchAsync(
            역할관점조회Request request,
            IReadOnlyList<IRolePerspectiveTarget> targets,
            IRoleInteractionSink interactionSink,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await query.실행Async(request, cancellationToken).ConfigureAwait(false);
            var application = applicator.Apply(snapshot, targets, interactionSink);
            return new 역할관점전환Result
            {
                Snapshot = snapshot,
                Application = application,
            };
        }
    }
}
