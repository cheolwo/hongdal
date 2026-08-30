using System.Reflection;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q009 승인 분야의 성장 낌새 Projection과 정확한 수치·기여 기록 비노출을 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "Projection 단위 시험이며 실제 온라인 인증·상대 관찰 UI·Play Mode 증거가 아니다.")]
public sealed class SimulationPlayerGrowthHintProjectionTests
{
    [Fact]
    public void 승인된분야만_정성단계의_성장낌새로_투영한다()
    {
        var profile = Profile();
        var result = new SimulationPlayerGrowthHintProjection().Project(
            profile, Simulation기본플레이어분야Catalog.Create(), new()
            {
                ObserverPlayerStableId = "player:observer",
                TargetPlayerStableId = profile.PlayerStableId,
                AuthorizationPolicyRevision = "growth-hint-consent.r1",
                AuthorizedDomainCodes = new[]
                {
                    Simulation플레이어분야Codes.창고재고,
                    Simulation플레이어분야Codes.전투사냥,
                },
                MaximumHintCount = 2,
            });

        Assert.True(result.Allowed);
        Assert.Equal(new[]
        {
            Simulation플레이어분야Codes.전투사냥,
            Simulation플레이어분야Codes.창고재고,
        }, result.Hints.Select(value => value.DomainStableId));
        Assert.All(result.Hints, value =>
            Assert.False(string.IsNullOrWhiteSpace(
                value.QualitativeStageCode)));
        Assert.False(result.ContainsExactProgressValues);
        Assert.False(result.ContainsContributionRecords);
        Assert.False(result.ContainsUnlockCodes);
        Assert.False(result.ContainsInventory);
        Assert.False(result.ChangesWorldState);
    }

    [Fact]
    public void 공개정책Revision이없으면_성장낌새를_반환하지않는다()
    {
        var profile = Profile();
        var result = new SimulationPlayerGrowthHintProjection().Project(
            profile, Simulation기본플레이어분야Catalog.Create(), new()
            {
                ObserverPlayerStableId = "player:observer",
                TargetPlayerStableId = profile.PlayerStableId,
                AuthorizedDomainCodes = new[]
                {
                    Simulation플레이어분야Codes.전투사냥,
                },
            });

        Assert.False(result.Allowed);
        Assert.Equal(Simulation성장낌새ProjectionCodes
            .PolicyRevisionRequired, result.ReasonCode);
        Assert.Empty(result.Hints);
    }

    [Fact]
    public void 출력계약에는_정확한진척값과_기여기록필드가없다()
    {
        var names = typeof(Simulation성장낌새HintSnapshot)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(value => value.Name).ToArray();

        Assert.DoesNotContain("이해도", names);
        Assert.DoesNotContain("현장숙련도", names);
        Assert.DoesNotContain("운영숙련도", names);
        Assert.DoesNotContain("기여기록들", names);
        Assert.DoesNotContain("활성해금Codes", names);
    }

    private static Simulation플레이어분야ProfileSnapshot Profile()
        => new Simulation플레이어분야ProfileSnapshot
        {
            PlayerStableId = "player:target",
            Revision = 7,
            분야진척들 = new[]
            {
                Progress(Simulation플레이어분야Codes.전투사냥,
                    3, 8, 1, Simulation분야단계Codes.숙련),
                Progress(Simulation플레이어분야Codes.창고재고,
                    2, 4, 3, Simulation분야단계Codes.익숙함),
                Progress(Simulation플레이어분야Codes.운영조직,
                    9, 9, 9, Simulation분야단계Codes.숙련),
            },
        };

    private static Simulation분야진척Snapshot Progress(string domain,
        int understanding, int field, int operation, string stage)
        => new Simulation분야진척Snapshot
        {
            분야StableId = domain,
            이해도 = understanding,
            현장숙련도 = field,
            운영숙련도 = operation,
            이해도단계Code = stage,
            현장숙련도단계Code = stage,
            운영숙련도단계Code = stage,
        };
}
