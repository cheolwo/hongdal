using System;
using System.Linq;
using Ssalddel.Unity.PresentationContracts;
using Xunit;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "플레이어 감각 표현 계약의 결정성·권위 분리·필수 영역 회귀를 검증한다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata
        .SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
    WorkOrderIds = new[] { "E9-WO-NATURE-SURVIVAL-SOLO-PLACEMENT" },
    Boundary = "계약 시험은 Unity Play Mode·Game View·실제 청음 증거가 아니다.")]
public sealed class PlayerSensoryPresentationTests
{
    private readonly 플레이어감각표현Validator validator = new();

    [Fact]
    public void Nature도끼와벌목계획은_배치와WI표현책임을분리한다()
    {
        var placement = Nature플레이어감각표현Fixture.CreatePlacementPlan();
        var axe = Nature플레이어감각표현Fixture.CreateAxePlan();
        var harvest = Nature플레이어감각표현Fixture.CreateHarvestPlan();

        validator.Validate(placement);
        validator.Validate(axe);
        validator.Validate(harvest);

        Assert.Equal(플레이어감각표현Validator.PlacementControlRevision,
            placement.PlacementControlRevision);
        Assert.Equal(placement.PlanHashSha256, axe.PlacementBindingPlanHash);
        Assert.Equal(placement.PlanHashSha256, harvest.PlacementBindingPlanHash);
        Assert.Contains(placement.Anchors,
            value => value.RoleCode == 배치표현AnchorRoleCodes.ToolSocket);
        Assert.Contains(placement.Anchors,
            value => value.RoleCode == 배치표현AnchorRoleCodes.AudioEmitter);
        Assert.Contains(placement.Anchors,
            value => value.RoleCode == 배치표현AnchorRoleCodes.Fx);
    }

    [Fact]
    public void 표현계획Hash는_입력배열순서와무관하다()
    {
        var placement = Nature플레이어감각표현Fixture.CreatePlacementPlan();
        var placementHash = placement.PlanHashSha256;
        placement.Anchors = placement.Anchors.Reverse().ToArray();
        placement.StructuralPlacementSourceStableIds = placement
            .StructuralPlacementSourceStableIds.Reverse().ToArray();
        placement.PlanHashSha256 = 플레이어감각표현Hasher.Compute(placement);
        validator.Validate(placement);

        var harvest = Nature플레이어감각표현Fixture.CreateHarvestPlan();
        var harvestHash = harvest.PlanHashSha256;
        harvest.Phases = harvest.Phases.Reverse().ToArray();
        foreach (var phase in harvest.Phases)
            phase.RequiredDomainCodes = phase.RequiredDomainCodes.Reverse().ToArray();
        harvest.PlanHashSha256 = 플레이어감각표현Hasher.Compute(harvest);
        validator.Validate(harvest);

        Assert.Equal(placementHash, placement.PlanHashSha256);
        Assert.Equal(harvestHash, harvest.PlanHashSha256);
    }

    [Fact]
    public void 음향Cue를선언하고Audio영역을누락하면_거부한다()
    {
        var harvest = Nature플레이어감각표현Fixture.CreateHarvestPlan();
        var working = harvest.Phases.Single(value =>
            value.PhaseCode == 감각표현단계Codes.Working);
        working.RequiredDomainCodes = working.RequiredDomainCodes.Where(value =>
            value != 표현규칙영역Codes.Audio).ToArray();
        harvest.PlanHashSha256 = 플레이어감각표현Hasher.Compute(harvest);

        var error = Assert.Throws<InvalidOperationException>(() =>
            validator.Validate(harvest));

        Assert.Equal("WiPresentationAudioDomainMissing", error.Message);
    }

    [Fact]
    public void 표현계획은_Animation이나Audio로권위완료를확정할수없다()
    {
        var harvest = Nature플레이어감각표현Fixture.CreateHarvestPlan();
        harvest.ConfirmsBusinessCompletion = true;
        harvest.PlanHashSha256 = 플레이어감각표현Hasher.Compute(harvest);

        var error = Assert.Throws<InvalidOperationException>(() =>
            validator.Validate(harvest));

        Assert.Equal("WiPresentationPlanInvalid", error.Message);
    }

    [Fact]
    public void 벌목은Animation과타격음이필수지만_Bgm은Area선택채널이다()
    {
        var harvest = Nature플레이어감각표현Fixture.CreateHarvestPlan();
        var working = harvest.Phases.Single(value =>
            value.PhaseCode == 감각표현단계Codes.Working);

        Assert.Contains(표현규칙영역Codes.Animation,
            working.RequiredDomainCodes);
        Assert.Contains(표현규칙영역Codes.Audio,
            working.RequiredDomainCodes);
        Assert.False(harvest.AreaMusicRequired);
        Assert.Equal(감각표현CueCodes.NatureExplorationMusic,
            harvest.AreaMusicCueCode);
    }
}
