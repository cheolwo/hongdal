using System;
using System.Linq;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Transport;
using Xunit;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class LogisticsFacilityOverviewTests
{
    private readonly LogisticsFacilityOverviewProjector projector = new();

    [Theory]
    [InlineData(CargoHandoffStateCodes.InTransit, LogisticsFacilityAreaCodes.VehicleGate)]
    [InlineData(CargoHandoffStateCodes.ArrivedAtWarehouse, LogisticsFacilityAreaCodes.InboundDock)]
    [InlineData(CargoHandoffStateCodes.ReceivingCompleted, LogisticsFacilityAreaCodes.Storage)]
    public void Handoff상태를_시설영역에명시적으로투영한다(string state, string area)
    {
        var result = projector.Project(Handoff(state))!;

        Assert.Equal(area, result.CurrentAreaCode);
        Assert.Equal(4, result.Areas.Length);
        Assert.Single(result.Areas, value => value.StateCode == LogisticsFacilityAreaStateCodes.Active);
        Assert.Contains("NPC 도착만으로 입고 완료되지 않음", result.BoundaryText);
    }

    [Fact]
    public void 입고완료는_접근과Dock과검수를완료표시하고보관을활성화한다()
    {
        var result = projector.Project(Handoff(CargoHandoffStateCodes.ReceivingCompleted))!;

        Assert.Equal(3, result.Areas.Count(value =>
            value.StateCode == LogisticsFacilityAreaStateCodes.Completed));
        Assert.Equal(LogisticsFacilityAreaStateCodes.Active,
            result.Areas.Single(value => value.AreaCode == LogisticsFacilityAreaCodes.Storage).StateCode);
    }

    [Fact]
    public void Handoff가없으면_가상의화물상태를만들지않는다()
        => Assert.Null(projector.Project(null));

    [Fact]
    public void 알수없는상태를거부한다()
    {
        var error = Assert.Throws<InvalidOperationException>(() => projector.Project(Handoff("Unknown")));
        Assert.Equal("LogisticsFacilityHandoffStateInvalid:Unknown", error.Message);
    }

    private static CargoWarehouseHandoffSnapshot Handoff(string state)
        => new()
        {
            StableId = "cargo-handoff:transport-71.inbound-91",
            Revision = 3,
            HandoffStateCode = state,
            CargoStableId = "cargo:transport-71",
            TransportTaskStableId = "transport-task:71",
            InboundTaskStableId = "inbound-task:91",
            GeneratedAt = new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
        };
}
