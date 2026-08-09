using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.UrbanMarket;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Unity.Tests;

public sealed class UrbanMarketResidentialRepresentativeNpcTests
{
    [Fact]
    public void 대표경로는기존경로를바꾸지않고두Zone에추가된다()
    {
        Assert.Empty(ZoneNpcRouteCatalog.Validate());
        var residential = Assert.Single(ZoneNpcRouteCatalog.All, route =>
            route.RouteCode == ResidentialGroupRepresentativeNpcCodes.ResidentialRoute);
        var market = Assert.Single(ZoneNpcRouteCatalog.All, route =>
            route.RouteCode == ResidentialGroupRepresentativeNpcCodes.MarketRoute);

        Assert.Equal(WorldZoneCodes.ResidentialCommunity, residential.WorldZoneCode);
        Assert.Equal(WorldZoneCodes.MarketOrder, market.WorldZoneCode);
        Assert.Contains(ZoneNpcRouteCatalog.All, route => route.RouteCode == "residential-orderer-pickup");
        Assert.Contains(ZoneNpcRouteCatalog.All, route => route.RouteCode == "market-orderer-browse");
    }

    [Fact]
    public void 대표방문은같은Npc를두개의ZoneLeg로연결한다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();

        Assert.Equal(visit.NpcStableId, visit.ResidentialBriefingLeg.NpcStableId);
        Assert.Equal(visit.NpcStableId, visit.MarketConsultationLeg.NpcStableId);
        Assert.NotEqual(visit.ResidentialBriefingLeg.WorldZoneCode,
            visit.MarketConsultationLeg.WorldZoneCode);
        Assert.Equal("representative-visit:sim:potato:1", visit.RepresentativeVisitStableId);
    }

    [Fact]
    public void 관리자검토대기에서는마트Leg만활성이다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();

        Assert.Same(visit.MarketConsultationLeg, visit.ActiveMovement());
        Assert.Equal("market.manager-desk", visit.ActiveMovement().DestinationWaypointKey);
        Assert.Equal(ResidentialGroupRepresentativeArrivalActionCodes.WaitForManagerReview,
            visit.ActiveMovement().ArrivalActionCode);
    }

    [Fact]
    public void 두Leg는Simulation이며CanonicalTask를주장하지않는다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();

        Assert.All(new[] { visit.ResidentialBriefingLeg, visit.MarketConsultationLeg }, leg =>
        {
            Assert.Equal(NpcMovementSourceTypeCodes.SimulatedFixture, leg.SourceTypeCode);
            Assert.Empty(leg.CanonicalTaskStableId);
        });
    }

    [Fact]
    public void 도착Action은표현입력이고업무Command효과가없다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();

        Assert.Equal(RepresentativeVisitCommandEffectCodes.None, visit.CommandEffectCode);
        Assert.NotEmpty(visit.ResidentialBriefingLeg.ArrivalActionCode);
        Assert.NotEmpty(visit.MarketConsultationLeg.ArrivalActionCode);
    }

    [Fact]
    public void 다른Npc의Leg를방문에붙이면거부한다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();
        visit.MarketConsultationLeg.NpcStableId = "npc:sim:other";

        AssertError("RepresentativeVisitMovementLegMismatch", () =>
            new ResidentialGroupRepresentativeVisitValidator().Validate(visit));
    }

    [Fact]
    public void 업무Stage와활성ZoneLeg가다르면거부한다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();
        visit.ActiveLegCode = ResidentialGroupRepresentativeVisitLegCodes.ResidentialBriefing;

        AssertError("RepresentativeVisitJourneyLegMismatch", () =>
            new ResidentialGroupRepresentativeVisitValidator().Validate(visit));
    }

    [Fact]
    public void 방문에ServerCommand효과를지정하면거부한다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();
        visit.CommandEffectCode = "ServerCommand";

        AssertError("RepresentativeVisitCommandEffectForbidden", () =>
            new ResidentialGroupRepresentativeVisitValidator().Validate(visit));
    }

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(expected, exception.Message);
    }
}
