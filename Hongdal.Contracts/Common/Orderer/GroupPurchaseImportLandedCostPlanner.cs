namespace Hongdal.Contracts.Common.Orderer;

public static class ImportLandedCostStageCode
{
    public const string ProductPurchase = "ProductPurchase";
    public const string OverseasHandling = "OverseasHandling";
    public const string InternationalFreightInsurance = "InternationalFreightInsurance";
    public const string CustomsReview = "CustomsReview";
    public const string CustomsDuty = "CustomsDuty";
    public const string ImportVat = "ImportVat";
    public const string BondedWarehouse = "BondedWarehouse";
    public const string CustomsBrokerage = "CustomsBrokerage";
    public const string DomesticTransportTo3pl = "DomesticTransportTo3pl";
    public const string ThreePlInbound = "ThreePlInbound";
}

public static class ImportLandedCostStageStatusCode
{
    public const string Planned = "Planned";
    public const string Estimated = "Estimated";
    public const string NeedsReview = "NeedsReview";
    public const string Blocked = "Blocked";
}

public static class ImportLandedCostStageCategoryCode
{
    public const string Overseas = "Overseas";
    public const string InternationalTransport = "InternationalTransport";
    public const string Customs = "Customs";
    public const string DomesticLogistics = "DomesticLogistics";
}

public sealed record ImportLandedCostDraft(
    decimal QuantityKg,
    decimal ProductPurchaseUnitPriceKrw,
    decimal OverseasHandlingUnitCostKrw = 0m,
    decimal InternationalFreightInsuranceUnitCostKrw = 0m,
    decimal CustomsDutyRate = 0m,
    decimal ImportVatRate = 0.1m,
    decimal BondedWarehouseUnitCostKrw = 0m,
    decimal CustomsBrokerageUnitCostKrw = 0m,
    decimal DomesticTransportTo3plUnitCostKrw = 0m,
    decimal ThreePlInboundUnitCostKrw = 0m,
    bool CustomsReviewRequired = true,
    bool CustomsRejected = false,
    string Currency = "KRW");

public sealed record ImportLandedCostStage(
    string StageCode,
    string DisplayName,
    string CategoryCode,
    int Sequence,
    decimal UnitCostKrw,
    decimal TotalCostKrw,
    decimal AccumulatedUnitCostKrw,
    decimal AccumulatedTotalCostKrw,
    string StatusCode,
    string Description);

public sealed record ImportLandedCostPlan(
    ImportLandedCostDraft Draft,
    decimal EstimatedCifUnitPriceKrw,
    decimal EstimatedCifTotalKrw,
    decimal EstimatedAfterTaxUnitCostKrw,
    decimal EstimatedAfterTaxTotalKrw,
    decimal EstimatedLandedUnitCostKrw,
    decimal EstimatedLandedTotalKrw,
    IReadOnlyList<ImportLandedCostStage> Stages,
    IReadOnlyList<string> Warnings,
    string Summary);

public static class GroupPurchaseImportLandedCostPlanner
{
    public static ImportLandedCostPlan Plan(ImportLandedCostDraft draft)
    {
        Validate(draft);

        var stages = new List<ImportLandedCostStage>();
        var warnings = new List<string>();
        var accumulatedUnitCost = 0m;

        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.ProductPurchase,
            "Product purchase",
            ImportLandedCostStageCategoryCode.Overseas,
            1,
            draft.ProductPurchaseUnitPriceKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Amount paid or expected to be paid to the overseas seller.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.OverseasHandling,
            "Overseas handling",
            ImportLandedCostStageCategoryCode.Overseas,
            2,
            draft.OverseasHandlingUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Overseas warehouse, inspection, repacking, consolidation, and forwarding costs.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.InternationalFreightInsurance,
            "International freight and insurance",
            ImportLandedCostStageCategoryCode.InternationalTransport,
            3,
            draft.InternationalFreightInsuranceUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Freight and insurance to the Korean port or airport. This completes the approximate CIF basis.");

        var cifUnit = accumulatedUnitCost;
        var reviewStatus = draft.CustomsRejected
            ? ImportLandedCostStageStatusCode.Blocked
            : draft.CustomsReviewRequired
                ? ImportLandedCostStageStatusCode.NeedsReview
                : ImportLandedCostStageStatusCode.Planned;
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.CustomsReview,
            "Customs and import review",
            ImportLandedCostStageCategoryCode.Customs,
            4,
            0m,
            reviewStatus,
            "HS code, import requirements, inspection, hold, or rejection risk is reviewed before release.");

        if (draft.CustomsRejected)
        {
            warnings.Add("Customs review is blocked or rejected. Downstream bonded warehouse, domestic transport, and 3PL inbound costs are not reliable yet.");
        }

        var dutyUnit = decimal.Round(cifUnit * ClampRate(draft.CustomsDutyRate), 0, MidpointRounding.AwayFromZero);
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.CustomsDuty,
            "Customs duty",
            ImportLandedCostStageCategoryCode.Customs,
            5,
            dutyUnit,
            ImportLandedCostStageStatusCode.Estimated,
            "Estimated duty calculated from the approximate CIF basis and configured duty rate.");

        var vatBaseUnit = cifUnit + dutyUnit;
        var vatUnit = decimal.Round(vatBaseUnit * ClampRate(draft.ImportVatRate), 0, MidpointRounding.AwayFromZero);
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.ImportVat,
            "Import VAT",
            ImportLandedCostStageCategoryCode.Customs,
            6,
            vatUnit,
            ImportLandedCostStageStatusCode.Estimated,
            "Estimated import VAT. This is not part of CIF; it is calculated after customs taxable value and duty.");

        var afterTaxUnit = accumulatedUnitCost;

        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.BondedWarehouse,
            "Bonded warehouse and terminal",
            ImportLandedCostStageCategoryCode.DomesticLogistics,
            7,
            draft.BondedWarehouseUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Domestic bonded storage, terminal, or handling cost after arrival.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.CustomsBrokerage,
            "Customs brokerage",
            ImportLandedCostStageCategoryCode.DomesticLogistics,
            8,
            draft.CustomsBrokerageUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Customs broker or import agency service fee.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.DomesticTransportTo3pl,
            "Domestic transport to 3PL",
            ImportLandedCostStageCategoryCode.DomesticLogistics,
            9,
            draft.DomesticTransportTo3plUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "Transport from port, airport, or bonded warehouse to the domestic 3PL warehouse.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            ImportLandedCostStageCode.ThreePlInbound,
            "3PL inbound",
            ImportLandedCostStageCategoryCode.DomesticLogistics,
            10,
            draft.ThreePlInboundUnitCostKrw,
            ImportLandedCostStageStatusCode.Estimated,
            "3PL receiving, inspection, labeling, and put-away cost.");

        var landedUnit = accumulatedUnitCost;
        return new ImportLandedCostPlan(
            draft,
            EstimatedCifUnitPriceKrw: cifUnit,
            EstimatedCifTotalKrw: ToTotal(cifUnit, draft.QuantityKg),
            EstimatedAfterTaxUnitCostKrw: afterTaxUnit,
            EstimatedAfterTaxTotalKrw: ToTotal(afterTaxUnit, draft.QuantityKg),
            EstimatedLandedUnitCostKrw: landedUnit,
            EstimatedLandedTotalKrw: ToTotal(landedUnit, draft.QuantityKg),
            Stages: stages,
            Warnings: warnings,
            Summary: BuildSummary(cifUnit, afterTaxUnit, landedUnit));
    }

    private static void AddStage(
        List<ImportLandedCostStage> stages,
        ImportLandedCostDraft draft,
        ref decimal accumulatedUnitCost,
        string stageCode,
        string displayName,
        string categoryCode,
        int sequence,
        decimal unitCost,
        string statusCode,
        string description)
    {
        var normalizedUnitCost = Math.Max(0m, unitCost);
        accumulatedUnitCost += normalizedUnitCost;
        stages.Add(new ImportLandedCostStage(
            stageCode,
            displayName,
            categoryCode,
            sequence,
            normalizedUnitCost,
            ToTotal(normalizedUnitCost, draft.QuantityKg),
            accumulatedUnitCost,
            ToTotal(accumulatedUnitCost, draft.QuantityKg),
            statusCode,
            description));
    }

    private static void Validate(ImportLandedCostDraft draft)
    {
        if (draft.QuantityKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.QuantityKg), draft.QuantityKg, "Quantity must be greater than zero.");
        }

        if (draft.ProductPurchaseUnitPriceKrw <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.ProductPurchaseUnitPriceKrw), draft.ProductPurchaseUnitPriceKrw, "Product purchase unit price must be greater than zero.");
        }
    }

    private static decimal ClampRate(decimal rate)
        => Math.Clamp(rate, 0m, 1m);

    private static decimal ToTotal(decimal unitCost, decimal quantityKg)
        => decimal.Round(unitCost * quantityKg, 0, MidpointRounding.AwayFromZero);

    private static string BuildSummary(decimal cifUnit, decimal afterTaxUnit, decimal landedUnit)
        => $"Estimated CIF {cifUnit:N0} KRW/kg, after-tax {afterTaxUnit:N0} KRW/kg, landed 3PL inbound {landedUnit:N0} KRW/kg.";
}
