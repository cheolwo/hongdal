using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.PresentationContracts.Cargo;

namespace Ssalddel.Tests.UnityData;

public sealed class CargoJourneyPresentationTests
{
    [Fact]
    public void 같은Cargo를_네Zone에하나의Identity와Lineage로투영한다()
    {
        var result = Project(CargoHandoffStateCodes.InTransit);

        Assert.Equal("cargo:transport-71", result.CargoStableId);
        Assert.Equal("cargo:transport-71", result.Identity.WorldId.Value);
        Assert.Equal(6, result.Identity.SourceIds.Length);
        Assert.Equal(4, result.Anchors.Length);
        Assert.All(result.Anchors, value =>
        {
            Assert.Equal(result.CargoStableId, value.CargoStableId);
            Assert.Equal(result.Identity.WorldId, Assert.Single(value.Identity.SourceWorldIds));
        });
    }

    [Fact]
    public void 운송중과창고도착은_현재Zone만바꾸고CargoIdentity를유지한다()
    {
        var transit = Project(CargoHandoffStateCodes.InTransit);
        var arrived = Project(CargoHandoffStateCodes.ArrivedAtWarehouse);

        Assert.Equal(CargoJourneyZoneCodes.TransportCorridor, transit.CurrentZoneCode);
        Assert.Equal(CargoJourneyZoneCodes.UrbanLogistics, arrived.CurrentZoneCode);
        Assert.Equal(transit.Identity.WorldId, arrived.Identity.WorldId);
        Assert.Equal(transit.CargoStableId, arrived.CargoStableId);
        Assert.Single(transit.Anchors, value => value.IsCurrent);
        Assert.Single(arrived.Anchors, value => value.IsCurrent);
    }

    [Fact]
    public void 입고완료만으로_Market도착을만들지않는다()
    {
        var result = Project(CargoHandoffStateCodes.ReceivingCompleted);
        var market = result.Anchors.Single(value => value.ZoneCode == CargoJourneyZoneCodes.UrbanMarket);

        Assert.Equal(CargoJourneyAnchorStateCodes.Planned, market.StateCode);
        Assert.False(market.IsCurrent);
        Assert.Equal(CargoJourneyZoneCodes.UrbanLogistics, result.CurrentZoneCode);
    }

    [Fact]
    public void Origin과Product는_명시적인StableSource여야한다()
    {
        var input = Input(CargoHandoffStateCodes.InTransit);
        input.OriginSourceStableId = string.Empty;
        Assert.StartsWith("CargoJourneyOriginSourceStableIdInvalid",
            Assert.Throws<InvalidOperationException>(() => new CargoJourneyProjector().Project(input)).Message);

        input = Input(CargoHandoffStateCodes.InTransit);
        input.ProductStableId = "potato";
        Assert.StartsWith("CargoJourneyProductStableIdInvalid",
            Assert.Throws<InvalidOperationException>(() => new CargoJourneyProjector().Project(input)).Message);
    }

    private static CargoJourneyPresentationModel Project(string state)
        => new CargoJourneyProjector().Project(Input(state));

    private static CargoJourneyProjectionInput Input(string state)
        => new()
        {
            Mode = DataRuntimeMode.Simulation,
            ProductStableId = "product:potato",
            OriginSourceStableId = "farm-handoff:sim.potato.1",
            Handoff = new CargoWarehouseHandoffSnapshot
            {
                StableId = "cargo-handoff:transport-71.inbound-91",
                Revision = 5,
                HandoffStateCode = state,
                CargoStableId = "cargo:transport-71",
                TransportTaskStableId = "transport-task:71",
                InboundTaskStableId = "inbound-task:91",
                GeneratedAt = DateTimeOffset.Parse("2026-08-09T00:00:00Z"),
            },
        };
}
