using Ssalddel.Unity.Cards;
using Ssalddel.Unity.PublicData;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationRealityContextPresentationTests
{
    [Fact]
    public void 기본투영은_현상만표현하고_원관측자료계약을갖지않는다()
    {
        var source = Projection(includeSourceDetails: false);

        var result = new RealityContextPresentationMapper().Map(source);

        Assert.True(result.PresentationOnly);
        Assert.Single(result.Phenomena);
        Assert.Empty(result.SourceInformation);
        var names = typeof(RealityContextPlayerProjectionApiModel).GetProperties()
            .Select(value => value.Name).ToArray();
        Assert.DoesNotContain("Measurements", names);
        Assert.DoesNotContain("Value", names);
        Assert.DoesNotContain("SourceHashSha256", names);
        Assert.DoesNotContain("ParcelStableId", names);
        Assert.DoesNotContain("ApiKey", names);
    }

    [Fact]
    public async Task 선택형출처상세는_읽기전용정보카드로만들어진다()
    {
        var presentation = new RealityContextPresentationMapper().Map(
            Projection(includeSourceDetails: true));
        var source = new RealityContextInformationCardFamilySource(presentation);

        var family = await source.LoadAsync(CancellationToken.None);

        var card = Assert.Single(family.Items);
        Assert.Equal(CardFamilyCodes.ConceptInformation, card.FamilyCode);
        Assert.Equal(CardHierarchyTierCodes.Knowledge, card.HierarchyTierCode);
        Assert.Equal(CardAuthorityCodes.ProjectionReadOnly, card.AuthorityCode);
        Assert.Equal(CardActionRouteCodes.OpenInformation, card.ActionRouteCode);
        Assert.Contains("관측소 지점 관측", card.Summary);
        Assert.Empty(family.Relations);
    }

    [Fact]
    public void 출처상세미요청응답에_출처가들어오면거부한다()
    {
        var source = Projection(includeSourceDetails: true);
        source.SourceDetailsIncluded = false;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new RealityContextPresentationMapper().Map(source));

        Assert.Equal("RealityContextSourceDetailsUnexpected", error.Message);
    }

    [Fact]
    public async Task 기본UseCase는_출처상세를요청하지않는다()
    {
        var client = new RecordingClient();
        var useCase = new RealityContextUseCase(new RealityContextRepository(
            client, new RealityContextPresentationMapper()));

        var result = await useCase.LoadWorldPhenomenaAsync("simulation-session:test");

        Assert.False(client.LastIncludeSourceDetails);
        Assert.Single(result.Phenomena);
    }

    private static RealityContextPlayerProjectionApiModel Projection(
        bool includeSourceDetails)
        => new()
        {
            ContextSnapshotStableId = "reality-context:session:test:v1",
            AvailabilityCode = "Available",
            FrozenAtUtc = new DateTimeOffset(2026, 8, 20, 0, 0, 0, TimeSpan.Zero),
            PresentationOnly = true,
            SourceDetailsIncluded = includeSourceDetails,
            Phenomena = new[]
            {
                new RealityContextPhenomenonApiModel
                {
                    PhenomenonStableId = "reality-context:session:test:v1:wet",
                    PhenomenonCode = "WetWorkContext",
                    TitleKorean = "젖은 작업 환경",
                    SummaryKorean = "배수와 밭 작업 순서를 살펴볼 만합니다.",
                    H3StableIds = new[] { "h3-candidate:highland-farm" },
                    AdvisoryCodes = new[] { "InspectDrainage" },
                },
            },
            SourceInformation = includeSourceDetails
                ? new[]
                {
                    new RealityContextSourceInformationApiModel
                    {
                        InformationStableId = "reality-source:kma:information",
                        SourceName = "기상청 종관기상관측 일자료",
                        ReferenceTimeUtc = new DateTimeOffset(2026, 8, 19, 12, 0, 0,
                            TimeSpan.Zero),
                        SpatialPrecisionCode = "StationObservation",
                        SourceHref = "https://www.data.go.kr/data/15059093/openapi.do",
                        LimitationCodes = new[]
                        {
                            "StationObservationIsNotParcelObservation",
                        },
                        LimitationSummariesKorean = new[]
                        {
                            "관측소 값은 개별 농지의 직접 관측이 아닙니다.",
                        },
                    },
                }
                : Array.Empty<RealityContextSourceInformationApiModel>(),
        };

    private sealed class RecordingClient : IRealityContextApiClient
    {
        public bool LastIncludeSourceDetails { get; private set; }

        public Task<RealityContextPlayerProjectionApiModel> GetAsync(
            string sessionStableId, bool includeSourceDetails,
            CancellationToken cancellationToken)
        {
            LastIncludeSourceDetails = includeSourceDetails;
            return Task.FromResult(Projection(includeSourceDetails));
        }
    }
}
