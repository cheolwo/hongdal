using Ssalddel.Unity.ResidentialPickup;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class ResidentialPickupVerticalSliceTests
{
    [Theory]
    [InlineData(ResidentialPickupRoleCodes.Orderer, "내 수령 상품")]
    [InlineData(ResidentialPickupRoleCodes.Transporter, "내 하차 대상")]
    public async Task 같은공동수령Object를_권한역할에맞게_투영한다(
        string roleCode,
        string expectedRoleLabel)
    {
        var useCase = UseCase(Response(roleCode, expectedRoleLabel));

        var result = await useCase.실행Async(roleCode);

        Assert.Equal(roleCode, result.AuthorizedRoleCode);
        Assert.Equal("residential-pickup:91", Assert.Single(result.PickupPoints).StableId);
        Assert.Equal(expectedRoleLabel, result.PickupPoints[0].RoleLabel);
    }

    [Fact]
    public void Mapper는_요청역할과_서버승인역할이_다르면_거부한다()
    {
        Assert.Equal(
            "ResidentialPickupAuthorizedRoleMismatch",
            Assert.Throws<InvalidOperationException>(() =>
                new ResidentialPickupPerspectiveMapper().Map(
                    Response(ResidentialPickupRoleCodes.Orderer, "내 수령 상품"),
                    ResidentialPickupRoleCodes.Transporter)).Message);
    }

    [Fact]
    public void Applicator는_StableId로_적용하고_사라진Object를_숨긴다()
    {
        var target = new Target("residential-pickup:91");
        var other = new Target("residential-pickup:92");
        var snapshot = new ResidentialPickupPerspectiveMapper().Map(
            Response(ResidentialPickupRoleCodes.Orderer, "내 수령 상품"),
            ResidentialPickupRoleCodes.Orderer);

        var unresolved = new ResidentialPickupPerspectiveApplicator().Apply(
            snapshot,
            new[] { target, other });

        Assert.Empty(unresolved);
        Assert.NotNull(target.Last);
        Assert.True(other.Hidden);
    }

    [Fact]
    public void Applicator는_역할별Perspective의_revision을_독립관리한다()
    {
        var target = new Target("residential-pickup:91");
        var mapper = new ResidentialPickupPerspectiveMapper();
        var orderer = mapper.Map(
            Response(ResidentialPickupRoleCodes.Orderer, "내 수령 상품"),
            ResidentialPickupRoleCodes.Orderer);
        var transporterSource = Response(
            ResidentialPickupRoleCodes.Transporter,
            "내 하차 대상");
        transporterSource.Revision = 1;
        var transporter = mapper.Map(
            transporterSource,
            ResidentialPickupRoleCodes.Transporter);
        var applicator = new ResidentialPickupPerspectiveApplicator();

        applicator.Apply(orderer, new[] { target });
        applicator.Apply(transporter, new[] { target });

        Assert.Equal("내 하차 대상", target.Last!.RoleLabel);
    }

    private static ResidentialPickupPerspectiveQueryUseCase UseCase(
        ResidentialPickupPerspectiveApiModel response)
    {
        var repository = new ResidentialPickupPerspectiveApiRepository(
            new Client(response),
            new ResidentialPickupPerspectiveMapper());
        return new ResidentialPickupPerspectiveQueryUseCase(repository);
    }

    private static ResidentialPickupPerspectiveApiModel Response(
        string roleCode,
        string roleLabel)
    {
        return new ResidentialPickupPerspectiveApiModel
        {
            StableId = "role-perspective:residential-pickup." + roleCode.ToLowerInvariant(),
            Revision = 7,
            AuthorizedRoleCode = roleCode,
            WorldZoneCode = "residential-pickup",
            ViewerScopeCode = "AuthorizedParty",
            SourceTypeCode = "OperationalProjection",
            AuthorizationDecisionId = "authorized:test",
            GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            PickupPoints =
            [
                new ResidentialPickupPointApiModel
                {
                    StableId = "residential-pickup:91",
                    CanonicalTaskStableId = "unloading-task:71.91",
                    PickupPointLabel = "공동 수령지",
                    ProductLabel = "감자 20kg",
                    Quantity = 3,
                    StatusCode = ResidentialPickupStatusCodes.Arrived,
                    RoleLabel = roleLabel,
                    CanInspect = true,
                    UpdatedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            ],
        };
    }

    private sealed class Client(ResidentialPickupPerspectiveApiModel response)
        : IResidentialPickupPerspectiveApiClient
    {
        public Task<ResidentialPickupPerspectiveApiModel> GetAsync(
            string requestedRoleCode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class Target(string stableId) : IResidentialPickupPointTarget
    {
        public string StableId { get; } = stableId;
        public ResidentialPickupPointSnapshot? Last { get; private set; }
        public bool Hidden { get; private set; }

        public void Apply(ResidentialPickupPointSnapshot point, string authorizedRoleCode)
        {
            Last = point;
            Hidden = false;
        }

        public void Hide()
        {
            Hidden = true;
        }
    }
}
