using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation타로카드뽑기Tests
{
    [Fact]
    public void 시작Deck은_일반타로네종류를세장씩가진열두복사본이다()
    {
        var deck = new Simulation타로카드뽑기().CreateStarterDeck();

        Assert.Equal(12, deck.Length);
        Assert.Equal(12, deck.Select(value => value.CardCopyStableId).Distinct().Count());
        Assert.Equal(4, deck.Select(value => value.Card.CardStableId).Distinct().Count());
        Assert.All(deck.GroupBy(value => value.Card.CardStableId), group =>
            Assert.Equal(3, group.Count()));
        Assert.All(deck, value =>
            Assert.Equal(SimulationTurnCardKindCodes.Tarot, value.Card.CardKindCode));
    }

    [Fact]
    public void 같은Seed와Turn과이력은_같은세장과방향을뽑는다()
    {
        var draw = new Simulation타로카드뽑기();
        var first = draw.Draw(240812, 4, new[] { "offer:prior-1" });
        var second = draw.Draw(240812, 4, new[] { "offer:prior-1" });

        Assert.Equal(first.DrawStableId, second.DrawStableId);
        Assert.Equal(first.TurnHistoryHash, second.TurnHistoryHash);
        Assert.Equal(
            first.Offers.Select(Key),
            second.Offers.Select(Key));
        Assert.Equal(new[] { 1, 2, 3 }, first.Offers.Select(value => value.OfferSlotNumber));
    }

    [Fact]
    public void Seed나Turn이달라지면_결정적뽑기입력과결과식별자가분리된다()
    {
        var draw = new Simulation타로카드뽑기();
        var baseline = draw.Draw(240812, 4, Array.Empty<string>());
        var otherSeed = draw.Draw(240813, 4, Array.Empty<string>());
        var otherTurn = draw.Draw(240812, 5, Array.Empty<string>());

        Assert.NotEqual(baseline.DrawStableId, otherSeed.DrawStableId);
        Assert.NotEqual(baseline.DrawStableId, otherTurn.DrawStableId);
    }

    [Fact]
    public void 같은카드종류가중복제안되어도_복사본과Offer식별자는충돌하지않는다()
    {
        var draw = new Simulation타로카드뽑기();
        var result = Enumerable.Range(1, 20_000)
            .Select(seed => draw.Draw(seed, 1, Array.Empty<string>()))
            .First(value => value.Offers
                .GroupBy(offer => offer.Card.CardStableId)
                .Any(group => group.Count() > 1));

        Assert.Equal(3, result.Offers.Select(value => value.OfferStableId).Distinct().Count());
        Assert.Equal(3, result.Offers.Select(value => value.CardCopyStableId).Distinct().Count());
    }

    [Fact]
    public void 제안된카드와방향을선택하면_턴기록과활성효과에복사본계보를보존한다()
    {
        var context = Context();
        var draw = context.Service.GetTurnClosingContext(context.Session.SessionStableId).TarotDraw;
        var offer = draw.Offers[0];
        var request = Select(context.Session.Revision, offer);

        var closed = context.Service.ConfirmTurnClosing(
            context.Session.SessionStableId,
            new SimulationTurnClosingConfirmRequest
            {
                CommandId = "command:tarot-draw.select-1",
                ExpectedRevision = context.Session.Revision,
                Preview = request,
            });

        var selected = Assert.Single(Assert.Single(closed.TurnClosings).SelectedCards);
        Assert.Equal(offer.OfferStableId, selected.OfferStableId);
        Assert.Equal(offer.CardCopyStableId, selected.CardCopyStableId);
        Assert.Equal(offer.OrientationCode, selected.OrientationCode);
        var active = Assert.Single(closed.ActiveTurnCardEffects);
        Assert.Equal(selected.OfferStableId, active.OfferStableId);
        Assert.Equal(selected.CardCopyStableId, active.CardCopyStableId);
        Assert.Equal(selected.OrientationCode, active.OrientationCode);
    }

    [Fact]
    public void 제안되지않은Offer나변조된카드방향은_턴마감전에차단한다()
    {
        var context = Context();
        var offer = context.Service
            .GetTurnClosingContext(context.Session.SessionStableId)
            .TarotDraw.Offers[0];
        var unknown = Select(context.Session.Revision, offer);
        unknown.SelectedTarotCard!.OfferStableId = "tarot-draw:forged:offer-1";
        var mismatch = Select(context.Session.Revision, offer);
        mismatch.SelectedTarotCard!.OrientationCode = offer.OrientationCode
            == Simulation타로카드방향Codes.Upright
            ? Simulation타로카드방향Codes.Reversed
            : Simulation타로카드방향Codes.Upright;

        var unavailable = Assert.Throws<SimulationConflictException>(() =>
            context.Service.PreviewTurnClosing(context.Session.SessionStableId, unknown));
        var forged = Assert.Throws<SimulationConflictException>(() =>
            context.Service.PreviewTurnClosing(context.Session.SessionStableId, mismatch));

        Assert.Equal("SimulationTarotOfferUnavailable", unavailable.ErrorCode);
        Assert.Equal("SimulationTarotOfferMismatch", forged.ErrorCode);
    }

    [Fact]
    public void 저장재생뒤에도_선택계보와다음Turn뽑기가같다()
    {
        var context = Context();
        var offer = context.Service
            .GetTurnClosingContext(context.Session.SessionStableId)
            .TarotDraw.Offers[0];
        var closed = context.Service.ConfirmTurnClosing(
            context.Session.SessionStableId,
            new SimulationTurnClosingConfirmRequest
            {
                CommandId = "command:tarot-draw.save-replay",
                ExpectedRevision = context.Session.Revision,
                Preview = Select(context.Session.Revision, offer),
            });
        var saved = context.Service.Save(closed.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:tarot-draw-1",
                ExpectedRevision = closed.Revision,
            });
        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });

        var sourceNext = context.Service.GetTurnClosingContext(closed.SessionStableId).TarotDraw;
        var restoredNext = restoredService
            .GetTurnClosingContext(restored.Session.SessionStableId).TarotDraw;
        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
        Assert.Equal(sourceNext.DrawStableId, restoredNext.DrawStableId);
        Assert.Equal(sourceNext.Offers.Select(Key), restoredNext.Offers.Select(Key));
    }

    private static string Key(Simulation타로CardOfferSnapshot value)
        => value.OfferStableId + "|" + value.CardCopyStableId + "|"
            + value.Card.CardStableId + "|" + value.OrientationCode;

    private static SimulationTurnClosingPreviewRequest Select(
        long revision,
        Simulation타로CardOfferSnapshot offer)
        => new()
        {
            ExpectedRevision = revision,
            SelectedTarotCard = new Simulation타로CardSelectionRequest
            {
                OfferStableId = offer.OfferStableId,
                CardStableId = offer.Card.CardStableId,
                OrientationCode = offer.OrientationCode,
            },
        };

    private static TestContext Context()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var session = service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:tarot-draw-1",
            ScenarioDataRevision = "scenario-data:tarot-draw-r1",
            ScenarioSeed = 240812,
            RuleRevision = "simulation-rule:tarot-draw-r1",
            DurationTicks = 14,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.tarot-1",
                TerritoryStableId = "territory:sim.tarot-1",
                SettlementStableId = "settlement:sim.tarot-1",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
        });
        return new TestContext(service, saveStore, session);
    }

    private sealed record TestContext(
        경영SimulationSessionService Service,
        InMemorySimulationSessionSaveStore SaveStore,
        경영SimulationSessionSnapshot Session);
}
