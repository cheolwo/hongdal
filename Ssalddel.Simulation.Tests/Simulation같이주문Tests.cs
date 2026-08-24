using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation같이주문Tests
{
    [Fact]
    public void Preview는_참여자의향과목표충족을집계하지만_원장을변경하지않는다()
    {
        var context = CreateContext();

        var preview = context.Service.PreviewGroupOrder(
            context.Session.SessionStableId,
            GroupOrder(20m, 20m, 20m));
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(3, preview.ParticipantCount);
        Assert.Equal(60m, preview.TotalQuantity);
        Assert.True(preview.ExplicitTargetMet);
        Assert.Equal(같이주문상태코드.확정대기, preview.SuggestedStateCode);
        Assert.Equal(같이주문상태코드.확정, preview.CompletionStateCode);
        Assert.Contains("AutomaticParticipantConsent", preview.ExcludedOperationalEffectCodes);
        Assert.Empty(current.GroupOrders);
    }

    [Fact]
    public void 목표충족Confirm은_의향별수량을보존하고_Tick에서확정한다()
    {
        var context = CreateContext();

        var scheduled = context.Service.ConfirmGroupOrder(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision, GroupOrder(20m, 20m, 20m)));
        var group = Assert.Single(scheduled.GroupOrders);

        Assert.Equal(같이주문상태코드.확정대기, group.StateCode);
        Assert.Equal(3, group.Intents.Length);
        Assert.Equal(60m, group.Intents.Sum(value => value.Quantity));
        Assert.All(group.Intents, value => Assert.True(value.ExplicitParticipationConsent));
        Assert.Equal(
            SimulationTaskStateCodes.Scheduled,
            scheduled.Tasks.Single(value => value.TaskStableId == group.TaskStableId).StateCode);

        var completed = Advance(context, scheduled, "command:tick.group-order-complete");
        var confirmed = Assert.Single(completed.GroupOrders);
        Assert.Equal(같이주문상태코드.확정, confirmed.StateCode);
        Assert.Equal(completed.WorldContext.WorldTick, confirmed.FinalizedTick);
    }

    [Fact]
    public void 목표미달Confirm은_Tick에서_모집종료목표미달로귀결된다()
    {
        var context = CreateContext();
        var request = GroupOrder(15m, 15m);

        var preview = context.Service.PreviewGroupOrder(context.Session.SessionStableId, request);
        var scheduled = context.Service.ConfirmGroupOrder(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision, request));
        var completed = Advance(context, scheduled, "command:tick.group-order-shortfall");

        Assert.Equal(같이주문상태코드.수요수집중, preview.SuggestedStateCode);
        Assert.Equal(같이주문상태코드.모집종료목표미달, preview.CompletionStateCode);
        Assert.False(preview.TargetParticipantCountMet);
        Assert.False(preview.TargetQuantityMet);
        Assert.Equal(
            같이주문상태코드.모집종료목표미달,
            Assert.Single(completed.GroupOrders).StateCode);
    }

    [Fact]
    public void 명시적참여동의가없는의향은_Preview에서차단되고Confirm할수없다()
    {
        var context = CreateContext();
        var request = GroupOrder(20m, 20m, 20m);
        request.Intents[1].ExplicitParticipationConsent = false;

        var preview = context.Service.PreviewGroupOrder(context.Session.SessionStableId, request);
        var error = Assert.Throws<SimulationConflictException>(() =>
            context.Service.ConfirmGroupOrder(
                context.Session.SessionStableId,
                Confirm(context.Session.Revision, request)));

        Assert.Contains("SimulationGroupOrderExplicitConsentRequired", preview.BlockReasonCodes);
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Empty(context.Service.Get(context.Session.SessionStableId).GroupOrders);
    }

    [Fact]
    public void 같은참여자의중복의향은_자동합산하지않고차단한다()
    {
        var context = CreateContext();
        var request = GroupOrder(20m, 20m, 20m);
        request.Intents[2].ParticipantStableId = request.Intents[1].ParticipantStableId;

        var preview = context.Service.PreviewGroupOrder(context.Session.SessionStableId, request);

        Assert.Contains("SimulationGroupOrderParticipantDuplicate", preview.BlockReasonCodes);
        Assert.Empty(context.Service.Get(context.Session.SessionStableId).GroupOrders);
    }

    [Fact]
    public void SaveReplay는_확정된같이주문과_참여자의향계보를동일하게복원한다()
    {
        var context = CreateContext();
        var scheduled = context.Service.ConfirmGroupOrder(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision, GroupOrder(20m, 20m, 20m)));
        var completed = Advance(context, scheduled, "command:tick.group-order-save");
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.group-order-1",
                ExpectedRevision = completed.Revision,
            });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            context.SaveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var group = Assert.Single(restored.Session.GroupOrders);
        Assert.Equal(같이주문상태코드.확정, group.StateCode);
        Assert.Equal(3, group.ParticipantCount);
        Assert.Equal(60m, group.TotalQuantity);
        Assert.Equal(3, group.Intents.Length);
        Assert.All(group.Intents, value => Assert.Contains(
            value.ParticipantStableId,
            value.SourceStableIds));
    }

    private static Context CreateContext()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var session = service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.group-order-1",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260811,
            RuleRevision = "rule:r1",
            DurationTicks = 14,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.residents-1",
                TerritoryStableId = "territory:sim.town-1",
                SettlementStableId = "settlement:sim.town-1",
                GameDateStartsOn = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
        });
        return new Context(service, saveStore, session);
    }

    private static Simulation같이주문ConfirmRequest Confirm(
        long revision,
        Simulation같이주문PreviewRequest request)
        => new()
        {
            CommandId = "command:group-order.finalize-1",
            ExpectedRevision = revision,
            GroupOrder = request,
        };

    private static Simulation같이주문PreviewRequest GroupOrder(params decimal[] quantities)
        => new()
        {
            GroupOrderStableId = "group-order:sim.potato-town-1",
            ProductStableId = "product:potato",
            DeliveryScopeStableId = "delivery-scope:sim.town-1",
            AggregationFacilityStableId = "facility:sim.community-hall-1",
            ActorStableId = "actor:sim.group-leader-1",
            UnitCode = "KGM",
            TargetParticipantCount = 3,
            TargetQuantity = 60m,
            FinalizationDurationTicks = 1,
            Intents = quantities.Select((quantity, index) => new Simulation같이주문의향Request
            {
                IntentStableId = $"group-intent:sim.potato-{index + 1}",
                ParticipantStableId = $"participant:sim.resident-{index + 1}",
                Quantity = quantity,
                ExplicitParticipationConsent = true,
                SourceStableIds = new[]
                {
                    $"source:fixture.group-intent-{index + 1}",
                },
            }).ToArray(),
            SourceStableIds = new[] { "source:fixture.group-order-1" },
        };

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        string commandId)
        => context.Service.Advance(
            context.Session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = commandId,
                ExpectedRevision = current.Revision,
                TickCount = 1,
            });

    private sealed record Context(
        경영SimulationSessionService Service,
        InMemorySimulationSessionSaveStore SaveStore,
        경영SimulationSessionSnapshot Session);
}
