using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Contracts.Common.WarehouseBilling;

public static class 물류대행계약당사자역할코드
{
    public const string 화주 = "Shipper";
    public const string 물류대행업체 = "LogisticsProvider";
}

public static class 물류대행계약당사자유형코드
{
    public const string 개인 = "Individual";
    public const string 사업자 = "Business";
    public const string 공동행동집단 = "CollectiveActionGroup";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            사업자 => 사업자,
            공동행동집단 => 공동행동집단,
            _ => 개인
        };
}

public static class 물류대행계약당사자자격출처코드
{
    /// <summary>
    /// 플랫폼의 고정 직업 역할이 아니라 이 계약에서 물건을 맡기는 당사자로 지정되었음을 뜻합니다.
    /// </summary>
    public const string 계약별지정 = "ContractSpecificAssignment";
}

public static class 물류대행계약상태코드
{
    public const string 비용검토초안 = "CostReviewDraft";
    public const string 서명대기 = "WaitingForSignature";
    public const string 활성 = "Active";
}

public static class 물류대행요율출처코드
{
    public const string 사용자입력검토안 = "UserEnteredReviewDraft";
    public const string 대행업체제안 = "ProviderProposal";
    public const string 양측합의 = "MutuallyAgreed";
}

public sealed record 물류대행계약당사자(
    string PartyId,
    string DisplayName,
    string RoleCode,
    string PartyTypeCode,
    string QualificationSourceCode,
    bool IsRequiredSigner);

public sealed record 물류대행서비스범위(
    string ServiceStageCode,
    string DisplayName,
    IReadOnlyList<string> ChargeCodes);

public sealed record 물류대행요율표스냅샷(
    string RateVersion,
    string RateSourceCode,
    string Currency,
    DateTimeOffset CapturedAtUtc,
    IReadOnlyList<WarehouseBillingRate> Rates);

public sealed record 물류대행계약초안(
    string DraftId,
    string ContractNumber,
    string StatusCode,
    IReadOnlyList<물류대행계약당사자> Parties,
    IReadOnlyList<물류대행서비스범위> ServiceScopes,
    물류대행요율표스냅샷 RateSnapshot,
    DateOnly ServicePeriodStart,
    DateOnly ServicePeriodEnd,
    DateTimeOffset CreatedAtUtc,
    bool IsBinding,
    bool CanActivate,
    string ActivationRequirement);

public sealed class 물류대행비용미리보기요청
{
    public string LogisticsProviderId { get; set; } = string.Empty;

    public string LogisticsProviderDisplayName { get; set; } = string.Empty;

    public string RequesterPartyTypeCode { get; set; } = 물류대행계약당사자유형코드.개인;

    public DateOnly ServicePeriodStart { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly ServicePeriodEnd { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string Currency { get; set; } = "KRW";

    public decimal TaxRate { get; set; } = 0.1m;

    public string RateSourceCode { get; set; } = 물류대행요율출처코드.사용자입력검토안;

    public IReadOnlyList<WarehouseBillingRate> Rates { get; set; } = [];

    public IReadOnlyList<WarehouseBillingUsage> Usages { get; set; } = [];
}

public sealed record 물류대행비용미리보기응답(
    물류대행계약초안 ContractDraft,
    WarehouseBillingDraft EstimatedBilling,
    IReadOnlyList<string> Warnings);

public static class 물류대행계약계획기
{
    private static readonly IReadOnlyDictionary<string, string> StageNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [물류대행서비스단계코드.입고] = "입고·하역",
            [물류대행서비스단계코드.검수] = "검수",
            [물류대행서비스단계코드.적재] = "적재",
            [물류대행서비스단계코드.보관] = "보관",
            [물류대행서비스단계코드.피킹] = "피킹",
            [물류대행서비스단계코드.포장] = "포장",
            [물류대행서비스단계코드.출고] = "출고·인계",
            [물류대행서비스단계코드.예외] = "반품·폐기·긴급 처리"
        };

    public static 물류대행비용미리보기응답 Plan(
        string requesterUserId,
        string requesterDisplayName,
        물류대행비용미리보기요청 request,
        DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterUserId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LogisticsProviderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LogisticsProviderDisplayName);

        var rates = (request.Rates.Count == 0
                ? WarehouseBillingRateCatalog.CreateDefaultRates()
                : request.Rates)
            .Where(rate => rate.IsEnabled)
            .Select(NormalizeRate)
            .ToArray();
        if (rates.Length == 0)
        {
            throw new ArgumentException("검토할 물류대행 요율이 한 개 이상 필요합니다.", nameof(request));
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "KRW"
            : request.Currency.Trim().ToUpperInvariant();
        var providerId = request.LogisticsProviderId.Trim();
        var providerName = request.LogisticsProviderDisplayName.Trim();
        var requesterId = requesterUserId.Trim();
        var requesterName = string.IsNullOrWhiteSpace(requesterDisplayName)
            ? "로그인 이용자"
            : requesterDisplayName.Trim();
        var serviceEnd = request.ServicePeriodEnd < request.ServicePeriodStart
            ? request.ServicePeriodStart
            : request.ServicePeriodEnd;
        var rateVersion = BuildRateVersion(rates, currency);
        var draftId = $"WSC-DRAFT-{Guid.NewGuid():N}";

        var billing = WarehouseBillingPlanner.Plan(
            providerId,
            requesterId,
            request.ServicePeriodStart,
            serviceEnd,
            request.Usages,
            rates,
            request.TaxRate,
            currency);

        var contract = new 물류대행계약초안(
            draftId,
            $"WSC-PREVIEW-{nowUtc:yyyyMMdd}-{draftId[^8..].ToUpperInvariant()}",
            물류대행계약상태코드.비용검토초안,
            [
                new(
                    requesterId,
                    requesterName,
                    물류대행계약당사자역할코드.화주,
                    물류대행계약당사자유형코드.Normalize(request.RequesterPartyTypeCode),
                    물류대행계약당사자자격출처코드.계약별지정,
                    true),
                new(
                    providerId,
                    providerName,
                    물류대행계약당사자역할코드.물류대행업체,
                    물류대행계약당사자유형코드.사업자,
                    물류대행계약당사자자격출처코드.계약별지정,
                    true)
            ],
            BuildServiceScopes(rates),
            new(
                rateVersion,
                NormalizeRateSource(request.RateSourceCode),
                currency,
                nowUtc,
                rates),
            request.ServicePeriodStart,
            serviceEnd,
            nowUtc,
            IsBinding: false,
            CanActivate: false,
            ActivationRequirement: "서비스 범위·SLA·손망실 책임·정산 주기·요율표를 양측이 확인하고 같은 문서 버전에 전자서명해야 활성화할 수 있습니다.");

        return new(
            contract,
            billing,
            [
                "이 금액은 입력한 작업량과 요율로 계산한 검토용 예상 비용이며 물류대행업체의 확정 견적이 아닙니다.",
                "화주는 고정 직업 역할이 아니라 이 계약에서 물건을 맡기는 당사자 역할입니다. 로그인한 개인·판매자·공동행동 운영자도 계약별로 화주가 될 수 있습니다.",
                "실제 비용은 입고·검수·적재·보관·피킹·포장·출고 작업 기록과 예외 승인 증빙을 기준으로 다시 정산해야 합니다.",
                "세금 적용 여부와 책임·보험·손망실·재위탁 조건은 실제 계약 체결 전에 전문가와 확인해야 합니다."
            ]);
    }

    private static WarehouseBillingRate NormalizeRate(WarehouseBillingRate rate)
        => rate with
        {
            ChargeCode = rate.ChargeCode.Trim(),
            DisplayName = rate.DisplayName.Trim(),
            UnitCode = rate.UnitCode.Trim(),
            UnitPrice = Math.Max(0m, rate.UnitPrice),
            ServiceStageCode = rate.ServiceStageCode.Trim(),
            CalculationDescription = rate.CalculationDescription.Trim(),
            EvidenceTypeCode = rate.EvidenceTypeCode.Trim(),
            MinimumChargeAmount = Math.Max(0m, rate.MinimumChargeAmount)
        };

    private static string NormalizeRateSource(string? value)
        => value?.Trim() switch
        {
            물류대행요율출처코드.대행업체제안 => 물류대행요율출처코드.대행업체제안,
            물류대행요율출처코드.양측합의 => 물류대행요율출처코드.양측합의,
            _ => 물류대행요율출처코드.사용자입력검토안
        };

    private static IReadOnlyList<물류대행서비스범위> BuildServiceScopes(
        IReadOnlyList<WarehouseBillingRate> rates)
        => rates
            .Where(rate => !string.IsNullOrWhiteSpace(rate.ServiceStageCode))
            .GroupBy(rate => rate.ServiceStageCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => new 물류대행서비스범위(
                group.Key,
                StageNames.TryGetValue(group.Key, out var displayName) ? displayName : group.Key,
                group.Select(rate => rate.ChargeCode)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .OrderBy(scope => StageOrder(scope.ServiceStageCode))
            .ToArray();

    private static int StageOrder(string stageCode)
        => stageCode switch
        {
            물류대행서비스단계코드.입고 => 10,
            물류대행서비스단계코드.검수 => 20,
            물류대행서비스단계코드.적재 => 30,
            물류대행서비스단계코드.보관 => 40,
            물류대행서비스단계코드.피킹 => 50,
            물류대행서비스단계코드.포장 => 60,
            물류대행서비스단계코드.출고 => 70,
            물류대행서비스단계코드.예외 => 80,
            _ => 999
        };

    private static string BuildRateVersion(
        IReadOnlyList<WarehouseBillingRate> rates,
        string currency)
    {
        var canonical = string.Join(
            "\n",
            rates
                .OrderBy(rate => rate.ChargeCode, StringComparer.OrdinalIgnoreCase)
                .Select(rate => string.Join(
                    "|",
                    rate.ChargeCode,
                    rate.UnitCode,
                    rate.UnitPrice.ToString(CultureInfo.InvariantCulture),
                    rate.MinimumChargeAmount.ToString(CultureInfo.InvariantCulture),
                    rate.ServiceStageCode,
                    rate.EvidenceTypeCode,
                    currency)));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return $"DRAFT-{Convert.ToHexString(hash)[..12]}";
    }
}
