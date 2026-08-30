using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "WI 단위 실행 우선순위와 복수 활성 작업 조회 계약의 회귀를 검증한다.",
    Boundary = "우선순위 대장은 E 단계 달성이나 실제 Play Mode 증거를 대신하지 않는다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3계약회귀)]
public sealed class SimulationWorldInteractionDeliveryPriorityTests
{
    [Fact]
    public void WI_105개는_한번씩배정되고_활성작업목록이_실행상태를_소유한다()
    {
        var all = SimulationWI실행우선순위Catalog.All;

        Assert.Equal(105, all.Count);
        Assert.Equal(105, all.Select(item => item.WorldInteractionId)
            .Distinct(StringComparer.Ordinal).Count());
        Assert.Null(SimulationWI실행우선순위Catalog.MaximumConcurrentWorkItems);
        Assert.Equal("DependencyAndOwnership", SimulationWI실행우선순위Catalog.ConcurrencyModeCode);
        var activeIds = SimulationWI실행우선순위Catalog.ActiveWorldInteractionIds;
        Assert.Equal(activeIds.Count, activeIds.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(activeIds.OrderBy(id => id, StringComparer.Ordinal),
            all.Where(item => item.개발작업상태Code == "Active")
                .Select(item => item.WorldInteractionId).OrderBy(id => id, StringComparer.Ordinal));
        Assert.All(activeIds, id =>
        {
            Assert.NotNull(Simulation세계상호작용이름Catalog.Find(id));
            var active = SimulationWI실행우선순위Catalog.Find(id)!;
            Assert.True(SimulationWI실행우선순위Catalog.IsActiveWorldInteraction(id));
            Assert.True(active.파동내순서 > 0);
            Assert.Equal("E7", active.목표EvidenceStage);
            Assert.NotEqual("DeferredRegistration", active.완결역할Code);
            Assert.NotEqual("DeferredIntegration", active.완결역할Code);
            Assert.NotEmpty(active.폐루프StableIds);
        });
        Assert.False(SimulationWI실행우선순위Catalog.IsActiveWorldInteraction("WI-UNKNOWN"));
    }

    [Fact]
    public void 등록대기WI는_실행되지않고_상위분류와_중복프로필은_실행목록에없다()
    {
        var 신규등록 = SimulationWI실행우선순위Catalog.All
            .Where(항목 => 항목.완결역할Code == "DeferredRegistration").ToArray();
        Assert.InRange(신규등록.Length, 0, 32);
        Assert.All(신규등록, 항목 =>
        {
            Assert.Equal("Deferred", 항목.개발작업상태Code);
            Assert.Empty(항목.폐루프StableIds);
            Assert.NotNull(Simulation세계상호작용이름Catalog.Find(항목.WorldInteractionId));
        });
        Assert.Null(Simulation세계상호작용이름Catalog.Find("WI-DEFENSE-SEGMENT-REPAIR"));
        Assert.Null(Simulation세계상호작용이름Catalog.Find("WI-WORLD-PATTERN-PLACEMENT-CONFIRM"));
        Assert.Null(Simulation세계상호작용이름Catalog.Find("WI-HUB-DEMAND-REMAINDER-RETURN"));
        Assert.Null(Simulation세계상호작용이름Catalog.Find("WI-ROUTE-SAFETY-IMPROVE"));
        Assert.Equal("손상된 시설 수리", Simulation세계상호작용이름Catalog.Find("WI-WORLD-04")!.한국어기능명);
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
