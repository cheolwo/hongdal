namespace Ssalddel.Contracts.Common.Orderer;

public static class 수입도착원가단계코드
{
    public const string 상품매입 = "ProductPurchase";
    public const string 해외처리 = "OverseasHandling";
    public const string 국제운임보험 = "InternationalFreightInsurance";
    public const string 통관검토 = "CustomsReview";
    public const string 관세 = "CustomsDuty";
    public const string 수입부가세 = "ImportVat";
    public const string 보세창고 = "BondedWarehouse";
    public const string 관세사수수료 = "CustomsBrokerage";
    public const string 국내3PL운송 = "DomesticTransportTo3pl";
    public const string 물류대행입고 = "ThreePlInbound";
}

public static class 수입도착원가단계상태코드
{
    public const string 계획됨 = "Planned";
    public const string 추정 = "Estimated";
    public const string 검토필요 = "NeedsReview";
    public const string 차단 = "Blocked";
}

public static class 수입도착원가단계분류코드
{
    public const string 해외 = "Overseas";
    public const string 국제운송 = "InternationalTransport";
    public const string 통관 = "Customs";
    public const string 국내물류 = "DomesticLogistics";
}

public sealed record 수입도착원가초안(
    decimal 수량Kg,
    decimal 상품매입단가Krw,
    decimal 해외처리단가Krw = 0m,
    decimal 국제운임보험단가Krw = 0m,
    decimal 관세율 = 0m,
    decimal 수입부가세율 = 0.1m,
    decimal 보세창고단가Krw = 0m,
    decimal 관세사수수료단가Krw = 0m,
    decimal 국내3PL운송단가Krw = 0m,
    decimal 물류대행입고단가Krw = 0m,
    bool 통관검토필요 = true,
    bool 통관반려 = false,
    string 통화 = "KRW");

public sealed record 수입도착원가단계(
    string 단계코드,
    string 표시명,
    string 분류코드,
    int 순서,
    decimal 단가Krw,
    decimal 총비용Krw,
    decimal Accumulated단가Krw,
    decimal Accumulated총비용Krw,
    string 상태코드,
    string 설명);

public sealed record 수입도착원가계획(
    수입도착원가초안 초안,
    decimal 예상Cif단가Krw,
    decimal 예상Cif총액Krw,
    decimal 예상세후단가Krw,
    decimal 예상세후총액Krw,
    decimal 예상도착단가Krw,
    decimal 예상도착총액Krw,
    IReadOnlyList<수입도착원가단계> 단계목록,
    IReadOnlyList<string> 경고목록,
    string 요약);

public static class 공동구매수입도착원가계획기
{
    public static 수입도착원가계획 계획(수입도착원가초안 draft)
    {
        Validate(draft);

        var stages = new List<수입도착원가단계>();
        var warnings = new List<string>();
        var accumulatedUnitCost = 0m;

        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.상품매입,
            "Product purchase",
            수입도착원가단계분류코드.해외,
            1,
            draft.상품매입단가Krw,
            수입도착원가단계상태코드.추정,
            "Amount paid or expected to be paid to the overseas seller.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.해외처리,
            "Overseas handling",
            수입도착원가단계분류코드.해외,
            2,
            draft.해외처리단가Krw,
            수입도착원가단계상태코드.추정,
            "Overseas warehouse, inspection, repacking, consolidation, and forwarding costs.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.국제운임보험,
            "International freight and insurance",
            수입도착원가단계분류코드.국제운송,
            3,
            draft.국제운임보험단가Krw,
            수입도착원가단계상태코드.추정,
            "Freight and insurance to the Korean port or airport. This completes the approximate CIF basis.");

        var cifUnit = accumulatedUnitCost;
        var reviewStatus = draft.통관반려
            ? 수입도착원가단계상태코드.차단
            : draft.통관검토필요
                ? 수입도착원가단계상태코드.검토필요
                : 수입도착원가단계상태코드.계획됨;
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.통관검토,
            "Customs and import review",
            수입도착원가단계분류코드.통관,
            4,
            0m,
            reviewStatus,
            "HS code, import requirements, inspection, hold, or rejection risk is reviewed before release.");

        if (draft.통관반려)
        {
            warnings.Add("Customs review is blocked or rejected. Downstream bonded warehouse, domestic transport, and 3PL inbound costs are not reliable yet.");
        }

        var dutyUnit = decimal.Round(cifUnit * ClampRate(draft.관세율), 0, MidpointRounding.AwayFromZero);
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.관세,
            "Customs duty",
            수입도착원가단계분류코드.통관,
            5,
            dutyUnit,
            수입도착원가단계상태코드.추정,
            "Estimated duty calculated from the approximate CIF basis and configured duty rate.");

        var vatBaseUnit = cifUnit + dutyUnit;
        var vatUnit = decimal.Round(vatBaseUnit * ClampRate(draft.수입부가세율), 0, MidpointRounding.AwayFromZero);
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.수입부가세,
            "Import VAT",
            수입도착원가단계분류코드.통관,
            6,
            vatUnit,
            수입도착원가단계상태코드.추정,
            "Estimated import VAT. This is not part of CIF; it is calculated after customs taxable value and duty.");

        var afterTaxUnit = accumulatedUnitCost;

        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.보세창고,
            "Bonded warehouse and terminal",
            수입도착원가단계분류코드.국내물류,
            7,
            draft.보세창고단가Krw,
            수입도착원가단계상태코드.추정,
            "Domestic bonded storage, terminal, or handling cost after arrival.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.관세사수수료,
            "Customs brokerage",
            수입도착원가단계분류코드.국내물류,
            8,
            draft.관세사수수료단가Krw,
            수입도착원가단계상태코드.추정,
            "Customs broker or import agency service fee.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.국내3PL운송,
            "Domestic transport to 3PL",
            수입도착원가단계분류코드.국내물류,
            9,
            draft.국내3PL운송단가Krw,
            수입도착원가단계상태코드.추정,
            "Transport from port, airport, or bonded warehouse to the domestic 3PL warehouse.");
        AddStage(
            stages,
            draft,
            ref accumulatedUnitCost,
            수입도착원가단계코드.물류대행입고,
            "3PL inbound",
            수입도착원가단계분류코드.국내물류,
            10,
            draft.물류대행입고단가Krw,
            수입도착원가단계상태코드.추정,
            "3PL receiving, inspection, labeling, and put-away cost.");

        var landedUnit = accumulatedUnitCost;
        return new 수입도착원가계획(
            draft,
            예상Cif단가Krw: cifUnit,
            예상Cif총액Krw: ToTotal(cifUnit, draft.수량Kg),
            예상세후단가Krw: afterTaxUnit,
            예상세후총액Krw: ToTotal(afterTaxUnit, draft.수량Kg),
            예상도착단가Krw: landedUnit,
            예상도착총액Krw: ToTotal(landedUnit, draft.수량Kg),
            단계목록: stages,
            경고목록: warnings,
            요약: Build요약(cifUnit, afterTaxUnit, landedUnit));
    }

    private static void AddStage(
        List<수입도착원가단계> stages,
        수입도착원가초안 draft,
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
        stages.Add(new 수입도착원가단계(
            stageCode,
            displayName,
            categoryCode,
            sequence,
            normalizedUnitCost,
            ToTotal(normalizedUnitCost, draft.수량Kg),
            accumulatedUnitCost,
            ToTotal(accumulatedUnitCost, draft.수량Kg),
            statusCode,
            description));
    }

    private static void Validate(수입도착원가초안 draft)
    {
        if (draft.수량Kg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.수량Kg), draft.수량Kg, "Quantity must be greater than zero.");
        }

        if (draft.상품매입단가Krw <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(draft.상품매입단가Krw), draft.상품매입단가Krw, "Product purchase unit price must be greater than zero.");
        }
    }

    private static decimal ClampRate(decimal rate)
        => Math.Clamp(rate, 0m, 1m);

    private static decimal ToTotal(decimal unitCost, decimal quantityKg)
        => decimal.Round(unitCost * quantityKg, 0, MidpointRounding.AwayFromZero);

    private static string Build요약(decimal cifUnit, decimal afterTaxUnit, decimal landedUnit)
        => $"Estimated CIF {cifUnit:N0} KRW/kg, after-tax {afterTaxUnit:N0} KRW/kg, landed 3PL inbound {landedUnit:N0} KRW/kg.";
}
