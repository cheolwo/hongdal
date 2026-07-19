using Ssalddel.Contracts.Common.Payments;

namespace Ssalddel.Tests.Contracts.Common.Payments;

public sealed class PaymentFlowPolicyResolverTests
{
    [Fact]
    public void TossCard_Prepaid_IsImmediateTossPayment()
    {
        var policy = PaymentFlowPolicyResolver.Resolve(PaymentMethodCode.TossCard, SettlementModeCode.Prepaid);

        Assert.True(policy.UsesTossPayments);
        Assert.True(policy.RequiresConfirmApi);
        Assert.False(policy.RequiresWebhook);
        Assert.False(policy.CreatesMonthlyInvoice);
        Assert.Equal(PaymentFlowStatusCode.PaymentWindowRequested, policy.InitialStatus);
        Assert.Equal(PaymentFlowStatusCode.Paid, policy.SuccessStatus);
    }

    [Fact]
    public void TossTransfer_Prepaid_IsImmediateTossPayment()
    {
        var policy = PaymentFlowPolicyResolver.Resolve(PaymentMethodCode.TossTransfer, SettlementModeCode.Prepaid);

        Assert.True(policy.UsesTossPayments);
        Assert.True(policy.RequiresConfirmApi);
        Assert.False(policy.RequiresWebhook);
        Assert.Equal(PaymentFlowStatusCode.Paid, policy.SuccessStatus);
    }

    [Fact]
    public void TossVirtualAccount_WaitsForDepositAndRequiresWebhook()
    {
        var policy = PaymentFlowPolicyResolver.Resolve(PaymentMethodCode.TossVirtualAccount, SettlementModeCode.Prepaid);

        Assert.True(policy.UsesTossPayments);
        Assert.True(policy.RequiresConfirmApi);
        Assert.True(policy.RequiresWebhook);
        Assert.Equal(PaymentFlowStatusCode.WaitingForDeposit, policy.InitialStatus);
        Assert.Equal(PaymentFlowStatusCode.Paid, policy.SuccessStatus);
    }

    [Fact]
    public void MonthlyInvoice_CreatesInvoiceBeforePayment()
    {
        var policy = PaymentFlowPolicyResolver.Resolve(PaymentMethodCode.MonthlySettlement, SettlementModeCode.MonthlyInvoice);

        Assert.False(policy.UsesTossPayments);
        Assert.False(policy.RequiresConfirmApi);
        Assert.False(policy.RequiresWebhook);
        Assert.True(policy.CreatesMonthlyInvoice);
        Assert.Equal(PaymentFlowStatusCode.InvoicePending, policy.InitialStatus);
    }

    [Fact]
    public void Subscription_UsesTossBillingKey()
    {
        var policy = PaymentFlowPolicyResolver.Resolve(PaymentMethodCode.TossBilling, SettlementModeCode.Subscription);

        Assert.True(policy.UsesTossPayments);
        Assert.True(policy.UsesBillingKey);
        Assert.True(policy.RequiresWebhook);
        Assert.Equal(PaymentFlowStatusCode.BillingScheduled, policy.InitialStatus);
    }
}
