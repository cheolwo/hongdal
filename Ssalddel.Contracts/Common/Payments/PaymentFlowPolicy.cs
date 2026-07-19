namespace Ssalddel.Contracts.Common.Payments;

public static class PaymentMethodCode
{
    public const string TossCard = "toss.card";
    public const string TossTransfer = "toss.transfer";
    public const string TossVirtualAccount = "toss.virtual-account";
    public const string TossBilling = "toss.billing";
    public const string MonthlySettlement = "monthly-settlement";
}

public static class SettlementModeCode
{
    public const string Prepaid = "prepaid";
    public const string PayOnCompletion = "pay-on-completion";
    public const string MonthlyInvoice = "monthly-invoice";
    public const string Subscription = "subscription";
}

public static class PaymentFlowStatusCode
{
    public const string Draft = "draft";
    public const string PaymentWindowRequested = "payment-window-requested";
    public const string WaitingForDeposit = "waiting-for-deposit";
    public const string Paid = "paid";
    public const string InvoicePending = "invoice-pending";
    public const string BillingScheduled = "billing-scheduled";
    public const string Failed = "failed";
    public const string Canceled = "canceled";
}

public sealed record PaymentFlowPolicy(
    string PaymentMethod,
    string SettlementMode,
    bool UsesTossPayments,
    bool RequiresConfirmApi,
    bool RequiresWebhook,
    bool CreatesMonthlyInvoice,
    bool UsesBillingKey,
    string InitialStatus,
    string SuccessStatus);

public static class PaymentFlowPolicyResolver
{
    public static PaymentFlowPolicy Resolve(string paymentMethod, string settlementMode)
    {
        return (paymentMethod, settlementMode) switch
        {
            (PaymentMethodCode.TossCard, SettlementModeCode.Subscription) => TossBillingSubscription(),
            (PaymentMethodCode.TossBilling, _) => TossBillingSubscription(),
            (_, SettlementModeCode.MonthlyInvoice) => MonthlyInvoice(paymentMethod),
            (PaymentMethodCode.TossVirtualAccount, _) => TossVirtualAccount(settlementMode),
            (PaymentMethodCode.TossTransfer, _) => TossImmediate(paymentMethod, settlementMode),
            (PaymentMethodCode.TossCard, _) => TossImmediate(paymentMethod, settlementMode),
            _ => throw new ArgumentOutOfRangeException(nameof(paymentMethod), paymentMethod, "Unsupported payment method.")
        };
    }

    private static PaymentFlowPolicy TossImmediate(string paymentMethod, string settlementMode)
    {
        return new PaymentFlowPolicy(
            paymentMethod,
            settlementMode,
            UsesTossPayments: true,
            RequiresConfirmApi: true,
            RequiresWebhook: false,
            CreatesMonthlyInvoice: false,
            UsesBillingKey: false,
            InitialStatus: PaymentFlowStatusCode.PaymentWindowRequested,
            SuccessStatus: PaymentFlowStatusCode.Paid);
    }

    private static PaymentFlowPolicy TossVirtualAccount(string settlementMode)
    {
        return new PaymentFlowPolicy(
            PaymentMethodCode.TossVirtualAccount,
            settlementMode,
            UsesTossPayments: true,
            RequiresConfirmApi: true,
            RequiresWebhook: true,
            CreatesMonthlyInvoice: false,
            UsesBillingKey: false,
            InitialStatus: PaymentFlowStatusCode.WaitingForDeposit,
            SuccessStatus: PaymentFlowStatusCode.Paid);
    }

    private static PaymentFlowPolicy MonthlyInvoice(string paymentMethod)
    {
        return new PaymentFlowPolicy(
            paymentMethod,
            SettlementModeCode.MonthlyInvoice,
            UsesTossPayments: paymentMethod != PaymentMethodCode.MonthlySettlement,
            RequiresConfirmApi: paymentMethod is PaymentMethodCode.TossCard or PaymentMethodCode.TossTransfer or PaymentMethodCode.TossVirtualAccount,
            RequiresWebhook: paymentMethod == PaymentMethodCode.TossVirtualAccount,
            CreatesMonthlyInvoice: true,
            UsesBillingKey: false,
            InitialStatus: PaymentFlowStatusCode.InvoicePending,
            SuccessStatus: PaymentFlowStatusCode.Paid);
    }

    private static PaymentFlowPolicy TossBillingSubscription()
    {
        return new PaymentFlowPolicy(
            PaymentMethodCode.TossBilling,
            SettlementModeCode.Subscription,
            UsesTossPayments: true,
            RequiresConfirmApi: false,
            RequiresWebhook: true,
            CreatesMonthlyInvoice: false,
            UsesBillingKey: true,
            InitialStatus: PaymentFlowStatusCode.BillingScheduled,
            SuccessStatus: PaymentFlowStatusCode.Paid);
    }
}
