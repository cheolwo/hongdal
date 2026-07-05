namespace Hongdal.Contracts.Common.Payments;

public sealed record PaymentRequestDraft(
    int TargetType,
    string TargetId,
    int ProviderType,
    int Amount,
    string OrderName,
    string PaymentMethod,
    string SettlementMode,
    string Currency = "KRW");

public sealed record PaymentRequestPlan(
    PaymentRequestDraft Draft,
    PaymentFlowPolicy Policy,
    string OrderId,
    string InitialStatus,
    bool ShouldOpenPaymentWindow,
    bool ShouldCreateInvoice,
    bool ShouldScheduleBilling);

public static class PaymentRequestPlanner
{
    public static PaymentRequestPlan Plan(PaymentRequestDraft draft, Func<string>? orderIdFactory = null)
    {
        Validate(draft);

        var policy = PaymentFlowPolicyResolver.Resolve(draft.PaymentMethod, draft.SettlementMode);
        var orderId = orderIdFactory?.Invoke() ?? CreateDefaultOrderId();

        return new PaymentRequestPlan(
            draft,
            policy,
            orderId,
            policy.InitialStatus,
            ShouldOpenPaymentWindow: policy.UsesTossPayments && !policy.UsesBillingKey && !policy.CreatesMonthlyInvoice,
            ShouldCreateInvoice: policy.CreatesMonthlyInvoice,
            ShouldScheduleBilling: policy.UsesBillingKey);
    }

    private static void Validate(PaymentRequestDraft draft)
    {
        if (draft.TargetType <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.TargetType), draft.TargetType, "Target type is required.");
        }

        if (string.IsNullOrWhiteSpace(draft.TargetId))
        {
            throw new ArgumentException("Target id is required.", nameof(draft.TargetId));
        }

        if (draft.ProviderType <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.ProviderType), draft.ProviderType, "Provider type is required.");
        }

        if (draft.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.Amount), draft.Amount, "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(draft.PaymentMethod))
        {
            throw new ArgumentException("Payment method is required.", nameof(draft.PaymentMethod));
        }

        if (string.IsNullOrWhiteSpace(draft.SettlementMode))
        {
            throw new ArgumentException("Settlement mode is required.", nameof(draft.SettlementMode));
        }
    }

    private static string CreateDefaultOrderId()
    {
        return $"hongdal_{Guid.NewGuid():N}";
    }
}
