namespace Ssalddel.Contracts.Common.WarehouseBilling;

public static class WarehouseBillingChargeCode
{
    public const string InboundUnloading = "inbound-unloading";
    public const string InboundInspection = "inbound-inspection";
    public const string BarcodeLabeling = "barcode-labeling";
    public const string PutAway = "put-away";
    public const string BundleHandling = "bundle-handling";
    public const string StorageDaily = "storage-daily";
    public const string Picking = "picking";
    public const string PackingLabor = "packing-labor";
    public const string PackingMaterial = "packing-material";
    public const string OutboundHandling = "outbound-handling";
    public const string Loading = "loading";
    public const string ReturnInspection = "return-inspection";
    public const string Disposal = "disposal";
    public const string UrgentHandling = "urgent-handling";
}

public static class WarehouseBillingUnitCode
{
    public const string Each = "each";
    public const string Box = "box";
    public const string Pallet = "pallet";
    public const string Sku = "sku";
    public const string Order = "order";
    public const string OrderLine = "order-line";
    public const string Cbm = "cbm";
    public const string CbmDay = "cbm-day";
    public const string PalletDay = "pallet-day";
}

public static class 물류대행서비스단계코드
{
    public const string 입고 = "Inbound";
    public const string 검수 = "Inspection";
    public const string 적재 = "PutAway";
    public const string 보관 = "Storage";
    public const string 피킹 = "Picking";
    public const string 포장 = "Packing";
    public const string 출고 = "Outbound";
    public const string 예외 = "Exception";
}

public static class 물류대행비용증빙코드
{
    public const string 입고기록 = "InboundRecord";
    public const string 검수기록 = "InspectionRecord";
    public const string 적재기록 = "PutAwayRecord";
    public const string 재고일자기록 = "InventoryDailyRecord";
    public const string 피킹기록 = "PickingRecord";
    public const string 포장기록 = "PackingRecord";
    public const string 출고인계기록 = "OutboundHandoffRecord";
    public const string 예외승인기록 = "ExceptionApprovalRecord";
}

public sealed record WarehouseBillingRate(
    string ChargeCode,
    string DisplayName,
    string UnitCode,
    decimal UnitPrice,
    bool IsEnabled = true,
    string ServiceStageCode = "",
    string CalculationDescription = "",
    string EvidenceTypeCode = "",
    decimal MinimumChargeAmount = 0m,
    bool IsNegotiable = true);

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
    string Memo,
    string ServiceStageCode = "",
    string CalculationDescription = "",
    string EvidenceTypeCode = "",
    decimal MinimumChargeAmount = 0m);

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
    /// <summary>
    /// 계약서 요율 별지와 예상 비용 화면을 검증하기 위한 예시값입니다.
    /// 전국 공통 가격표나 특정 물류대행업체의 확정 견적이 아닙니다.
    /// </summary>
    public static IReadOnlyList<WarehouseBillingRate> CreateDefaultRates()
    {
        return
        [
            new(WarehouseBillingChargeCode.InboundUnloading, "입고·하역", WarehouseBillingUnitCode.Pallet, 1_500m, ServiceStageCode: 물류대행서비스단계코드.입고, CalculationDescription: "실제 하역한 팔레트 수", EvidenceTypeCode: 물류대행비용증빙코드.입고기록),
            new(WarehouseBillingChargeCode.InboundInspection, "입고 검수", WarehouseBillingUnitCode.Each, 300m, ServiceStageCode: 물류대행서비스단계코드.검수, CalculationDescription: "검수 완료 수량", EvidenceTypeCode: 물류대행비용증빙코드.검수기록),
            new(WarehouseBillingChargeCode.BarcodeLabeling, "바코드 라벨 부착", WarehouseBillingUnitCode.Each, 120m, ServiceStageCode: 물류대행서비스단계코드.검수, CalculationDescription: "라벨 부착 수량", EvidenceTypeCode: 물류대행비용증빙코드.검수기록),
            new(WarehouseBillingChargeCode.PutAway, "적재", WarehouseBillingUnitCode.Cbm, 900m, ServiceStageCode: 물류대행서비스단계코드.적재, CalculationDescription: "적재 완료 부피", EvidenceTypeCode: 물류대행비용증빙코드.적재기록),
            new(WarehouseBillingChargeCode.BundleHandling, "묶음 구성", WarehouseBillingUnitCode.Each, 500m, ServiceStageCode: 물류대행서비스단계코드.포장, CalculationDescription: "구성 완료 묶음 수", EvidenceTypeCode: 물류대행비용증빙코드.포장기록),
            new(WarehouseBillingChargeCode.StorageDaily, "보관", WarehouseBillingUnitCode.CbmDay, 80m, ServiceStageCode: 물류대행서비스단계코드.보관, CalculationDescription: "일별 보관 부피 × 겹치는 보관 일수", EvidenceTypeCode: 물류대행비용증빙코드.재고일자기록),
            new(WarehouseBillingChargeCode.Picking, "피킹", WarehouseBillingUnitCode.OrderLine, 350m, ServiceStageCode: 물류대행서비스단계코드.피킹, CalculationDescription: "피킹 완료 주문 라인 수", EvidenceTypeCode: 물류대행비용증빙코드.피킹기록),
            new(WarehouseBillingChargeCode.PackingLabor, "포장 작업", WarehouseBillingUnitCode.Order, 700m, ServiceStageCode: 물류대행서비스단계코드.포장, CalculationDescription: "포장 완료 주문 수", EvidenceTypeCode: 물류대행비용증빙코드.포장기록),
            new(WarehouseBillingChargeCode.PackingMaterial, "포장 자재", WarehouseBillingUnitCode.Box, 500m, ServiceStageCode: 물류대행서비스단계코드.포장, CalculationDescription: "실제 사용한 포장 상자 수", EvidenceTypeCode: 물류대행비용증빙코드.포장기록),
            new(WarehouseBillingChargeCode.OutboundHandling, "출고 처리", WarehouseBillingUnitCode.Order, 800m, ServiceStageCode: 물류대행서비스단계코드.출고, CalculationDescription: "출고 완료 주문 수", EvidenceTypeCode: 물류대행비용증빙코드.출고인계기록),
            new(WarehouseBillingChargeCode.Loading, "상차", WarehouseBillingUnitCode.Pallet, 1_200m, ServiceStageCode: 물류대행서비스단계코드.출고, CalculationDescription: "운송 기사에게 인계한 팔레트 수", EvidenceTypeCode: 물류대행비용증빙코드.출고인계기록),
            new(WarehouseBillingChargeCode.ReturnInspection, "반품 재검수", WarehouseBillingUnitCode.Each, 450m, ServiceStageCode: 물류대행서비스단계코드.예외, CalculationDescription: "반품 후 재검수한 수량", EvidenceTypeCode: 물류대행비용증빙코드.예외승인기록),
            new(WarehouseBillingChargeCode.Disposal, "폐기 처리", WarehouseBillingUnitCode.Each, 300m, ServiceStageCode: 물류대행서비스단계코드.예외, CalculationDescription: "승인 후 폐기한 수량", EvidenceTypeCode: 물류대행비용증빙코드.예외승인기록),
            new(WarehouseBillingChargeCode.UrgentHandling, "긴급·시간외 처리", WarehouseBillingUnitCode.Order, 2_000m, ServiceStageCode: 물류대행서비스단계코드.예외, CalculationDescription: "사전 승인된 긴급 또는 시간외 처리 건수", EvidenceTypeCode: 물류대행비용증빙코드.예외승인기록)
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
        var calculatedAmount = decimal.Round(quantity * Math.Max(0m, rate.UnitPrice), 0, MidpointRounding.AwayFromZero);
        var amount = quantity <= 0
            ? 0m
            : Math.Max(calculatedAmount, Math.Max(0m, rate.MinimumChargeAmount));

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
            usage.Memo,
            rate.ServiceStageCode,
            rate.CalculationDescription,
            rate.EvidenceTypeCode,
            Math.Max(0m, rate.MinimumChargeAmount));
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
