namespace Hongdal.Contracts.Common.WarehouseBilling;

public static class WarehouseBillingChargeCode
{
    public const string InboundInspection = "inbound-inspection";
    public const string BarcodeLabeling = "barcode-labeling";
    public const string PutAway = "put-away";
    public const string BundleHandling = "bundle-handling";
    public const string StorageDaily = "storage-daily";
}

public static class WarehouseBillingUnitCode
{
    public const string Each = "each";
    public const string Cbm = "cbm";
    public const string CbmDay = "cbm-day";
    public const string PalletDay = "pallet-day";
}

public sealed record WarehouseBillingRate(
    string ChargeCode,
    string DisplayName,
    string UnitCode,
    decimal UnitPrice,
    bool IsEnabled = true);

public sealed record WarehouseBillingUsage(
    string ChargeCode,
    decimal Quantity,
    DateOnly? StartedOn = null,
    DateOnly? EndedOn = null,
    string ReferenceId = "",
    string Memo = "");

public sealed record WarehouseBillingLine(
    string ChargeCode,
    string DisplayName,
    string UnitCode,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount,
    DateOnly? StartedOn,
    DateOnly? EndedOn,
    string ReferenceId,
    string Memo);

public sealed record WarehouseBillingDraft(
    string LogisticsAgentId,
    string CustomerId,
    DateOnly BillingPeriodStart,
    DateOnly BillingPeriodEnd,
    string Currency,
    IReadOnlyList<WarehouseBillingLine> Lines,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public static class WarehouseBillingRateCatalog
{
    public static IReadOnlyList<WarehouseBillingRate> CreateDefaultRates()
    {
        return
        [
            new(WarehouseBillingChargeCode.InboundInspection, "Inbound inspection", WarehouseBillingUnitCode.Each, 300m),
            new(WarehouseBillingChargeCode.BarcodeLabeling, "Barcode labeling", WarehouseBillingUnitCode.Each, 120m),
            new(WarehouseBillingChargeCode.PutAway, "Put away", WarehouseBillingUnitCode.Cbm, 900m),
            new(WarehouseBillingChargeCode.BundleHandling, "Bundle handling", WarehouseBillingUnitCode.Each, 500m),
            new(WarehouseBillingChargeCode.StorageDaily, "Storage", WarehouseBillingUnitCode.CbmDay, 80m)
        ];
    }
}

public static class WarehouseBillingPlanner
{
    public static WarehouseBillingDraft Plan(
        string logisticsAgentId,
        string customerId,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd,
        IEnumerable<WarehouseBillingUsage> usages,
        IEnumerable<WarehouseBillingRate> rates,
        decimal taxRate = 0.1m,
        string currency = "KRW")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logisticsAgentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

        if (billingPeriodEnd < billingPeriodStart)
        {
            throw new ArgumentException("Billing period end must be greater than or equal to start.");
        }

        if (taxRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taxRate), taxRate, "Tax rate cannot be negative.");
        }

        var rateMap = rates
            .Where(x => x.IsEnabled)
            .ToDictionary(x => x.ChargeCode, StringComparer.OrdinalIgnoreCase);

        var lines = usages
            .Select(usage => CreateLine(usage, rateMap, billingPeriodStart, billingPeriodEnd))
            .Where(line => line.Quantity > 0)
            .OrderBy(line => line.ChargeCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(line => line.ReferenceId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var subtotal = lines.Sum(x => x.Amount);
        var tax = decimal.Round(subtotal * taxRate, 0, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;

        return new WarehouseBillingDraft(
            logisticsAgentId,
            customerId,
            billingPeriodStart,
            billingPeriodEnd,
            currency,
            lines,
            subtotal,
            tax,
            total);
    }

    private static WarehouseBillingLine CreateLine(
        WarehouseBillingUsage usage,
        IReadOnlyDictionary<string, WarehouseBillingRate> rateMap,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd)
    {
        if (!rateMap.TryGetValue(usage.ChargeCode, out var rate))
        {
            throw new InvalidOperationException($"Warehouse billing rate is missing: {usage.ChargeCode}");
        }

        var quantity = ResolveBillableQuantity(usage, rate, billingPeriodStart, billingPeriodEnd);
        var amount = decimal.Round(quantity * rate.UnitPrice, 0, MidpointRounding.AwayFromZero);

        return new WarehouseBillingLine(
            usage.ChargeCode,
            rate.DisplayName,
            rate.UnitCode,
            quantity,
            rate.UnitPrice,
            amount,
            usage.StartedOn,
            usage.EndedOn,
            usage.ReferenceId,
            usage.Memo);
    }

    private static decimal ResolveBillableQuantity(
        WarehouseBillingUsage usage,
        WarehouseBillingRate rate,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd)
    {
        if (usage.Quantity <= 0)
        {
            return 0m;
        }

        if (rate.UnitCode is not WarehouseBillingUnitCode.CbmDay and not WarehouseBillingUnitCode.PalletDay)
        {
            return usage.Quantity;
        }

        var startedOn = Max(usage.StartedOn ?? billingPeriodStart, billingPeriodStart);
        var endedOn = Min(usage.EndedOn ?? billingPeriodEnd, billingPeriodEnd);
        if (endedOn < startedOn)
        {
            return 0m;
        }

        var days = endedOn.DayNumber - startedOn.DayNumber + 1;
        return usage.Quantity * days;
    }

    private static DateOnly Max(DateOnly left, DateOnly right) => left >= right ? left : right;

    private static DateOnly Min(DateOnly left, DateOnly right) => left <= right ? left : right;
}
