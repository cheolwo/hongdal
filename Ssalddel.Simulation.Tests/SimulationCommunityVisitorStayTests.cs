using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "WI-COMMUNITY-VISITOR-STAY의 Preview·Confirm·멱등·거부·결정적 표현을 검증한다.",
    Boundary = "Logic·Presentation E3 자동시험이며 Save·RemoteHost·공간·Game View 증거가 아니다.",
    WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
public sealed class SimulationCommunityVisitorStayTests
{
    [Fact]
    public void Preview는_권위상태와_revision을_바꾸지_않는다()
    {
        var aggregate = CreateAggregate();
        var before = aggregate.Snapshot();

        var preview = aggregate.Preview(Preview(
            Simulation공동체방문자체류Codes.임시체류수용));
        var after = aggregate.Snapshot();

        Assert.True(preview.CanConfirm);
        Assert.Equal(Simulation공동체방문자체류Codes.임시체류,
            preview.ProjectedStatusCode);
        Assert.Equal(Simulation공동체방문자체류Codes.환대확인,
            preview.ProjectedMindTraceCode);
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
        Assert.Equal(before.WorldRevision, after.WorldRevision);
        Assert.Empty(after.ActionLedger.TailRecords);
    }

    [Fact]
    public void 수용Confirm은_방문자를_임시체류로_바꾸고_수용칸과_마음계보를_남긴다()
    {
        var aggregate = CreateAggregate();

        var result = aggregate.Confirm(Confirm("command:visitor:accept",
            Simulation공동체방문자체류Codes.임시체류수용));

        Assert.False(result.Reused);
        Assert.Equal(8, result.Ledger.WorldRevision);
        Assert.Equal(2, result.Ledger.OccupiedGuestCapacity);
        var visitor = Assert.Single(result.Ledger.Visitors);
        Assert.Equal(Simulation공동체방문자체류Codes.임시체류,
            visitor.StatusCode);
        Assert.Equal(Simulation공동체방문자체류Codes.환대확인,
            visitor.MindTraceCode);
        Assert.Equal(Simulation공동체방문자체류Codes.WorldInteractionId,
            result.ActionRecord.WorldInteractionId);
        Assert.Equal("player:host", result.ActionRecord.InitiatorStableId);
        Assert.Equal(7, result.ActionRecord.BeforeWorldRevision);
        Assert.Equal(8, result.ActionRecord.AfterWorldRevision);
        Assert.Single(result.Ledger.ActionLedger.TailRecords);
    }

    [Fact]
    public void 거절Confirm은_수용칸을_쓰지않고_경계보호_계보를_남긴다()
    {
        var aggregate = CreateAggregate();

        var result = aggregate.Confirm(Confirm("command:visitor:reject",
            Simulation공동체방문자체류Codes.거절선택));

        Assert.Equal(1, result.Ledger.OccupiedGuestCapacity);
        var visitor = Assert.Single(result.Ledger.Visitors);
        Assert.Equal(Simulation공동체방문자체류Codes.거절,
            visitor.StatusCode);
        Assert.Equal(Simulation공동체방문자체류Codes.경계보호,
            visitor.MindTraceCode);
    }

    [Fact]
    public void 같은_Command는_결과를_재사용하고_다른_payload는_거부한다()
    {
        var aggregate = CreateAggregate();
        var request = Confirm("command:visitor:idem",
            Simulation공동체방문자체류Codes.임시체류수용);
        var first = aggregate.Confirm(request);

        var second = aggregate.Confirm(request);

        Assert.True(second.Reused);
        Assert.Equal(first.Ledger.StateHashSha256,
            second.Ledger.StateHashSha256);
        Assert.Single(second.Ledger.ActionLedger.TailRecords);
        request.DecisionCode = Simulation공동체방문자체류Codes.거절선택;
        var conflict = Assert.Throws<SimulationConflictException>(
            () => aggregate.Confirm(request));
        Assert.Equal(Simulation공동체방문자체류Codes.CommandPayloadConflict,
            conflict.ErrorCode);
    }

    [Theory]
    [InlineData(6, "visitor:one",
        Simulation공동체방문자체류Codes.임시체류수용,
        Simulation공동체방문자체류Codes.ExpectedRevisionMismatch)]
    [InlineData(7, "visitor:unknown",
        Simulation공동체방문자체류Codes.임시체류수용,
        Simulation공동체방문자체류Codes.VisitorUnknown)]
    [InlineData(7, "visitor:one", "JoinPermanently",
        Simulation공동체방문자체류Codes.DecisionInvalid)]
    public void 거부경계는_상태를_바꾸지_않는다(long revision,
        string visitor, string decision, string errorCode)
    {
        var aggregate = CreateAggregate();
        var before = aggregate.Snapshot();
        var request = Confirm("command:visitor:blocked", decision);
        request.ExpectedWorldRevision = revision;
        request.VisitorStableId = visitor;

        var error = Assert.Throws<SimulationConflictException>(
            () => aggregate.Confirm(request));

        Assert.Equal(errorCode, error.ErrorCode);
        Assert.Equal(before.StateHashSha256,
            aggregate.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 수용여력이_없으면_수용만_차단되고_거절은_가능하다()
    {
        var initial = Initial();
        initial.OccupiedGuestCapacity = initial.GuestCapacity;
        var aggregate = new Simulation공동체방문자체류Aggregate(initial);

        var accept = aggregate.Preview(Preview(
            Simulation공동체방문자체류Codes.임시체류수용));
        var reject = aggregate.Preview(Preview(
            Simulation공동체방문자체류Codes.거절선택));

        Assert.False(accept.CanConfirm);
        Assert.Contains(Simulation공동체방문자체류Codes.CapacityUnavailable,
            accept.BlockReasonCodes);
        Assert.True(reject.CanConfirm);
    }

    [Fact]
    public void 이미_결정된_방문자는_다시_결정할수없다()
    {
        var aggregate = CreateAggregate();
        aggregate.Confirm(Confirm("command:visitor:first",
            Simulation공동체방문자체류Codes.거절선택));

        var preview = aggregate.Preview(new Simulation공동체방문자체류PreviewRequest
        {
            ObservedWorldRevision = 8,
            VisitorStableId = "visitor:one",
            DecisionCode = Simulation공동체방문자체류Codes.임시체류수용,
        });

        Assert.False(preview.CanConfirm);
        Assert.Contains(Simulation공동체방문자체류Codes.VisitorAlreadyDecided,
            preview.BlockReasonCodes);
    }

    [Fact]
    public void 카드투영은_StableId순서이며_권위상태를_바꾸지_않는다()
    {
        var service = new Simulation공동체방문자체류Service(
            new InMemorySimulation공동체방문자체류Store());
        var initial = Initial();
        initial.Visitors = new[]
        {
            new Simulation공동체방문자Definition
                { VisitorStableId = "visitor:zulu" },
            new Simulation공동체방문자Definition
                { VisitorStableId = "visitor:alpha" },
        };
        service.Create("ledger:visitor:test", initial);
        var before = service.Get("ledger:visitor:test");

        var cards = service.ProjectCards("ledger:visitor:test");
        var after = service.Get("ledger:visitor:test");

        Assert.Equal(new[] { "visitor:alpha", "visitor:zulu" },
            cards.Select(value => value.VisitorStableId));
        Assert.All(cards, value => Assert.Equal(7,
            value.SourceWorldRevision));
        Assert.All(cards, value => Assert.Equal(2,
            value.RemainingGuestCapacity));
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
    }

    private static Simulation공동체방문자체류Aggregate CreateAggregate()
        => new(Initial());

    private static Simulation공동체방문자체류InitialStateRequest Initial()
        => new()
        {
            WorldStableId = "world:nature-camp",
            SessionStableId = "session:community-visitor",
            HostPlayerStableId = "player:host",
            InitialWorldRevision = 7,
            GuestCapacity = 3,
            OccupiedGuestCapacity = 1,
            Visitors = new[]
            {
                new Simulation공동체방문자Definition
                    { VisitorStableId = "visitor:one" },
            },
        };

    private static Simulation공동체방문자체류PreviewRequest Preview(
        string decision) => new()
        {
            ObservedWorldRevision = 7,
            VisitorStableId = "visitor:one",
            DecisionCode = decision,
        };

    private static Simulation공동체방문자체류ConfirmRequest Confirm(
        string command, string decision) => new()
        {
            CommandId = command,
            ExpectedWorldRevision = 7,
            VisitorStableId = "visitor:one",
            DecisionCode = decision,
        };
}
