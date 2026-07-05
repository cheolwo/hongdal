using Hongdal.Contracts.Common.Payments;

namespace Hongdal.Tests.Contracts.Common.Payments;

public sealed class PaymentRequestPlannerTests
{
    [Fact]
    public void Plan_CardPrepaid_OpensPaymentWindow()
    {
        var draft = CreateDraft(PaymentMethodCode.TossCard, SettlementModeCode.Prepaid);

        var plan = PaymentRequestPlanner.Plan(draft, () => "order-card");

        Assert.Equal("order-card", plan.OrderId);
        Assert.True(plan.ShouldOpenPaymentWindow);
        Assert.False(plan.ShouldCreateInvoice);
        Assert.False(plan.ShouldScheduleBilling);
        Assert.Equal(PaymentFlowStatusCode.PaymentWindowRequested, plan.InitialStatus);
    }

    [Fact]
    public void Plan_VirtualAccountPrepaid_OpensPaymentWindowAndWaitsForWebhook()
    {
        var draft = CreateDraft(PaymentMethodCode.TossVirtualAccount, SettlementModeCode.Prepaid);

        var plan = PaymentRequestPlanner.Plan(draft, () => "order-va");

        Assert.True(plan.ShouldOpenPaymentWindow);
        Assert.True(plan.Policy.RequiresWebhook);
        Assert.Equal(PaymentFlowStatusCode.WaitingForDeposit, plan.InitialStatus);
    }

    [Fact]
    public void Plan_MonthlyInvoice_CreatesInvoiceInsteadOfOpeningWindow()
    {
        var draft = CreateDraft(PaymentMethodCode.MonthlySettlement, SettlementModeCode.MonthlyInvoice);

        var plan = PaymentRequestPlanner.Plan(draft, () => "order-monthly");

        Assert.False(plan.ShouldOpenPaymentWindow);
        Assert.True(plan.ShouldCreateInvoice);
        Assert.False(plan.ShouldScheduleBilling);
        Assert.Equal(PaymentFlowStatusCode.InvoicePending, plan.InitialStatus);
    }

    [Fact]
    public void Plan_Subscription_SchedulesBilling()
    {
        var draft = CreateDraft(PaymentMethodCode.TossBilling, SettlementModeCode.Subscription);

        var plan = PaymentRequestPlanner.Plan(draft, () => "order-subscription");

        Assert.False(plan.ShouldOpenPaymentWindow);
        Assert.False(plan.ShouldCreateInvoice);
        Assert.True(plan.ShouldScheduleBilling);
        Assert.True(plan.Policy.UsesBillingKey);
    }

    [Fact]
    public void Plan_RejectsInvalidAmount()
    {
        var draft = CreateDraft(PaymentMethodCode.TossCard, SettlementModeCode.Prepaid) with { Amount = 0 };

        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentRequestPlanner.Plan(draft));
    }

    private static PaymentRequestDraft CreateDraft(string paymentMethod, string settlementMode)
    {
        return new PaymentRequestDraft(
            TargetType: 계약결제대상유형.용달운송의뢰,
            TargetId: "REQ-1",
            ProviderType: 계약결제제공자.TossPayments,
            Amount: 10000,
            OrderName: "Hongdal shipment request",
            PaymentMethod: paymentMethod,
            SettlementMode: settlementMode);
    }
}
