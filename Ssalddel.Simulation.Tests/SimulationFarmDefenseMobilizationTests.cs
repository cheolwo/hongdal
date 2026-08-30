using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "WI-FARM-DEFENSE-MOBILIZE의 Preview·Confirm·멱등·거부·결정적 표현을 검증한다.",
    Boundary = "Logic·Presentation E3 자동시험이며 Save·RemoteHost·공간·Game View 증거가 아니다.",
    WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
public sealed class SimulationFarmDefenseMobilizationTests
{
    [Fact]
    public void Preview는_권위상태와_revision을_바꾸지_않는다()
    {
        var aggregate = CreateAggregate();
        var before = aggregate.Snapshot();

        var preview = aggregate.Preview(Preview());
        var after = aggregate.Snapshot();

        Assert.True(preview.CanConfirm);
        Assert.Equal(new[] { "worker:a", "worker:b" },
            preview.SuspendedWorkerStableIds);
        Assert.Equal(before.WorldRevision, after.WorldRevision);
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
        Assert.Empty(after.SuspendedProductionWorkerStableIds);
        Assert.Empty(after.ActionLedger.TailRecords);
    }

    [Fact]
    public void Confirm은_분대를_출동시키고_배정인력의_생산기여를_중단한다()
    {
        var aggregate = CreateAggregate();

        var result = aggregate.Confirm(Confirm("command:defense:mobilize"));

        Assert.False(result.Reused);
        Assert.Equal(12, result.Ledger.WorldRevision);
        var squad = Assert.Single(result.Ledger.Squads);
        Assert.Equal(SimulationFarm방위소집Codes.출동, squad.StatusCode);
        Assert.Equal("threat:wolves", squad.MobilizedThreatStableId);
        Assert.Equal(new[] { "worker:a", "worker:b" },
            result.Ledger.SuspendedProductionWorkerStableIds);
        Assert.Equal(SimulationFarm방위소집Codes.WorldInteractionId,
            result.ActionRecord.WorldInteractionId);
        Assert.Equal(11, result.ActionRecord.BeforeWorldRevision);
        Assert.Equal(12, result.ActionRecord.AfterWorldRevision);
        Assert.Single(result.Ledger.ActionLedger.TailRecords);
    }

    [Fact]
    public void 같은_Command는_결과를_재사용하고_중복기록을_만들지_않는다()
    {
        var aggregate = CreateAggregate();
        var request = Confirm("command:defense:idem");
        var first = aggregate.Confirm(request);

        var second = aggregate.Confirm(request);

        Assert.True(second.Reused);
        Assert.Equal(first.Ledger.StateHashSha256,
            second.Ledger.StateHashSha256);
        Assert.Single(second.Ledger.ActionLedger.TailRecords);
        request.ThreatStableId = "threat:other";
        var conflict = Assert.Throws<SimulationConflictException>(
            () => aggregate.Confirm(request));
        Assert.Equal(SimulationFarm방위소집Codes.CommandPayloadConflict,
            conflict.ErrorCode);
    }

    [Theory]
    [InlineData(10, "squad:alpha", "threat:wolves",
        SimulationFarm방위소집Codes.ExpectedRevisionMismatch)]
    [InlineData(11, "squad:unknown", "threat:wolves",
        SimulationFarm방위소집Codes.SquadUnknown)]
    [InlineData(11, "squad:alpha", "threat:unknown",
        SimulationFarm방위소집Codes.ThreatUnknown)]
    public void 거부경계는_상태를_바꾸지_않는다(long revision,
        string squad, string threat, string errorCode)
    {
        var aggregate = CreateAggregate();
        var before = aggregate.Snapshot();
        var request = Confirm("command:defense:blocked");
        request.ExpectedWorldRevision = revision;
        request.SquadStableId = squad;
        request.ThreatStableId = threat;

        var error = Assert.Throws<SimulationConflictException>(
            () => aggregate.Confirm(request));

        Assert.Equal(errorCode, error.ErrorCode);
        Assert.Equal(before.StateHashSha256,
            aggregate.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 준비되지않은_분대는_출동할수없다()
    {
        var request = Initial();
        request.Squads[0].IsReady = false;
        var aggregate = new SimulationFarm방위소집Aggregate(request);

        var preview = aggregate.Preview(Preview());

        Assert.False(preview.CanConfirm);
        Assert.Contains(SimulationFarm방위소집Codes.SquadNotReady,
            preview.BlockReasonCodes);
    }

    [Fact]
    public void 카드투영은_StableId순서이며_권위상태를_바꾸지_않는다()
    {
        var service = new SimulationFarm방위소집Service(
            new InMemorySimulationFarm방위소집Store());
        var request = Initial();
        request.Squads = new[]
        {
            new SimulationFarm방위분대Definition
            {
                SquadStableId = "squad:zulu", IsReady = true,
                AssignedWorkerStableIds = new[] { "worker:z" },
            },
            request.Squads[0],
        };
        service.Create("ledger:farm-defense:test", request);
        var before = service.Get("ledger:farm-defense:test");

        var cards = service.ProjectCards("ledger:farm-defense:test");
        var after = service.Get("ledger:farm-defense:test");

        Assert.Equal(new[] { "squad:alpha", "squad:zulu" },
            cards.Select(value => value.SquadStableId));
        Assert.All(cards, value => Assert.Equal(11,
            value.SourceWorldRevision));
        Assert.Equal(before.StateHashSha256, after.StateHashSha256);
    }

    private static SimulationFarm방위소집Aggregate CreateAggregate()
        => new(Initial());

    private static SimulationFarm방위소집InitialStateRequest Initial()
        => new()
        {
            WorldStableId = "world:farm",
            SessionStableId = "session:farm-defense",
            InitialWorldRevision = 11,
            ApproachingThreatStableIds = new[] { "threat:wolves" },
            Squads = new[]
            {
                new SimulationFarm방위분대Definition
                {
                    SquadStableId = "squad:alpha",
                    IsReady = true,
                    AssignedWorkerStableIds = new[]
                        { "worker:b", "worker:a", "worker:a" },
                },
            },
        };

    private static SimulationFarm방위소집PreviewRequest Preview()
        => new()
        {
            ObservedWorldRevision = 11,
            SquadStableId = "squad:alpha",
            ThreatStableId = "threat:wolves",
        };

    private static SimulationFarm방위소집ConfirmRequest Confirm(string command)
        => new()
        {
            CommandId = command,
            ExpectedWorldRevision = 11,
            SquadStableId = "squad:alpha",
            ThreatStableId = "threat:wolves",
        };
}
