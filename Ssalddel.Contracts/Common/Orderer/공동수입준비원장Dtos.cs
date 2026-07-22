using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동수입준비국가코드
{
    public const string 대한민국 = "KR";
    public const string 미국 = "US";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        대한민국,
        미국
    };

    public static string 정규화(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}

public static class 공동수입준비비용범주코드
{
    public const string 상품원가 = "Goods";
    public const string 국제운송보험 = "InternationalFreightAndInsurance";
    public const string 관세 = "CustomsDuty";
    public const string 세금 = "ImportTax";
    public const string 국내이행 = "DomesticFulfillment";

    public static IReadOnlyList<string> 필수목록 { get; } =
    [
        상품원가,
        국제운송보험,
        관세,
        세금,
        국내이행
    ];
}

public static class 공동수입준비품목분류체계코드
{
    public const string 한국HsK = "HSK";
    public const string 미국Hts = "HTSUS";

    public static string 대상국가체계(string countryCode)
        => 공동수입준비국가코드.정규화(countryCode) == 공동수입준비국가코드.미국
            ? 미국Hts
            : 한국HsK;
}

public static class 공동수입준비검토상태코드
{
    public const string 미확인 = "Unverified";
    public const string 근거수집 = "EvidenceCollected";
    public const string 전문가검토필요 = "QualifiedReviewRequired";
    public const string 전문가검토완료 = "ReviewedByQualifiedProfessional";
    public const string 해당없음 = "NotApplicable";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        미확인,
        근거수집,
        전문가검토필요,
        전문가검토완료,
        해당없음
    };
}

public static class 공동수입준비책임역할코드
{
    public const string 판매자수출자 = "SellerOrExporter";
    public const string 수입자 = "ImporterOfRecord";
    public const string 관세사 = "CustomsBroker";
    public const string 플랫폼 = "PlatformFacilitator";
    public const string 운송수행자 = "TransportProvider";

    public static IReadOnlyList<string> 필수초안역할목록 { get; } =
    [
        판매자수출자,
        수입자,
        관세사,
        플랫폼
    ];
}

public static class 공동수입준비원장상태코드
{
    public const string 초안 = "Draft";
    public const string 전문검토자료준비 = "ReadyForQualifiedReview";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Contract,
    "승인된 공동구매 수요를 공급자 근거, 견적, 예상 비용, HS·HTS 후보, 국가별 규제 검토와 책임 초안으로 인계하는 1.5 계약입니다.",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "계약 서명, 결제, 수입 신고, 품목분류 확정, 공급자 자동 선정과 운송 지시를 허용하지 않습니다.")]
public sealed class 공동수입준비원장저장요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public long? 기대Revision { get; set; }
    public string 재료키 { get; set; } = string.Empty;
    public string 재료명 { get; set; } = string.Empty;
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 도착국가코드 { get; set; } = 공동수입준비국가코드.대한민국;
    public string 기준통화코드 { get; set; } = "KRW";
    public List<공동수입공급자근거> 공급자근거목록 { get; set; } = [];
    public List<공동수입견적근거> 견적목록 { get; set; } = [];
    public List<공동수입예상비용근거> 예상비용목록 { get; set; } = [];
    public List<공동수입품목분류후보> 품목분류후보목록 { get; set; } = [];
    public List<공동수입국가별검토항목> 국가별검토항목목록 { get; set; } = [];
    public List<공동수입책임초안> 책임초안목록 { get; set; } = [];
    public List<string> 미확인항목목록 { get; set; } = [];
}

public sealed class 공동수입공급자근거
{
    public string 공급자후보키 { get; set; } = string.Empty;
    public string 조직명 { get; set; } = string.Empty;
    public string 국가코드 { get; set; } = string.Empty;
    public string 관계코드 { get; set; } = string.Empty;
    public string 공식식별자 { get; set; } = string.Empty;
    public string 근거요약 { get; set; } = string.Empty;
    public string 원출처명 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public string 검토자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 검토시각Utc { get; set; }
    public bool 최신상태재확인필요 { get; set; } = true;
    public bool 플랫폼자동선정여부 { get; set; }
}

public sealed class 공동수입견적근거
{
    public string 견적키 { get; set; } = string.Empty;
    public string 공급자후보키 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = string.Empty;
    public string 수량단위 { get; set; } = string.Empty;
    public decimal 최소주문수량 { get; set; }
    public decimal 단가 { get; set; }
    public int 납기일수 { get; set; }
    public string 포장조건 { get; set; } = string.Empty;
    public string Incoterms후보 { get; set; } = string.Empty;
    public DateTimeOffset 유효기한Utc { get; set; }
    public string 원출처명 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
}

public sealed class 공동수입예상비용근거
{
    public string 비용키 { get; set; } = string.Empty;
    public string 범주코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = string.Empty;
    public decimal 예상금액 { get; set; }
    public string 계산근거 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public DateTimeOffset? 유효기한Utc { get; set; }
}

public sealed class 공동수입품목분류후보
{
    public string 후보키 { get; set; } = string.Empty;
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 분류체계코드 { get; set; } = string.Empty;
    public string 품목코드 { get; set; } = string.Empty;
    public string 분류근거 { get; set; } = string.Empty;
    public decimal 신뢰도 { get; set; }
    public string 검토상태코드 { get; set; } = 공동수입준비검토상태코드.전문가검토필요;
    public string 검토자표시명 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public bool 전문가검토필요 { get; set; } = true;
}

public sealed class 공동수입국가별검토항목
{
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 항목코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 검토상태코드 { get; set; } = 공동수입준비검토상태코드.미확인;
    public string 책임역할코드 { get; set; } = string.Empty;
    public string 공식원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public string 미확인사유 { get; set; } = string.Empty;
}

public sealed class 공동수입책임초안
{
    public string 역할코드 { get; set; } = string.Empty;
    public string 당사자표시명 { get; set; } = string.Empty;
    public string 책임요약 { get; set; } = string.Empty;
    public bool 당사자확인여부 { get; set; }
}

public sealed class 공동수입준비원장평가응답
{
    public bool 공급자근거구조완료 { get; set; }
    public bool 견적구조완료 { get; set; }
    public bool 예상비용구조완료 { get; set; }
    public bool 품목분류후보구조완료 { get; set; }
    public bool 국가별검토구조완료 { get; set; }
    public bool 책임초안구조완료 { get; set; }
    public bool 전문검토인계가능 { get; set; }
    public bool 계약서명가능 { get; set; }
    public bool 결제가능 { get; set; }
    public bool 신고실행가능 { get; set; }
    public bool 운송지시가능 { get; set; }
    public IReadOnlyList<string> 차단사유목록 { get; set; } = [];
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public IReadOnlyList<string> 명시된미확인항목목록 { get; set; } = [];
}

public sealed class 공동수입준비원장응답
{
    public string 원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public bool 생성됨 { get; set; }
    public bool 이미처리됨 { get; set; }
    public string 상태코드 { get; set; } = 공동수입준비원장상태코드.초안;
    public string 자동집단Id { get; set; } = string.Empty;
    public string 원천수요운영체제Id { get; set; } = string.Empty;
    public string 원천인계요청Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 원천Hs코드 { get; set; } = string.Empty;
    public decimal 모인수요수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public 공동수입준비원장저장요청 준비자료 { get; set; } = new();
    public 공동수입준비원장평가응답 평가 { get; set; } = new();
    public DateTimeOffset 저장시각Utc { get; set; }
}

public static class 공동수입준비원장정책
{
    public static 공동수입준비원장평가응답 평가(
        공동수입준비원장저장요청 request,
        공동구매자동집단응답 group,
        DateTimeOffset evaluatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(group);

        var blockers = new List<string>();
        var warnings = new List<string>();
        var unresolved = request.미확인항목목록
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var destinationCountry = 공동수입준비국가코드.정규화(request.도착국가코드);
        var baseCurrency = NormalizeCurrency(request.기준통화코드);

        if (!공동수입준비국가코드.지원목록.Contains(destinationCountry))
        {
            blockers.Add("도착 국가는 한국(KR) 또는 미국(US)으로 명시해야 합니다.");
        }
        if (NormalizeCountry(request.출발국가코드).Length != 2)
        {
            blockers.Add("선적 출발국가 ISO 2자리 코드가 필요합니다.");
        }
        if (baseCurrency.Length != 3)
        {
            blockers.Add("예상 원가 비교에 사용할 ISO 4217 기준통화가 필요합니다.");
        }

        var supplierReady = EvaluateSuppliers(request, evaluatedAtUtc, blockers, warnings);
        var quoteReady = EvaluateQuotes(request, evaluatedAtUtc, baseCurrency, blockers, warnings);
        var costReady = EvaluateCosts(request, evaluatedAtUtc, baseCurrency, blockers, warnings);
        var classificationReady = EvaluateClassifications(
            request,
            group,
            destinationCountry,
            evaluatedAtUtc,
            blockers,
            unresolved);
        var complianceReady = EvaluateCompliance(
            request,
            destinationCountry,
            evaluatedAtUtc,
            blockers,
            unresolved);
        var responsibilityReady = EvaluateResponsibilities(request, blockers);
        var handoffReady = supplierReady
                           && quoteReady
                           && costReady
                           && classificationReady
                           && complianceReady
                           && responsibilityReady
                           && blockers.Count == 0;

        return new 공동수입준비원장평가응답
        {
            공급자근거구조완료 = supplierReady,
            견적구조완료 = quoteReady,
            예상비용구조완료 = costReady,
            품목분류후보구조완료 = classificationReady,
            국가별검토구조완료 = complianceReady,
            책임초안구조완료 = responsibilityReady,
            전문검토인계가능 = handoffReady,
            계약서명가능 = false,
            결제가능 = false,
            신고실행가능 = false,
            운송지시가능 = false,
            차단사유목록 = blockers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            경고목록 = warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            명시된미확인항목목록 = unresolved.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        };
    }

    private static bool EvaluateSuppliers(
        공동수입준비원장저장요청 request,
        DateTimeOffset now,
        ICollection<string> blockers,
        ICollection<string> warnings)
    {
        if (request.공급자근거목록.Count == 0)
        {
            blockers.Add("원출처가 확인된 국내외 공급자 또는 관련 기업 후보가 하나 이상 필요합니다.");
            return false;
        }

        var ready = true;
        if (HasDuplicate(request.공급자근거목록.Select(item => item.공급자후보키)))
        {
            blockers.Add("공급자 후보 키는 원장 안에서 중복될 수 없습니다.");
            ready = false;
        }

        foreach (var supplier in request.공급자근거목록)
        {
            if (string.IsNullOrWhiteSpace(supplier.공급자후보키)
                || string.IsNullOrWhiteSpace(supplier.조직명)
                || NormalizeCountry(supplier.국가코드).Length != 2
                || string.IsNullOrWhiteSpace(supplier.관계코드)
                || string.IsNullOrWhiteSpace(supplier.공식식별자)
                || string.IsNullOrWhiteSpace(supplier.근거요약)
                || string.IsNullOrWhiteSpace(supplier.원출처명)
                || !IsSourceUrl(supplier.원출처Url)
                || !IsObservedTime(supplier.확인시각Utc, now)
                || string.IsNullOrWhiteSpace(supplier.검토자표시명)
                || !supplier.검토시각Utc.HasValue
                || !IsObservedTime(supplier.검토시각Utc.Value, now))
            {
                blockers.Add($"공급자 후보 '{Label(supplier.조직명, supplier.공급자후보키)}'에 원출처, 식별 근거, 확인 시각과 검토자 기록이 필요합니다.");
                ready = false;
            }
            if (supplier.플랫폼자동선정여부)
            {
                blockers.Add($"공급자 후보 '{Label(supplier.조직명, supplier.공급자후보키)}'는 플랫폼 자동 선정 대상으로 저장할 수 없습니다.");
                ready = false;
            }
            if (supplier.최신상태재확인필요)
            {
                warnings.Add($"공급자 후보 '{Label(supplier.조직명, supplier.공급자후보키)}'는 거래 전에 현재 영업·등록 상태를 다시 확인해야 합니다.");
            }
        }

        return ready;
    }

    private static bool EvaluateQuotes(
        공동수입준비원장저장요청 request,
        DateTimeOffset now,
        string baseCurrency,
        ICollection<string> blockers,
        ICollection<string> warnings)
    {
        if (request.견적목록.Count == 0)
        {
            blockers.Add("공급자 견적이 하나 이상 필요합니다.");
            return false;
        }

        var ready = true;
        var supplierKeys = request.공급자근거목록
            .Select(item => item.공급자후보키.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (HasDuplicate(request.견적목록.Select(item => item.견적키)))
        {
            blockers.Add("견적 키는 원장 안에서 중복될 수 없습니다.");
            ready = false;
        }

        foreach (var quote in request.견적목록)
        {
            var quoteCurrency = NormalizeCurrency(quote.통화코드);
            if (string.IsNullOrWhiteSpace(quote.견적키)
                || !supplierKeys.Contains(quote.공급자후보키.Trim())
                || quoteCurrency.Length != 3
                || string.IsNullOrWhiteSpace(quote.수량단위)
                || quote.최소주문수량 <= 0
                || quote.단가 <= 0
                || quote.납기일수 <= 0
                || string.IsNullOrWhiteSpace(quote.포장조건)
                || string.IsNullOrWhiteSpace(quote.Incoterms후보)
                || string.IsNullOrWhiteSpace(quote.원출처명)
                || !IsSourceUrl(quote.원출처Url)
                || !IsObservedTime(quote.확인시각Utc, now))
            {
                blockers.Add($"견적 '{Label(quote.견적키, "미지정")}'에 공급자, 통화, 단위, MOQ, 단가, 납기, 포장, Incoterms 후보와 원출처가 필요합니다.");
                ready = false;
            }
            if (quote.유효기한Utc <= now)
            {
                blockers.Add($"견적 '{Label(quote.견적키, "미지정")}'의 유효기간이 지났거나 입력되지 않았습니다.");
                ready = false;
            }
            if (baseCurrency.Length == 3
                && quoteCurrency.Length == 3
                && !string.Equals(baseCurrency, quoteCurrency, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"견적 '{quote.견적키}'은 기준통화 {baseCurrency}와 달라 환율 근거를 예상 비용에 별도로 남겨야 합니다.");
            }
        }

        return ready;
    }

    private static bool EvaluateCosts(
        공동수입준비원장저장요청 request,
        DateTimeOffset now,
        string baseCurrency,
        ICollection<string> blockers,
        ICollection<string> warnings)
    {
        var ready = true;
        var categories = request.예상비용목록
            .Where(item => !string.IsNullOrWhiteSpace(item.범주코드))
            .Select(item => item.범주코드.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var requiredCategory in 공동수입준비비용범주코드.필수목록)
        {
            if (!categories.Contains(requiredCategory))
            {
                blockers.Add($"예상 총원가에 '{requiredCategory}' 비용을 분리해 기록해야 합니다.");
                ready = false;
            }
        }

        if (HasDuplicate(request.예상비용목록.Select(item => item.비용키)))
        {
            blockers.Add("예상 비용 키는 원장 안에서 중복될 수 없습니다.");
            ready = false;
        }

        foreach (var cost in request.예상비용목록)
        {
            var currency = NormalizeCurrency(cost.통화코드);
            if (string.IsNullOrWhiteSpace(cost.비용키)
                || string.IsNullOrWhiteSpace(cost.범주코드)
                || string.IsNullOrWhiteSpace(cost.표시명)
                || currency.Length != 3
                || cost.예상금액 < 0
                || string.IsNullOrWhiteSpace(cost.계산근거)
                || !IsSourceUrl(cost.원출처Url)
                || !IsObservedTime(cost.확인시각Utc, now))
            {
                blockers.Add($"예상 비용 '{Label(cost.표시명, cost.비용키)}'에 범주, 통화, 계산 근거, 원출처와 확인 시각이 필요합니다.");
                ready = false;
            }
            if (cost.유효기한Utc.HasValue && cost.유효기한Utc.Value <= now)
            {
                blockers.Add($"예상 비용 '{Label(cost.표시명, cost.비용키)}'의 근거 유효기간이 지났습니다.");
                ready = false;
            }
            if (baseCurrency.Length == 3
                && currency.Length == 3
                && !string.Equals(baseCurrency, currency, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"예상 비용 '{Label(cost.표시명, cost.비용키)}'은 기준통화 {baseCurrency} 환산 근거가 필요합니다.");
            }
        }

        return ready;
    }

    private static bool EvaluateClassifications(
        공동수입준비원장저장요청 request,
        공동구매자동집단응답 group,
        string destinationCountry,
        DateTimeOffset now,
        ICollection<string> blockers,
        ICollection<string> unresolved)
    {
        var destinationCandidates = request.품목분류후보목록
            .Where(item => string.Equals(
                NormalizeCountry(item.관할국가코드),
                destinationCountry,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (destinationCandidates.Length == 0)
        {
            blockers.Add($"도착 국가 {destinationCountry}에 사용할 HS·HTS 품목분류 후보가 필요합니다.");
            return false;
        }

        var ready = true;
        var expectedSystem = 공동수입준비품목분류체계코드.대상국가체계(destinationCountry);
        var sourceHs = Digits(group.HS코드);
        var hasSourceMatch = sourceHs.Length < 6;
        foreach (var candidate in destinationCandidates)
        {
            var candidateCode = Digits(candidate.품목코드);
            if (string.IsNullOrWhiteSpace(candidate.후보키)
                || !string.Equals(candidate.분류체계코드.Trim(), expectedSystem, StringComparison.OrdinalIgnoreCase)
                || candidateCode.Length is < 6 or > 10
                || string.IsNullOrWhiteSpace(candidate.분류근거)
                || candidate.신뢰도 is < 0 or > 1
                || !공동수입준비검토상태코드.지원목록.Contains(candidate.검토상태코드)
                || !IsSourceUrl(candidate.원출처Url)
                || !IsObservedTime(candidate.확인시각Utc, now))
            {
                blockers.Add($"품목분류 후보 '{Label(candidate.품목코드, candidate.후보키)}'에 관할 체계, 코드, 근거, 신뢰도, 검토 상태와 원출처가 필요합니다.");
                ready = false;
            }
            if (sourceHs.Length >= 6
                && candidateCode.Length >= 6
                && sourceHs[..6] == candidateCode[..6])
            {
                hasSourceMatch = true;
            }
            if (candidate.전문가검토필요
                || !string.Equals(
                    candidate.검토상태코드,
                    공동수입준비검토상태코드.전문가검토완료,
                    StringComparison.OrdinalIgnoreCase))
            {
                unresolved.Add($"{destinationCountry} {expectedSystem} 후보 {candidate.품목코드}의 자격 있는 전문가 검토");
            }
        }

        if (!hasSourceMatch)
        {
            blockers.Add("1.0 수요 원장의 HS 코드와 같은 6단위 계열의 품목분류 후보가 필요합니다.");
            ready = false;
        }

        return ready;
    }

    private static bool EvaluateCompliance(
        공동수입준비원장저장요청 request,
        string destinationCountry,
        DateTimeOffset now,
        ICollection<string> blockers,
        ICollection<string> unresolved)
    {
        var destinationItems = request.국가별검토항목목록
            .Where(item => string.Equals(
                NormalizeCountry(item.관할국가코드),
                destinationCountry,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (destinationItems.Length == 0)
        {
            blockers.Add($"도착 국가 {destinationCountry}의 식품·통관 준비 검토 항목을 별도 목록으로 기록해야 합니다.");
            return false;
        }

        var ready = true;
        foreach (var item in request.국가별검토항목목록)
        {
            var country = NormalizeCountry(item.관할국가코드);
            if (!공동수입준비국가코드.지원목록.Contains(country)
                || string.IsNullOrWhiteSpace(item.항목코드)
                || string.IsNullOrWhiteSpace(item.표시명)
                || !공동수입준비검토상태코드.지원목록.Contains(item.검토상태코드)
                || string.IsNullOrWhiteSpace(item.책임역할코드)
                || !IsSourceUrl(item.공식원출처Url)
                || !IsObservedTime(item.확인시각Utc, now))
            {
                blockers.Add($"국가별 검토 항목 '{Label(item.표시명, item.항목코드)}'에 국가, 상태, 책임 역할, 공식 출처와 확인 시각이 필요합니다.");
                ready = false;
            }
            if (string.Equals(item.검토상태코드, 공동수입준비검토상태코드.미확인, StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.검토상태코드, 공동수입준비검토상태코드.전문가검토필요, StringComparison.OrdinalIgnoreCase))
            {
                unresolved.Add(string.IsNullOrWhiteSpace(item.미확인사유)
                    ? $"{country} {item.표시명} 검토"
                    : $"{country} {item.표시명}: {item.미확인사유.Trim()}");
            }
        }

        return ready;
    }

    private static bool EvaluateResponsibilities(
        공동수입준비원장저장요청 request,
        ICollection<string> blockers)
    {
        var ready = true;
        var roles = request.책임초안목록
            .Where(item => !string.IsNullOrWhiteSpace(item.역할코드))
            .Select(item => item.역할코드.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var requiredRole in 공동수입준비책임역할코드.필수초안역할목록)
        {
            if (!roles.Contains(requiredRole))
            {
                blockers.Add($"공동수입 책임 초안에 '{requiredRole}' 역할이 필요합니다.");
                ready = false;
            }
        }

        foreach (var responsibility in request.책임초안목록)
        {
            if (string.IsNullOrWhiteSpace(responsibility.역할코드)
                || string.IsNullOrWhiteSpace(responsibility.당사자표시명)
                || string.IsNullOrWhiteSpace(responsibility.책임요약))
            {
                blockers.Add("책임 초안마다 역할, 당사자 표시명과 책임 범위가 필요합니다.");
                ready = false;
            }
        }

        return ready;
    }

    private static bool HasDuplicate(IEnumerable<string> values)
        => values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

    private static bool IsSourceUrl(string? value)
        => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static bool IsObservedTime(DateTimeOffset value, DateTimeOffset now)
        => value != default && value <= now.AddMinutes(5);

    private static string NormalizeCountry(string? value)
        => 공동수입준비국가코드.정규화(value);

    private static string NormalizeCurrency(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static string Digits(string? value)
        => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string Label(string? preferred, string? fallback)
        => !string.IsNullOrWhiteSpace(preferred)
            ? preferred.Trim()
            : !string.IsNullOrWhiteSpace(fallback)
                ? fallback.Trim()
                : "미지정";
}
