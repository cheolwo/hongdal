namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 구매·계약·정산 원장 사이에서 같은 거래 목적을 이어 주는 공통 키입니다.
/// 계정 유형이 아니라 주문 한 건의 거래 문맥이며, 운송·창고 원장에는 공개 요약 키만 전파합니다.
/// </summary>
public static class 공동구매거래문맥원장키
{
    public const string 거래유형 = "TransactionTypeCode";
    public const string 가격표시기준 = "PriceBasisCode";
    public const string 원천거래문맥원장Id = "SourceTransactionContextLedgerId";
    public const string 구매조직수 = "PurchasingOrganizationCount";
    public const string 세금계산서요청수 = "TaxInvoiceRequestCount";
    public const string 구매조직참조키 = "PurchasingOrganizationReference";
    public const string 구매조직표시명 = "PurchasingOrganizationName";
    public const string 사업자검증상태 = "BusinessVerificationStatusCode";
    public const string 세금계산서필요 = "TaxInvoiceRequired";
}

/// <summary>
/// 다른 원장과 화면에 노출해도 되는 거래 문맥의 최소 요약입니다.
/// 개별 구매조직 식별정보는 포함하지 않습니다.
/// </summary>
public sealed class 공동구매거래문맥응답
{
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 원천거래문맥원장Id { get; set; } = string.Empty;
    public int 구매조직수 { get; set; }
    public int 세금계산서요청수 { get; set; }
}

public static class 공동구매거래문맥정책
{
    public static 공동구매거래문맥응답 생성(
        공동구매자동집단응답 group,
        string? sourceLedgerId = null)
    {
        ArgumentNullException.ThrowIfNull(group);
        var transactionType = 공동구매거래유형코드.정규화(group.거래유형);
        var demands = group.수요목록 ?? [];
        return new 공동구매거래문맥응답
        {
            거래유형 = transactionType,
            가격표시기준 = 공동구매가격표시기준코드.정규화(group.가격표시기준, transactionType),
            원천거래문맥원장Id = sourceLedgerId?.Trim() ?? string.Empty,
            구매조직수 = transactionType == 공동구매거래유형코드.B2B
                ? demands
                    .Where(demand => !string.IsNullOrWhiteSpace(demand.구매조직참조키))
                    .Select(demand => demand.구매조직참조키.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
                : 0,
            세금계산서요청수 = transactionType == 공동구매거래유형코드.B2B
                ? demands.Count(demand => demand.세금계산서필요)
                : 0
        };
    }

    public static bool 호환됨(공동구매자동집단응답 left, 공동구매자동집단응답 right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return 호환됨(left.거래유형, left.가격표시기준, right.거래유형, right.가격표시기준);
    }

    public static bool 호환됨(
        string? leftTransactionType,
        string? leftPriceBasis,
        string? rightTransactionType,
        string? rightPriceBasis)
    {
        var leftType = 공동구매거래유형코드.정규화(leftTransactionType);
        var rightType = 공동구매거래유형코드.정규화(rightTransactionType);
        return string.Equals(leftType, rightType, StringComparison.Ordinal)
               && string.Equals(
                   공동구매가격표시기준코드.정규화(leftPriceBasis, leftType),
                   공동구매가격표시기준코드.정규화(rightPriceBasis, rightType),
                   StringComparison.OrdinalIgnoreCase);
    }
}
