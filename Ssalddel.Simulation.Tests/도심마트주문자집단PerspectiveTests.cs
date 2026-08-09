using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class 도심마트주문자집단PerspectiveTests
{
    [Fact]
    public void 주민은본인참여와집단조건만본다()
    {
        var state = Resident(true);

        Assert.True(state.IsAuthorized);
        Assert.Equal("participant:sim:001", state.ParticipantStableId);
        Assert.Equal(5m, state.MyIntentQuantity);
        Assert.Equal(5m, state.MyConfirmedQuantity);
        Assert.Equal(7, state.RequestedFulfillmentStartsAtTick);
        Assert.Equal(27, state.RequestedFulfillmentEndsAtTick);
        Assert.Equal("pickup-point:residential:sample-1", state.PickupPointStableId);
    }

    [Fact]
    public void 대표는의향과확정집계를분리해서본다()
    {
        var state = Representative(true);

        Assert.True(state.IsAuthorized);
        Assert.Equal(67, state.IntentParticipantCount);
        Assert.Equal(410m, state.IntentQuantity);
        Assert.Equal(61, state.ConfirmedParticipantCount);
        Assert.Equal(385m, state.ConfirmedQuantity);
        Assert.Equal(도심마트대표CanonicalRoleCodes.GroupPurchaseRepresentative,
            state.CanonicalRoleCode);
    }

    [Fact]
    public void 대표Action에는주민일괄확정주문변경결제가없다()
    {
        var actions = Representative(true).AvailableActionCodes;

        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.ReviewAggregateDemand, actions);
        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.ReviewInquiryStatus, actions);
        Assert.DoesNotContain(actions, action => action.Contains("ConfirmAll", StringComparison.Ordinal));
        Assert.DoesNotContain(actions, action => action.Contains("MemberOrder", StringComparison.Ordinal));
        Assert.DoesNotContain(actions, action => action.Contains("Payment", StringComparison.Ordinal));
        Assert.DoesNotContain(actions, action => action.Contains("Contract", StringComparison.Ordinal));
    }

    [Fact]
    public void 대표Capability가없으면Projection과Action을모두제거한다()
    {
        var state = Representative(false);

        Assert.False(state.IsAuthorized);
        Assert.Empty(state.OrdererGroupStableId);
        Assert.Empty(state.RepresentativeStableId);
        Assert.Empty(state.AvailableActionCodes);
        Assert.False(state.Dialogue.CanOpen);
    }

    [Fact]
    public void 마트관리자는확정수요공급검토Queue를받는다()
    {
        var state = Manager(true);

        Assert.True(state.IsAuthorized);
        Assert.Equal(도심마트주문자집단ManagerQueueCodes.ConfirmedDemandSupplyReviewRequired,
            state.QueueCode);
        Assert.Equal(410m, state.IntentQuantity);
        Assert.Equal(385m, state.ConfirmedQuantity);
        Assert.Contains(도심마트주문자집단ManagerReasonCodes.GroupConfirmedDemandPresent,
            state.PriorityReasonCodes);
        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.ReviewOrdererGroupDemand,
            state.AvailableActionCodes);
        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.PreviewSupplyPlan,
            state.AvailableActionCodes);
    }

    [Fact]
    public void 마트관리자Projection에는주민과대표Identity가없다()
    {
        var properties = typeof(마트관리자주문자집단PerspectiveWorldState)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(properties, name => name.Contains("ParticipantStableId", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, name => name.Contains("RepresentativeStableId", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, name => name.Contains("User", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Contact", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Address", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Payment", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void 대화는Surface를열뿐Command효과가없다()
    {
        var representative = Representative(true).Dialogue;
        var manager = Manager(true).Dialogue;

        Assert.True(representative.CanOpen);
        Assert.True(manager.CanOpen);
        Assert.Equal(도심마트DialogueCommandEffectCodes.None, representative.CommandEffectCode);
        Assert.Equal(도심마트DialogueCommandEffectCodes.None, manager.CommandEffectCode);
        Assert.Equal(representative.InquiryStableId, manager.InquiryStableId);
    }

    [Fact]
    public void OfferReady상태에서만대표에게조건전달과수령조율Action을준다()
    {
        var inquiry = 도심마트주문자집단PerspectiveSimulationFixture.Inquiry();
        inquiry.Inquiries[0].StateCode = 도심마트대표마트문의StateCodes.OfferReady;
        inquiry.Inquiries[0].MarketQuoteRevision = "market-quote:potato:1";
        var state = new 도심마트대표주문자집단PerspectiveInterpreter().Interpret(
            도심마트공동주택주문자집단SimulationFixture.Create(), inquiry, true);

        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.RelayMarketOffer,
            state.AvailableActionCodes);
        Assert.Contains(도심마트주문자집단PerspectiveActionCodes.CoordinatePickup,
            state.AvailableActionCodes);
        Assert.DoesNotContain(도심마트주문자집단PerspectiveActionCodes.PrepareMarketInquiry,
            state.AvailableActionCodes);
    }

    [Fact]
    public void 문의초안은마트관리자에게노출하거나Action을주지않는다()
    {
        var inquiry = 도심마트주문자집단PerspectiveSimulationFixture.Inquiry();
        inquiry.Inquiries[0].StateCode = 도심마트대표마트문의StateCodes.DraftPreparing;
        var state = new 마트관리자주문자집단PerspectiveInterpreter().Interpret(
            도심마트공동주택주문자집단SimulationFixture.Create(), inquiry, true);

        Assert.Equal(도심마트주문자집단ManagerQueueCodes.NoActionNeeded, state.QueueCode);
        Assert.Empty(state.AvailableActionCodes);
        Assert.Empty(state.PriorityReasonCodes);
        Assert.False(state.Dialogue.CanOpen);
    }

    [Fact]
    public void 같은입력은역할별같은Revision을만든다()
    {
        Assert.Equal(Resident(true).PerspectiveRevision, Resident(true).PerspectiveRevision);
        Assert.Equal(Representative(true).PerspectiveRevision,
            Representative(true).PerspectiveRevision);
        Assert.Equal(Manager(true).PerspectiveRevision, Manager(true).PerspectiveRevision);
    }

    [Fact]
    public void 다른Session과대표불일치문의는거부한다()
    {
        var inquiry = 도심마트주문자집단PerspectiveSimulationFixture.Inquiry();
        inquiry.SessionStableId = "simulation-session:other";
        AssertError("OrdererGroupPerspectiveSessionMismatch", () =>
            new 마트관리자주문자집단PerspectiveInterpreter().Interpret(
                도심마트공동주택주문자집단SimulationFixture.Create(), inquiry, true));

        inquiry = 도심마트주문자집단PerspectiveSimulationFixture.Inquiry();
        inquiry.Inquiries[0].RepresentativeStableId = "representative:sim:other";
        AssertError("OrdererGroupPerspectiveRepresentativeMismatch", () =>
            new 도심마트대표주문자집단PerspectiveInterpreter().Interpret(
                도심마트공동주택주문자집단SimulationFixture.Create(), inquiry, true));
    }

    private static 도심마트주민주문자집단PerspectiveWorldState Resident(bool authorized)
        => new 도심마트주민주문자집단PerspectiveInterpreter().Interpret(
            도심마트공동주택주문자집단SimulationFixture.Create(),
            도심마트주문자집단PerspectiveSimulationFixture.OwnParticipation(),
            authorized);

    private static 도심마트대표주문자집단PerspectiveWorldState Representative(bool authorized)
        => new 도심마트대표주문자집단PerspectiveInterpreter().Interpret(
            도심마트공동주택주문자집단SimulationFixture.Create(),
            도심마트주문자집단PerspectiveSimulationFixture.Inquiry(),
            authorized);

    private static 마트관리자주문자집단PerspectiveWorldState Manager(bool authorized)
        => new 마트관리자주문자집단PerspectiveInterpreter().Interpret(
            도심마트공동주택주문자집단SimulationFixture.Create(),
            도심마트주문자집단PerspectiveSimulationFixture.Inquiry(),
            authorized);

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
