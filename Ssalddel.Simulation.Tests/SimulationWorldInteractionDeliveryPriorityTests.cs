using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "WI 단위 실행 우선순위와 WIP 한도 계약의 회귀를 검증한다.",
    Boundary = "우선순위 대장은 E 단계 달성이나 실제 Play Mode 증거를 대신하지 않는다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3계약회귀)]
public sealed class SimulationWorldInteractionDeliveryPriorityTests
{
    [Fact]
    public void WI_66개는_한번씩배정되고_지식습득하나만E5다음관문으로활성이다()
    {
        var all = SimulationWI실행우선순위Catalog.All;

        Assert.Equal(66, all.Count);
        Assert.Equal(66, all.Select(item => item.WorldInteractionId)
            .Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(1, SimulationWI실행우선순위Catalog.WorkInProgressLimit);
        Assert.Equal("WI-ACTOR-03",
            SimulationWI실행우선순위Catalog.ActiveWorldInteractionId);
        Assert.Equal("E5",
            SimulationWI실행우선순위Catalog.ActiveEvidenceStage);

        var active = Assert.Single(all, item =>
            item.개발작업상태Code == "Active");
        Assert.Equal("WI-ACTOR-03", active.WorldInteractionId);
        Assert.Equal("D1", active.실행파동Code);
        Assert.Equal(23, active.파동내순서);
        Assert.Equal("E7", active.목표EvidenceStage);
        Assert.Equal("NotApplicable", active.NpcE8정책Code);
    }

    [Fact]
    public void NPC와선택형NPC_WI는_E8정책을분리한다()
    {
        Assert.Equal("Required", SimulationWI실행우선순위Catalog
            .Find("WI-NATURE-17")!.NpcE8정책Code);
        Assert.Equal("Required", SimulationWI실행우선순위Catalog
            .Find("WI-HUB-04")!.NpcE8정책Code);
        Assert.Equal("Conditional", SimulationWI실행우선순위Catalog
            .Find("WI-FARM-04")!.NpcE8정책Code);
        Assert.Equal("NotApplicable", SimulationWI실행우선순위Catalog
            .Find("WI-CITY-02")!.NpcE8정책Code);
    }
}
