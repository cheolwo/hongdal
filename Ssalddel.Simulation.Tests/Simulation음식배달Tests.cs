using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation음식배달Tests
{
    [Fact]
    public void Preview는_가상주문경계와예상기간을보여주지만_원장을변경하지않는다()
    {
        var context = CreateContext();

        var preview = context.Service.PreviewFoodDelivery(
            context.Session.SessionStableId,
            FoodDelivery());

        Assert.Equal(음식배달상태코드.주문대기, preview.SuggestedStateCode);
        Assert.Equal(6, preview.TotalDurationTicks);
        Assert.Contains("RealDriverDispatch", preview.ExcludedOperationalEffectCodes);
        Assert.Contains("NoPersonalAddress", preview.BoundaryCodes);
        Assert.Contains("ReceiptRequiresSeparateConfirmation", preview.BoundaryCodes);
        Assert.Equal(SimulationDecisionStateCodes.Previewed,
            preview.CommonDecisionPreview.Decision.StateCode);
        Assert.Empty(context.Service.Get(context.Session.SessionStableId).FoodDeliveries);
    }

    [Fact]
    public void Confirm은_운영주문없이_주문대기원장과생애주기Task를생성한다()
    {
        var context = CreateContext();

        var confirmed = Confirm(context);
        var order = Assert.Single(confirmed.FoodDeliveries);

        Assert.Equal(음식배달상태코드.주문대기, order.StateCode);
        Assert.Equal("menu-item:sim.potato-stew-1", order.MenuItemStableId);
        Assert.Equal("facility:sim.restaurant-1", order.RestaurantFacilityStableId);
        Assert.Equal("facility:sim.residence-1", order.DestinationFacilityStableId);
        Assert.Null(order.DeliveredTick);
        Assert.Equal(SimulationTaskStateCodes.Scheduled,
            confirmed.Tasks.Single(value => value.TaskStableId == order.TaskStableId).StateCode);
    }

    [Fact]
    public void WorldTick은_조리부터전달완료까지_허용된상태전이를순서대로남긴다()
    {
        var context = CreateContext();
        var current = Confirm(context);

        current = Advance(context, current, 1);
        Assert.Equal(음식배달상태코드.조리중, Assert.Single(current.FoodDeliveries).StateCode);
        current = Advance(context, current, 2);
        Assert.Equal(음식배달상태코드.픽업대기, Assert.Single(current.FoodDeliveries).StateCode);
        current = Advance(context, current, 3);
        Assert.Equal(음식배달상태코드.기사배정, Assert.Single(current.FoodDeliveries).StateCode);
        current = Advance(context, current, 4);
        Assert.Equal(음식배달상태코드.픽업완료, Assert.Single(current.FoodDeliveries).StateCode);
        current = Advance(context, current, 5);
        current = Advance(context, current, 6);
        var delivered = Assert.Single(current.FoodDeliveries);

        Assert.Equal(음식배달상태코드.전달완료, delivered.StateCode);
        Assert.Equal(current.CurrentTick, delivered.DeliveredTick);
        Assert.Equal(5, delivered.StateHistory.Length);
        Assert.Equal(new[]
        {
            음식배달상태코드.조리중,
            음식배달상태코드.픽업대기,
            음식배달상태코드.기사배정,
            음식배달상태코드.픽업완료,
            음식배달상태코드.전달완료,
        }, delivered.StateHistory.Select(value => value.ToStateCode).ToArray());
    }

    [Fact]
    public void 수령확인은_전달완료전에는차단되고_별도Confirm과Tick이필요하다()
    {
        var context = CreateContext();
        var confirmed = Confirm(context);
        var early = Receipt(Assert.Single(confirmed.FoodDeliveries).Revision);

        var blocked = context.Service.PreviewFoodDeliveryReceipt(
            context.Session.SessionStableId, early);

        Assert.Contains("SimulationFoodDeliveryNotDelivered", blocked.Decision.BlockReasonCodes);

        var delivered = AdvanceToDelivered(context, confirmed);
        var order = Assert.Single(delivered.FoodDeliveries);
        var receipt = Receipt(order.Revision);
        var scheduled = context.Service.ConfirmFoodDeliveryReceipt(
            context.Session.SessionStableId,
            new Simulation음식배달수령ConfirmRequest
            {
                CommandId = "command:food-delivery.receipt-1",
                ExpectedRevision = delivered.Revision,
                Receipt = receipt,
            });

        Assert.Equal(음식배달상태코드.전달완료,
            Assert.Single(scheduled.FoodDeliveries).StateCode);
        var received = Advance(context, scheduled, 7);
        var completed = Assert.Single(received.FoodDeliveries);
        Assert.Equal(음식배달상태코드.수령확인, completed.StateCode);
        Assert.Equal(received.CurrentTick, completed.ReceivedTick);
    }

    [Fact]
    public void 같은Command재시도는_음식배달원장을중복생성하지않는다()
    {
        var context = CreateContext();
        var request = new Simulation음식배달ConfirmRequest
        {
            CommandId = "command:food-delivery.accept-idempotent",
            ExpectedRevision = context.Session.Revision,
            FoodDelivery = FoodDelivery(),
        };

        var first = context.Service.ConfirmFoodDelivery(context.Session.SessionStableId, request);
        var retry = context.Service.ConfirmFoodDelivery(context.Session.SessionStableId, request);

        Assert.Equal(first.Revision, retry.Revision);
        Assert.Single(retry.FoodDeliveries);
        Assert.Single(retry.Tasks);
    }

    [Fact]
    public void SaveReplay는_수령확인과전체음식배달계보를동일하게복원한다()
    {
        var context = CreateContext();
        var delivered = AdvanceToDelivered(context, Confirm(context));
        var order = Assert.Single(delivered.FoodDeliveries);
        var scheduled = context.Service.ConfirmFoodDeliveryReceipt(
            context.Session.SessionStableId,
            new Simulation음식배달수령ConfirmRequest
            {
                CommandId = "command:food-delivery.receipt-save",
                ExpectedRevision = delivered.Revision,
                Receipt = Receipt(order.Revision),
            });
        var completed = Advance(context, scheduled, 7);
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.food-delivery-1",
                ExpectedRevision = completed.Revision,
            });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });
        var restoredOrder = Assert.Single(restored.Session.FoodDeliveries);

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(음식배달상태코드.수령확인, restoredOrder.StateCode);
        Assert.Equal(6, restoredOrder.StateHistory.Length);
        Assert.Equal("participant:sim.orderer-1", restoredOrder.OrdererStableId);
        Assert.Contains("source:fixture.food-delivery-1", restoredOrder.SourceStableIds);
    }

    private static Context CreateContext()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var session = service.Create(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.food-delivery-1",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260811,
            RuleRevision = "rule:r1",
            DurationTicks = 20,
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

    private static 경영SimulationSessionSnapshot Confirm(Context context)
        => context.Service.ConfirmFoodDelivery(
            context.Session.SessionStableId,
            new Simulation음식배달ConfirmRequest
            {
                CommandId = "command:food-delivery.accept-1",
                ExpectedRevision = context.Session.Revision,
                FoodDelivery = FoodDelivery(),
            });

    private static Simulation음식배달PreviewRequest FoodDelivery()
        => new()
        {
            FoodOrderStableId = "food-order:sim.potato-stew-1",
            MenuItemStableId = "menu-item:sim.potato-stew-1",
            RestaurantFacilityStableId = "facility:sim.restaurant-1",
            DestinationFacilityStableId = "facility:sim.residence-1",
            DeliveryScopeStableId = "delivery-scope:sim.town-1",
            OrdererStableId = "participant:sim.orderer-1",
            ActorStableId = "actor:sim.restaurant-1",
            Quantity = 2m,
            UnitCode = "serving",
            PreparationDurationTicks = 2,
            DeliveryDurationTicks = 2,
            SourceStableIds = new[] { "source:fixture.food-delivery-1" },
        };

    private static Simulation음식배달수령PreviewRequest Receipt(long revision)
        => new()
        {
            FoodOrderStableId = "food-order:sim.potato-stew-1",
            FoodOrderRevision = revision,
            ActorStableId = "participant:sim.orderer-1",
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { "source:fixture.food-delivery-receipt-1" },
        };

    private static 경영SimulationSessionSnapshot AdvanceToDelivered(
        Context context,
        경영SimulationSessionSnapshot current)
    {
        for (var tick = 1; tick <= 6; tick++) current = Advance(context, current, tick);
        return current;
    }

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        int suffix)
        => context.Service.Advance(
            context.Session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = $"command:tick.food-delivery-{suffix}",
                ExpectedRevision = current.Revision,
                TickCount = 1,
            });

    private sealed record Context(
        경영SimulationSessionService Service,
        InMemorySimulationSessionSaveStore SaveStore,
        경영SimulationSessionSnapshot Session);
}
