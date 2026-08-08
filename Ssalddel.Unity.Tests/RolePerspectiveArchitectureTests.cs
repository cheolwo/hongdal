using Ssalddel.Unity.Perspectives;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Tests.UnityData;

public sealed class RolePerspectiveArchitectureTests
{
    private static readonly DateTimeOffset GeneratedAt =
        DateTimeOffset.Parse("2026-08-08T13:00:00+09:00");

    [Fact]
    public async Task 요청역할과_Zone이_서버승인Snapshot과_일치할_때만_조회된다()
    {
        var apiClient = new FakeRolePerspectiveApiClient(request =>
            CreateApiModel(request.RequestedRoleCode, request.WorldZoneCode));
        var repository = new RolePerspectiveApiRepository(apiClient, new RolePerspectiveMapper());
        var useCase = new 역할관점조회UseCase(repository);

        var snapshot = await useCase.실행Async(new 역할관점조회Request
        {
            RequestedRoleCode = RolePerspectiveCodes.Orderer,
            WorldZoneCode = WorldZoneCodes.MarketOrder,
        });

        Assert.Equal(RolePerspectiveCodes.Orderer, snapshot.AuthorizedRoleCode);
        Assert.Equal(WorldZoneCodes.MarketOrder, snapshot.WorldZoneCode);
        Assert.Equal("authorization:test-decision", snapshot.AuthorizationDecisionId);
        Assert.Single(snapshot.ObjectEmphases);
        Assert.Single(snapshot.AllowedInteractions);
        Assert.Equal(1, apiClient.CallCount);
        Assert.Equal(
            "api/v1/driver/world/zones/urban-logistics-center/perspective",
            RolePerspectiveApiRoutes.DriverUrbanLogisticsCenter);
    }

    [Fact]
    public async Task 서버가_다른_역할을_승인한_응답은_클라이언트에서_거부한다()
    {
        var apiClient = new FakeRolePerspectiveApiClient(request =>
            CreateApiModel(RolePerspectiveCodes.Transporter, request.WorldZoneCode));
        var repository = new RolePerspectiveApiRepository(apiClient, new RolePerspectiveMapper());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.조회Async(new 역할관점조회Request
            {
                RequestedRoleCode = RolePerspectiveCodes.Orderer,
                WorldZoneCode = WorldZoneCodes.MarketOrder,
            }));

        Assert.Equal("RequestedRoleWasNotAuthorized", exception.Message);
    }

    [Fact]
    public void 서버Command는_확인과_Canonical재조회_없이는_매핑되지_않는다()
    {
        var source = CreateApiModel(RolePerspectiveCodes.Transporter, WorldZoneCodes.UrbanLogisticsCenter);
        source.AllowedInteractions[0].EffectCode = WorldInteractionEffectCodes.ServerCommand;
        source.AllowedInteractions[0].RequiresExplicitConfirmation = false;
        source.AllowedInteractions[0].RequiresCanonicalStateRefresh = true;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new RolePerspectiveMapper().Map(source));

        Assert.Equal("UnsafeServerCommandBoundary:inspect-product", exception.Message);
    }

    [Fact]
    public void 역할전환은_World대상을_교체하지_않고_Role상태와_허용Interaction만_바꾼다()
    {
        var crate = new FakeRolePerspectiveTarget("product:potato-20kg");
        var targets = new IRolePerspectiveTarget[] { crate };
        var interactionSink = new FakeRoleInteractionSink();
        var applicator = new RolePerspectiveApplicator();
        var producer = new RolePerspectiveMapper().Map(
            CreateApiModel(RolePerspectiveCodes.Producer, WorldZoneCodes.MarketOrder));

        producer.ObjectEmphases[0].Label = "내 출고 물량";
        var producerResult = applicator.Apply(producer, targets, interactionSink);

        Assert.Equal(1, producerResult.AppliedTargetCount);
        Assert.Equal("내 출고 물량", crate.AppliedPerspective?.Label);
        Assert.Equal("inspect-product", interactionSink.Interactions.Single().InteractionCode);

        var orderer = new RolePerspectiveMapper().Map(
            CreateApiModel(RolePerspectiveCodes.Orderer, WorldZoneCodes.MarketOrder));
        orderer.ObjectEmphases[0].Label = "내가 주문할 상품";
        orderer.AllowedInteractions[0].InteractionCode = "open-order-detail";

        var ordererResult = applicator.Apply(orderer, targets, interactionSink);

        Assert.Equal(1, ordererResult.AppliedTargetCount);
        Assert.Equal(2, crate.ClearCount);
        Assert.Equal("내가 주문할 상품", crate.AppliedPerspective?.Label);
        Assert.Equal("open-order-detail", interactionSink.Interactions.Single().InteractionCode);
        Assert.Same(crate, targets[0]);
    }

    [Fact]
    public async Task RoleExperienceCoordinator는_서버조회후_같은Zone대상에_관점을_적용한다()
    {
        var apiClient = new FakeRolePerspectiveApiClient(request =>
            CreateApiModel(request.RequestedRoleCode, request.WorldZoneCode));
        var useCase = new 역할관점조회UseCase(
            new RolePerspectiveApiRepository(apiClient, new RolePerspectiveMapper()));
        var coordinator = new RoleExperienceCoordinator(useCase, new RolePerspectiveApplicator());
        var target = new FakeRolePerspectiveTarget("product:potato-20kg");
        var sink = new FakeRoleInteractionSink();

        var result = await coordinator.SwitchAsync(
            new 역할관점조회Request
            {
                RequestedRoleCode = RolePerspectiveCodes.Transporter,
                WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
            },
            new IRolePerspectiveTarget[] { target },
            sink);

        Assert.Equal(RolePerspectiveCodes.Transporter, result.Snapshot.AuthorizedRoleCode);
        Assert.Equal(1, result.Application.AppliedTargetCount);
        Assert.Equal(RoleObjectEmphasisCodes.Primary, target.AppliedPerspective?.EmphasisCode);
        Assert.Single(sink.Interactions);
    }

    [Fact]
    public void 로드되지_않은_Zone대상은_권한을_추론하지_않고_미해결로_보고한다()
    {
        var snapshot = new RolePerspectiveMapper().Map(
            CreateApiModel(RolePerspectiveCodes.Transporter, WorldZoneCodes.UrbanLogisticsCenter));
        var sink = new FakeRoleInteractionSink();

        var result = new RolePerspectiveApplicator().Apply(
            snapshot,
            Array.Empty<IRolePerspectiveTarget>(),
            sink);

        Assert.Equal(0, result.AppliedTargetCount);
        Assert.Equal(new[] { "product:potato-20kg" }, result.UnresolvedTargetStableIds);
        Assert.Single(sink.Interactions);
    }

    private static RolePerspectiveApiModel CreateApiModel(string roleCode, string zoneCode)
    {
        return new RolePerspectiveApiModel
        {
            StableId = "perspective:market-order",
            Revision = 7,
            AuthorizedRoleCode = roleCode,
            WorldZoneCode = zoneCode,
            ViewerScopeCode = WorldViewerScopeCodes.AuthorizedParty,
            SourceTypeCode = RolePerspectiveSourceTypeCodes.OperationalProjection,
            AuthorizationDecisionId = "authorization:test-decision",
            GeneratedAt = GeneratedAt,
            ObjectEmphases = new[]
            {
                new RoleObjectEmphasisApiModel
                {
                    TargetStableId = "product:potato-20kg",
                    EmphasisCode = RoleObjectEmphasisCodes.Primary,
                    Label = "역할별 강조",
                    DetailPanelCode = "product-detail",
                },
            },
            AllowedInteractions = new[]
            {
                new RoleAllowedInteractionApiModel
                {
                    InteractionCode = "inspect-product",
                    TargetStableId = "product:potato-20kg",
                    EffectCode = WorldInteractionEffectCodes.ReadOnly,
                },
            },
        };
    }

    private sealed class FakeRolePerspectiveApiClient : IRolePerspectiveApiClient
    {
        private readonly Func<역할관점조회Request, RolePerspectiveApiModel> responseFactory;

        public FakeRolePerspectiveApiClient(
            Func<역할관점조회Request, RolePerspectiveApiModel> responseFactory)
        {
            this.responseFactory = responseFactory;
        }

        public int CallCount { get; private set; }

        public Task<RolePerspectiveApiModel> GetAsync(
            역할관점조회Request request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class FakeRolePerspectiveTarget : IRolePerspectiveTarget
    {
        public FakeRolePerspectiveTarget(string stableId)
        {
            StableId = stableId;
        }

        public string StableId { get; }

        public int ClearCount { get; private set; }

        public 역할Object관점? AppliedPerspective { get; private set; }

        public void ClearRolePerspective()
        {
            ClearCount++;
            AppliedPerspective = null;
        }

        public void ApplyRolePerspective(역할Object관점 perspective)
        {
            AppliedPerspective = perspective;
        }
    }

    private sealed class FakeRoleInteractionSink : IRoleInteractionSink
    {
        public IReadOnlyList<역할허용Interaction> Interactions { get; private set; } =
            Array.Empty<역할허용Interaction>();

        public void ReplaceAllowedInteractions(IReadOnlyList<역할허용Interaction> interactions)
        {
            Interactions = interactions;
        }
    }
}
