using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Cards;

namespace Ssalddel.Unity.PublicData
{
    public sealed class RealityContextPlayerProjectionApiModel
    {
        public string ContextSnapshotStableId { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } = string.Empty;
        public DateTimeOffset FrozenAtUtc { get; set; }
        public RealityContextPhenomenonApiModel[] Phenomena { get; set; } =
            Array.Empty<RealityContextPhenomenonApiModel>();
        public RealityContextSourceInformationApiModel[] SourceInformation { get; set; } =
            Array.Empty<RealityContextSourceInformationApiModel>();
        public bool SourceDetailsIncluded { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class RealityContextPhenomenonApiModel
    {
        public string PhenomenonStableId { get; set; } = string.Empty;
        public string PhenomenonCode { get; set; } = string.Empty;
        public string TitleKorean { get; set; } = string.Empty;
        public string SummaryKorean { get; set; } = string.Empty;
        public string[] H3StableIds { get; set; } = Array.Empty<string>();
        public string[] AdvisoryCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class RealityContextSourceInformationApiModel
    {
        public string InformationStableId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? ReferenceTimeUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
        public string[] LimitationSummariesKorean { get; set; } = Array.Empty<string>();
    }

    public sealed class RealityContextWorldPresentation
    {
        public string ContextSnapshotStableId { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } = string.Empty;
        public DateTimeOffset FrozenAtUtc { get; set; }
        public RealityContextPhenomenonPresentation[] Phenomena { get; set; } =
            Array.Empty<RealityContextPhenomenonPresentation>();
        public RealityContextSourceInformationPresentation[] SourceInformation { get; set; } =
            Array.Empty<RealityContextSourceInformationPresentation>();
        public bool PresentationOnly { get; set; }
    }

    public sealed class RealityContextPhenomenonPresentation
    {
        public string StableId { get; set; } = string.Empty;
        public string PhenomenonCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string[] H3StableIds { get; set; } = Array.Empty<string>();
        public string[] AdvisoryCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class RealityContextSourceInformationPresentation
    {
        public string StableId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? ReferenceTimeUtc { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string[] LimitationCodes { get; set; } = Array.Empty<string>();
        public string[] LimitationSummariesKorean { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// 서버가 확정한 현상과 선택형 출처 설명만 투영한다.
    /// 관측 원수치, 입력 hash, API key, 필지 식별자는 이 경계에 존재하지 않는다.
    /// </summary>
    public sealed class RealityContextPresentationMapper
    {
        public RealityContextWorldPresentation Map(
            RealityContextPlayerProjectionApiModel source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.ContextSnapshotStableId)
                || string.IsNullOrWhiteSpace(source.AvailabilityCode)
                || source.FrozenAtUtc == default
                || source.Phenomena == null || source.SourceInformation == null
                || !source.PresentationOnly
                || source.Phenomena.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.PhenomenonStableId)
                    || string.IsNullOrWhiteSpace(value.PhenomenonCode)
                    || string.IsNullOrWhiteSpace(value.TitleKorean)
                    || value.H3StableIds == null || value.AdvisoryCodes == null)
                || source.SourceInformation.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.InformationStableId)
                    || string.IsNullOrWhiteSpace(value.SourceName)
                    || string.IsNullOrWhiteSpace(value.SpatialPrecisionCode)
                    || value.LimitationCodes == null
                    || value.LimitationSummariesKorean == null))
                throw new InvalidOperationException("RealityContextProjectionInvalid");
            if (!source.SourceDetailsIncluded && source.SourceInformation.Length > 0)
                throw new InvalidOperationException("RealityContextSourceDetailsUnexpected");

            return new RealityContextWorldPresentation
            {
                ContextSnapshotStableId = source.ContextSnapshotStableId,
                AvailabilityCode = source.AvailabilityCode,
                FrozenAtUtc = source.FrozenAtUtc,
                Phenomena = source.Phenomena.Select(value =>
                    new RealityContextPhenomenonPresentation
                    {
                        StableId = value.PhenomenonStableId,
                        PhenomenonCode = value.PhenomenonCode,
                        Title = value.TitleKorean,
                        Summary = value.SummaryKorean,
                        H3StableIds = value.H3StableIds.ToArray(),
                        AdvisoryCodes = value.AdvisoryCodes.ToArray(),
                    }).ToArray(),
                SourceInformation = source.SourceInformation.Select(value =>
                    new RealityContextSourceInformationPresentation
                    {
                        StableId = value.InformationStableId,
                        SourceName = value.SourceName,
                        ReferenceTimeUtc = value.ReferenceTimeUtc,
                        SpatialPrecisionCode = value.SpatialPrecisionCode,
                        SourceHref = value.SourceHref,
                        LimitationCodes = value.LimitationCodes.ToArray(),
                        LimitationSummariesKorean = value.LimitationSummariesKorean.ToArray(),
                    }).ToArray(),
                PresentationOnly = true,
            };
        }
    }

    public interface IRealityContextApiClient
    {
        Task<RealityContextPlayerProjectionApiModel> GetAsync(string sessionStableId,
            bool includeSourceDetails, CancellationToken cancellationToken);
    }

    public sealed class RealityContextRepository
    {
        private readonly IRealityContextApiClient apiClient;
        private readonly RealityContextPresentationMapper mapper;

        public RealityContextRepository(IRealityContextApiClient client,
            RealityContextPresentationMapper presentationMapper)
        {
            apiClient = client ?? throw new ArgumentNullException(nameof(client));
            mapper = presentationMapper
                ?? throw new ArgumentNullException(nameof(presentationMapper));
        }

        public async Task<RealityContextWorldPresentation> LoadAsync(
            string sessionStableId, bool includeSourceDetails,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new ArgumentException("SimulationSessionStableIdMissing",
                    nameof(sessionStableId));
            return mapper.Map(await apiClient.GetAsync(sessionStableId.Trim(),
                includeSourceDetails, cancellationToken));
        }
    }

    public sealed class RealityContextUseCase
    {
        private readonly RealityContextRepository repository;

        public RealityContextUseCase(RealityContextRepository realityRepository)
            => repository = realityRepository
                ?? throw new ArgumentNullException(nameof(realityRepository));

        public Task<RealityContextWorldPresentation> LoadWorldPhenomenaAsync(
            string sessionStableId, CancellationToken cancellationToken = default)
            => repository.LoadAsync(sessionStableId, false, cancellationToken);

        public Task<RealityContextWorldPresentation> LoadOptionalSourceDetailsAsync(
            string sessionStableId, CancellationToken cancellationToken = default)
            => repository.LoadAsync(sessionStableId, true, cancellationToken);
    }

    public sealed class RealityContextInformationCardFamilySource : ICardFamilySource
    {
        private readonly RealityContextWorldPresentation projection;

        public RealityContextInformationCardFamilySource(
            RealityContextWorldPresentation worldPresentation)
            => projection = worldPresentation
                ?? throw new ArgumentNullException(nameof(worldPresentation));

        public string FamilyCode => CardFamilyCodes.ConceptInformation;

        public Task<CardWorkspaceFamilySnapshot> LoadAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = projection.SourceInformation.Select(value =>
                new CardWorkspaceItem
                {
                    CardStableId = value.StableId,
                    Title = value.SourceName,
                    Summary = BuildSummary(value),
                    FamilyCode = CardFamilyCodes.ConceptInformation,
                    HierarchyTierCode = CardHierarchyTierCodes.Knowledge,
                    AuthorityCode = CardAuthorityCodes.ProjectionReadOnly,
                    ActionRouteCode = CardActionRouteCodes.OpenInformation,
                    IsAvailable = true,
                    IsLocked = false,
                }).ToArray();
            return Task.FromResult(new CardWorkspaceFamilySnapshot
            {
                FamilyCode = FamilyCode,
                Items = items,
                Relations = Array.Empty<CardWorkspaceRelation>(),
                SourceRevision = projection.FrozenAtUtc.UtcDateTime.Ticks,
            });
        }

        private static string BuildSummary(
            RealityContextSourceInformationPresentation value)
        {
            var reference = value.ReferenceTimeUtc?.ToUniversalTime()
                .ToString("yyyy-MM-dd HH:mm 'UTC'") ?? "기준 시각 없음";
            var limitations = value.LimitationSummariesKorean.Length == 0
                ? "제한 설명 없음" : string.Join(" ", value.LimitationSummariesKorean);
            return reference + " · " + DescribeSpatialPrecision(
                value.SpatialPrecisionCode) + " · " + limitations;
        }

        private static string DescribeSpatialPrecision(string code)
            => code switch
            {
                "StationObservation" => "관측소 지점 관측",
                "MarketSurvey" => "시장 조사",
                "FiveKilometerGrid" => "5km 격자",
                _ => code,
            };
    }
}
