using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 같이수입준비국가코드
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

public static class 같이수입준비비용범주코드
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

public static class 같이수입준비품목분류체계코드
{
    public const string 한국HsK = "HSK";
    public const string 미국Hts = "HTSUS";

    public static string 대상국가체계(string countryCode)
        => 같이수입준비국가코드.정규화(countryCode) == 같이수입준비국가코드.미국
            ? 미국Hts
            : 한국HsK;
}

public static class 같이수입준비검토상태코드
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

public static class 같이수입준비책임역할코드
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

public static class 같이수입준비원장상태코드
{
    public const string 초안 = "Draft";
    public const string 전문검토자료준비 = "ReadyForQualifiedReview";
}

public static class 같이수입준비국제운송방식코드
{
    public const string Lcl = "LCL";
    public const string Fcl = "FCL";

    public static IReadOnlyList<string> 비교후보목록 { get; } = [Lcl, Fcl];

    public static bool 지원여부(string? value)
        => 비교후보목록.Contains(value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase);
}

public static class 같이수입준비국제운송검토상태코드
{
    public const string 검토필요 = "ReviewRequired";
    public const string 비교중 = "Comparing";
    public const string 포워더회신완료 = "ForwarderResponseReceived";
    // 기존 저장 자료와 호출부의 소스 호환을 위한 별칭입니다.
    public const string 사람선택완료 = 포워더회신완료;

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        검토필요,
        비교중,
        포워더회신완료
    };
}

public static class 같이수입준비포워더인계상태코드
{
    public const string 초안 = "Draft";
    public const string 인계준비 = "ReadyForHandoff";
    public const string 인계기록됨 = "HandoffRecorded";
    public const string 회신기록됨 = "ResponseRecorded";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        초안,
        인계준비,
        인계기록됨,
        회신기록됨
    };
}

public static class 같이수입준비포워더전달정보범위코드
{
    public const string 집계수요전용 = "AggregatedDemandOnly";
    public const string 동의된사용자별최소정보 = "ConsentedUserMinimum";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        집계수요전용,
        동의된사용자별최소정보
    };
}

public static class 같이수입준비포워더전달항목코드
{
    public const string 재료별집계수량 = "MaterialAggregateQuantity";
    public const string 온도조건 = "TemperatureRequirement";
    public const string 출발도착국가 = "OriginDestinationCountry";
    public const string 수령권역요약 = "DeliveryAreaSummary";
    public const string 희망일정 = "RequestedSchedule";
    public const string 견적요청조건 = "QuoteRequestConditions";

    public static IReadOnlyList<string> 기본집계목록 { get; } =
    [
        재료별집계수량,
        온도조건,
        출발도착국가,
        수령권역요약,
        희망일정,
        견적요청조건
    ];
}

public static class 같이수입준비Incoterms코드
{
    public const string Fca = "FCA";
    public const string Fob = "FOB";
    public const string Cfr = "CFR";
    public const string Cif = "CIF";
    public const string Dap = "DAP";
    public const string Ddp = "DDP";

    public static IReadOnlyList<string> 후보목록 { get; } = [Fca, Fob, Cfr, Cif, Dap, Ddp];
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Contract,
    "승인된 공동구매 수요를 기존 같이 수입 원장의 공급·가격·무역 준비 블록과 포워더 인계 자료로 연결하는 1.5 계약입니다.",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "플랫폼은 수요 집계·정보 최소화·동의 확인·인계 기록만 조율하며 포워더 자동 선정, 외부 자동 전송, 계약, 결제, 신고와 운송 지시를 허용하지 않습니다.")]
public sealed class 같이수입준비원장저장요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public long? 기대Revision { get; set; }
    // 기존 단일 재료 API/저장 자료의 역직렬화 호환을 위해 유지합니다. 새 흐름은 재료품목목록을 사용합니다.
    public string 재료키 { get; set; } = string.Empty;
    public string 재료명 { get; set; } = string.Empty;
    public List<같이수입준비재료품목> 재료품목목록 { get; set; } = [];
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 도착국가코드 { get; set; } = 같이수입준비국가코드.대한민국;
    public string 기준통화코드 { get; set; } = "KRW";
    public 같이수입준비포워더인계 포워더인계 { get; set; } = new();
    public 같이수입준비국제운송검토 국제운송검토 { get; set; } = new();
    public List<같이수입공급자근거> 공급자근거목록 { get; set; } = [];
    public List<같이수입견적근거> 견적목록 { get; set; } = [];
    public List<같이수입예상비용근거> 예상비용목록 { get; set; } = [];
    public List<같이수입품목분류후보> 품목분류후보목록 { get; set; } = [];
    public List<같이수입국가별검토항목> 국가별검토항목목록 { get; set; } = [];
    public List<같이수입책임초안> 책임초안목록 { get; set; } = [];
    public List<string> 미확인항목목록 { get; set; } = [];
}

public sealed class 같이수입준비재료품목
{
    public string 재료키 { get; set; } = string.Empty;
    public string 재료명 { get; set; } = string.Empty;
    public string 원천자동집단Id { get; set; } = string.Empty;
    public string 원천Hs코드 { get; set; } = string.Empty;
    public decimal 모인수요수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
}

public sealed class 같이수입준비국제운송검토
{
    public string 검토상태코드 { get; set; } = 같이수입준비국제운송검토상태코드.검토필요;
    public List<string> 방식후보목록 { get; set; } = [.. 같이수입준비국제운송방식코드.비교후보목록];
    public string 포워더제안방식코드 { get; set; } = string.Empty;
    public string 포워더회신요약 { get; set; } = string.Empty;
    public string 회신업체표시명 { get; set; } = string.Empty;
    public string 회신기록자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 회신시각Utc { get; set; }
    // 아래 네 필드는 기존 저장 자료 역직렬화 호환용입니다. 새 입력은 위의 포워더 회신 필드를 사용합니다.
    public string 선택방식코드 { get; set; } = string.Empty;
    public string 판단근거 { get; set; } = string.Empty;
    public string 검토자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 검토시각Utc { get; set; }
}

public sealed class 같이수입준비포워더인계
{
    public string 인계상태코드 { get; set; } = 같이수입준비포워더인계상태코드.초안;
    public string 전달대상업체키 { get; set; } = string.Empty;
    public string 전달대상업체명 { get; set; } = string.Empty;
    public string 전달정보범위코드 { get; set; } = 같이수입준비포워더전달정보범위코드.집계수요전용;
    public List<string> 전달항목코드목록 { get; set; } = [.. 같이수입준비포워더전달항목코드.기본집계목록];
    public string 전달범위요약 { get; set; } = "개인 식별정보를 제외한 재료별 합산 수요와 물류 조건";
    public bool 개인정보포함여부 { get; set; }
    public bool 정보제공동의확인여부 { get; set; }
    public string 정보제공동의근거참조 { get; set; } = string.Empty;
    public string 전달패키지버전 { get; set; } = "1.0";
    public string 인계기록자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 인계시각Utc { get; set; }
}

public sealed class 같이수입공급자근거
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

public sealed class 같이수입견적근거
{
    public string 견적키 { get; set; } = string.Empty;
    public string 재료키 { get; set; } = string.Empty;
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

public sealed class 같이수입예상비용근거
{
    public string 비용키 { get; set; } = string.Empty;
    public string 재료키 { get; set; } = string.Empty;
    public string 범주코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = string.Empty;
    public decimal 예상금액 { get; set; }
    public string 계산근거 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public DateTimeOffset? 유효기한Utc { get; set; }
}

public sealed class 같이수입품목분류후보
{
    public string 후보키 { get; set; } = string.Empty;
    public string 재료키 { get; set; } = string.Empty;
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 분류체계코드 { get; set; } = string.Empty;
    public string 품목코드 { get; set; } = string.Empty;
    public string 분류근거 { get; set; } = string.Empty;
    public decimal 신뢰도 { get; set; }
    public string 검토상태코드 { get; set; } = 같이수입준비검토상태코드.전문가검토필요;
    public string 검토자표시명 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public bool 전문가검토필요 { get; set; } = true;
}

public sealed class 같이수입국가별검토항목
{
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 항목코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 검토상태코드 { get; set; } = 같이수입준비검토상태코드.미확인;
    public string 책임역할코드 { get; set; } = string.Empty;
    public string 공식원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public string 미확인사유 { get; set; } = string.Empty;
}

public sealed class 같이수입책임초안
{
    public string 역할코드 { get; set; } = string.Empty;
    public string 당사자표시명 { get; set; } = string.Empty;
    public string 책임요약 { get; set; } = string.Empty;
    public bool 당사자확인여부 { get; set; }
}

public sealed class 같이수입준비원장평가응답
{
    public bool 재료품목구조완료 { get; set; }
    public bool 공급자근거구조완료 { get; set; }
    public bool 견적구조완료 { get; set; }
    public bool 예상비용구조완료 { get; set; }
    public bool 품목분류후보구조완료 { get; set; }
    public bool 국가별검토구조완료 { get; set; }
    public bool 포워더인계구조완료 { get; set; }
    public bool 포워더인계준비가능 { get; set; }
    public bool 포워더인계기록완료 { get; set; }
    public bool 포워더회신기록완료 { get; set; }
    public bool 국제운송검토구조완료 { get; set; }
    public bool 책임초안구조완료 { get; set; }
    public bool 전문검토인계가능 { get; set; }
    public bool 계약서명가능 { get; set; }
    public bool 결제가능 { get; set; }
    public bool 신고실행가능 { get; set; }
    public bool 운송지시가능 { get; set; }
    public bool 포워더자동선정가능 { get; set; }
    public bool 외부자동전송가능 { get; set; }
    public IReadOnlyList<string> 차단사유목록 { get; set; } = [];
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public IReadOnlyList<string> 명시된미확인항목목록 { get; set; } = [];
}

public sealed class 같이수입준비원장응답
{
    public string 원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public bool 생성됨 { get; set; }
    public bool 이미처리됨 { get; set; }
    public string 상태코드 { get; set; } = 같이수입준비원장상태코드.초안;
    public string 자동집단Id { get; set; } = string.Empty;
    public string 원천수요운영체제Id { get; set; } = string.Empty;
    public string 원천인계요청Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 원천Hs코드 { get; set; } = string.Empty;
    public decimal 모인수요수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public 공동구매거래문맥응답 거래문맥 { get; set; } = new();
    public IReadOnlyList<같이수입준비원천수요응답> 원천수요목록 { get; set; } = [];
    public 같이수입준비원장저장요청 준비자료 { get; set; } = new();
    public 같이수입준비원장평가응답 평가 { get; set; } = new();
    public DateTimeOffset 저장시각Utc { get; set; }
}

public sealed class 같이수입준비원천수요응답
{
    public string 자동집단Id { get; set; } = string.Empty;
    public string 인계요청Id { get; set; } = string.Empty;
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public string 재료키 { get; set; } = string.Empty;
    public string 재료명 { get; set; } = string.Empty;
    public string 원천Hs코드 { get; set; } = string.Empty;
    public decimal 모인수요수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
}

public static class 같이수입준비원장정책
{
    public static 같이수입준비원장평가응답 평가(
        같이수입준비원장저장요청 request,
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
        var destinationCountry = 같이수입준비국가코드.정규화(request.도착국가코드);
        var baseCurrency = NormalizeCurrency(request.기준통화코드);
        var materialItems = 유효재료품목목록(request, group);

        var destinationReady = 같이수입준비국가코드.지원목록.Contains(destinationCountry);
        if (!destinationReady)
        {
            blockers.Add("도착 국가는 한국(KR) 또는 미국(US)으로 명시해야 합니다.");
        }
        var originReady = NormalizeCountry(request.출발국가코드).Length == 2;
        if (!originReady)
        {
            blockers.Add("선적 출발국가 ISO 2자리 코드가 필요합니다.");
        }
        var currencyReady = baseCurrency.Length == 3;
        if (!currencyReady)
        {
            blockers.Add("예상 원가 비교에 사용할 ISO 4217 기준통화가 필요합니다.");
        }

        var materialReady = EvaluateMaterials(materialItems, group, blockers);
        var supplierReady = EvaluateSuppliers(request, evaluatedAtUtc, blockers, warnings);
        var quoteReady = EvaluateQuotes(request, materialItems, evaluatedAtUtc, baseCurrency, blockers, warnings);
        var costReady = EvaluateCosts(request, materialItems, evaluatedAtUtc, baseCurrency, blockers, warnings);
        var classificationReady = EvaluateClassifications(
            request,
            materialItems,
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
        var forwarderHandoff = EvaluateForwarderHandoff(request, blockers, warnings, unresolved);
        var transportReady = EvaluateInternationalTransport(request, blockers, warnings, unresolved);
        var responsibilityReady = EvaluateResponsibilities(request, blockers);
        var forwarderPreparationReady = materialReady
                                        && destinationReady
                                        && originReady
                                        && forwarderHandoff.StructureReady;
        var forwarderResponseRecorded = forwarderHandoff.HandoffRecorded
                                        && 포워더회신완료여부(request);
        var handoffReady = materialReady
                           && supplierReady
                           && quoteReady
                           && costReady
                           && classificationReady
                           && complianceReady
                           && forwarderHandoff.StructureReady
                           && transportReady
                           && responsibilityReady
                           && blockers.Count == 0;

        return new 같이수입준비원장평가응답
        {
            재료품목구조완료 = materialReady,
            공급자근거구조완료 = supplierReady,
            견적구조완료 = quoteReady,
            예상비용구조완료 = costReady,
            품목분류후보구조완료 = classificationReady,
            국가별검토구조완료 = complianceReady,
            포워더인계구조완료 = forwarderHandoff.StructureReady,
            포워더인계준비가능 = forwarderPreparationReady,
            포워더인계기록완료 = forwarderHandoff.HandoffRecorded,
            포워더회신기록완료 = forwarderResponseRecorded,
            국제운송검토구조완료 = transportReady,
            책임초안구조완료 = responsibilityReady,
            전문검토인계가능 = handoffReady,
            계약서명가능 = false,
            결제가능 = false,
            신고실행가능 = false,
            운송지시가능 = false,
            포워더자동선정가능 = false,
            외부자동전송가능 = false,
            차단사유목록 = blockers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            경고목록 = warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            명시된미확인항목목록 = unresolved.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        };
    }

    public static void 단일재료호환정규화(
        같이수입준비원장저장요청 request,
        공동구매자동집단응답 group)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(group);

        request.재료품목목록 ??= [];
        request.포워더인계 ??= new 같이수입준비포워더인계();
        request.포워더인계.전달항목코드목록 ??= [.. 같이수입준비포워더전달항목코드.기본집계목록];
        request.국제운송검토 ??= new 같이수입준비국제운송검토();
        request.국제운송검토.방식후보목록 ??= [.. 같이수입준비국제운송방식코드.비교후보목록];
        if (request.재료품목목록.Count == 0)
        {
            request.재료품목목록.Add(new 같이수입준비재료품목
            {
                재료키 = string.IsNullOrWhiteSpace(request.재료키) ? group.상품키 : request.재료키.Trim(),
                재료명 = string.IsNullOrWhiteSpace(request.재료명) ? group.상품명 : request.재료명.Trim(),
                원천자동집단Id = group.자동집단Id,
                원천Hs코드 = group.HS코드,
                모인수요수량 = group.총희망수량,
                수량단위 = group.수량단위
            });
        }

        var first = request.재료품목목록[0];
        request.재료키 = string.IsNullOrWhiteSpace(first.재료키) ? request.재료키 : first.재료키.Trim();
        request.재료명 = string.IsNullOrWhiteSpace(first.재료명) ? request.재료명 : first.재료명.Trim();
    }

    public static IReadOnlyList<같이수입준비재료품목> 유효재료품목목록(
        같이수입준비원장저장요청 request,
        공동구매자동집단응답 group)
    {
        if (request.재료품목목록 is { Count: > 0 })
        {
            return request.재료품목목록;
        }

        return
        [
            new 같이수입준비재료품목
            {
                재료키 = string.IsNullOrWhiteSpace(request.재료키) ? group.상품키 : request.재료키.Trim(),
                재료명 = string.IsNullOrWhiteSpace(request.재료명) ? group.상품명 : request.재료명.Trim(),
                원천자동집단Id = group.자동집단Id,
                원천Hs코드 = group.HS코드,
                모인수요수량 = group.총희망수량,
                수량단위 = group.수량단위
            }
        ];
    }

    private static bool EvaluateMaterials(
        IReadOnlyList<같이수입준비재료품목> materials,
        공동구매자동집단응답 anchorGroup,
        ICollection<string> blockers)
    {
        if (materials.Count == 0)
        {
            blockers.Add("같이 수입 준비 묶음에는 재료 품목이 하나 이상 필요합니다.");
            return false;
        }

        var ready = true;
        if (HasDuplicate(materials.Select(item => item.재료키)))
        {
            blockers.Add("재료 키는 한 준비 묶음 안에서 중복될 수 없습니다.");
            ready = false;
        }
        if (HasDuplicate(materials.Select(item => item.원천자동집단Id)))
        {
            blockers.Add("같은 1.0 수요 집단을 준비 묶음에 두 번 넣을 수 없습니다.");
            ready = false;
        }

        foreach (var material in materials)
        {
            if (string.IsNullOrWhiteSpace(material.재료키)
                || string.IsNullOrWhiteSpace(material.재료명)
                || string.IsNullOrWhiteSpace(material.원천자동집단Id)
                || material.모인수요수량 <= 0
                || string.IsNullOrWhiteSpace(material.수량단위))
            {
                blockers.Add($"재료 '{Label(material.재료명, material.재료키)}'에 재료 키·원천 수요 집단·모인 수량과 단위가 필요합니다.");
                ready = false;
            }
        }

        var anchor = materials.FirstOrDefault(item => string.Equals(
            item.원천자동집단Id?.Trim(),
            anchorGroup.자동집단Id,
            StringComparison.Ordinal));
        if (anchor is null)
        {
            blockers.Add("경로의 기준 1.0 수요 집단이 재료 품목 묶음에 포함되어야 합니다.");
            ready = false;
        }
        else if (!string.Equals(anchor.재료키?.Trim(), anchorGroup.상품키, StringComparison.OrdinalIgnoreCase))
        {
            blockers.Add("기준 수요 집단과 연결된 재료 키가 원천 상품 키와 일치해야 합니다.");
            ready = false;
        }

        return ready;
    }

    private static bool EvaluateSuppliers(
        같이수입준비원장저장요청 request,
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
        같이수입준비원장저장요청 request,
        IReadOnlyList<같이수입준비재료품목> materials,
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
        var materialKeys = materials
            .Select(item => item.재료키.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var quotedMaterialKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (HasDuplicate(request.견적목록.Select(item => item.견적키)))
        {
            blockers.Add("견적 키는 원장 안에서 중복될 수 없습니다.");
            ready = false;
        }

        foreach (var quote in request.견적목록)
        {
            var quoteCurrency = NormalizeCurrency(quote.통화코드);
            var materialKey = ResolveMaterialKey(quote.재료키, materials);
            if (string.IsNullOrWhiteSpace(quote.견적키)
                || string.IsNullOrWhiteSpace(materialKey)
                || !materialKeys.Contains(materialKey)
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
                blockers.Add($"견적 '{Label(quote.견적키, "미지정")}'에 대상 재료, 공급자, 통화, 단위, MOQ, 단가, 납기, 포장, Incoterms 후보와 원출처가 필요합니다.");
                ready = false;
            }
            else
            {
                quotedMaterialKeys.Add(materialKey);
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

        foreach (var material in materials.Where(item => !quotedMaterialKeys.Contains(item.재료키.Trim())))
        {
            blockers.Add($"재료 '{Label(material.재료명, material.재료키)}'에 연결된 공급자 견적이 하나 이상 필요합니다.");
            ready = false;
        }

        return ready;
    }

    private static bool EvaluateCosts(
        같이수입준비원장저장요청 request,
        IReadOnlyList<같이수입준비재료품목> materials,
        DateTimeOffset now,
        string baseCurrency,
        ICollection<string> blockers,
        ICollection<string> warnings)
    {
        var ready = true;
        var materialKeys = materials
            .Select(item => item.재료키.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var categories = request.예상비용목록
            .Where(item => !string.IsNullOrWhiteSpace(item.범주코드))
            .Select(item => item.범주코드.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var requiredCategory in 같이수입준비비용범주코드.필수목록)
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
                || !string.IsNullOrWhiteSpace(cost.재료키) && !materialKeys.Contains(cost.재료키.Trim())
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
        같이수입준비원장저장요청 request,
        IReadOnlyList<같이수입준비재료품목> materials,
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
        var expectedSystem = 같이수입준비품목분류체계코드.대상국가체계(destinationCountry);
        var materialByKey = materials.ToDictionary(
            item => item.재료키.Trim(),
            StringComparer.OrdinalIgnoreCase);
        var coveredMaterialKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in destinationCandidates)
        {
            var materialKey = ResolveMaterialKey(candidate.재료키, materials);
            var candidateCode = Digits(candidate.품목코드);
            if (string.IsNullOrWhiteSpace(candidate.후보키)
                || string.IsNullOrWhiteSpace(materialKey)
                || !materialByKey.ContainsKey(materialKey)
                || !string.Equals(candidate.분류체계코드.Trim(), expectedSystem, StringComparison.OrdinalIgnoreCase)
                || candidateCode.Length is < 6 or > 10
                || string.IsNullOrWhiteSpace(candidate.분류근거)
                || candidate.신뢰도 is < 0 or > 1
                || !같이수입준비검토상태코드.지원목록.Contains(candidate.검토상태코드)
                || !IsSourceUrl(candidate.원출처Url)
                || !IsObservedTime(candidate.확인시각Utc, now))
            {
                blockers.Add($"품목분류 후보 '{Label(candidate.품목코드, candidate.후보키)}'에 대상 재료, 관할 체계, 코드, 근거, 신뢰도, 검토 상태와 원출처가 필요합니다.");
                ready = false;
            }
            else
            {
                var sourceHs = Digits(materialByKey[materialKey].원천Hs코드);
                if (sourceHs.Length >= 6
                    && candidateCode.Length >= 6
                    && sourceHs[..6] != candidateCode[..6])
                {
                    blockers.Add($"재료 '{materialByKey[materialKey].재료명}'의 1.0 참고 HS 코드와 같은 6단위 계열 후보가 필요합니다.");
                    ready = false;
                }
                else
                {
                    coveredMaterialKeys.Add(materialKey);
                }
            }
            if (candidate.전문가검토필요
                || !string.Equals(
                    candidate.검토상태코드,
                    같이수입준비검토상태코드.전문가검토완료,
                    StringComparison.OrdinalIgnoreCase))
            {
                unresolved.Add($"{destinationCountry} {expectedSystem} 후보 {candidate.품목코드}의 자격 있는 전문가 검토");
            }
        }

        foreach (var material in materials.Where(item => !coveredMaterialKeys.Contains(item.재료키.Trim())))
        {
            blockers.Add($"재료 '{Label(material.재료명, material.재료키)}'에 연결된 {expectedSystem} 후보가 필요합니다.");
            ready = false;
        }

        return ready;
    }

    private static bool EvaluateCompliance(
        같이수입준비원장저장요청 request,
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
            if (!같이수입준비국가코드.지원목록.Contains(country)
                || string.IsNullOrWhiteSpace(item.항목코드)
                || string.IsNullOrWhiteSpace(item.표시명)
                || !같이수입준비검토상태코드.지원목록.Contains(item.검토상태코드)
                || string.IsNullOrWhiteSpace(item.책임역할코드)
                || !IsSourceUrl(item.공식원출처Url)
                || !IsObservedTime(item.확인시각Utc, now))
            {
                blockers.Add($"국가별 검토 항목 '{Label(item.표시명, item.항목코드)}'에 국가, 상태, 책임 역할, 공식 출처와 확인 시각이 필요합니다.");
                ready = false;
            }
            if (string.Equals(item.검토상태코드, 같이수입준비검토상태코드.미확인, StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.검토상태코드, 같이수입준비검토상태코드.전문가검토필요, StringComparison.OrdinalIgnoreCase))
            {
                unresolved.Add(string.IsNullOrWhiteSpace(item.미확인사유)
                    ? $"{country} {item.표시명} 검토"
                    : $"{country} {item.표시명}: {item.미확인사유.Trim()}");
            }
        }

        return ready;
    }

    private static bool EvaluateInternationalTransport(
        같이수입준비원장저장요청 request,
        ICollection<string> blockers,
        ICollection<string> warnings,
        ICollection<string> unresolved)
    {
        var review = request.국제운송검토 ?? new 같이수입준비국제운송검토();
        var candidates = (review.방식후보목록 ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var ready = true;
        if (!같이수입준비국제운송검토상태코드.지원목록.Contains(review.검토상태코드)
            || candidates.Length == 0
            || candidates.Any(item => !같이수입준비국제운송방식코드.지원여부(item)))
        {
            blockers.Add("국제 운송 검토에는 LCL·FCL 중 하나 이상의 비교 후보와 유효한 검토 상태가 필요합니다.");
            ready = false;
        }

        var proposal = ForwarderProposal(review);
        if (!string.IsNullOrWhiteSpace(proposal)
            && (!같이수입준비국제운송방식코드.지원여부(proposal)
                || !candidates.Contains(proposal, StringComparer.OrdinalIgnoreCase)))
        {
            blockers.Add("포워더가 제안한 국제 운송 방식은 원장에 기록된 LCL·FCL 비교 후보 중 하나여야 합니다.");
            ready = false;
        }

        var responseState = string.Equals(
            review.검토상태코드,
            같이수입준비국제운송검토상태코드.포워더회신완료,
            StringComparison.OrdinalIgnoreCase);
        if (responseState
            && (string.IsNullOrWhiteSpace(proposal)
                || string.IsNullOrWhiteSpace(ForwarderResponseSummary(review))
                || string.IsNullOrWhiteSpace(ForwarderResponseCompany(review))
                || string.IsNullOrWhiteSpace(ForwarderResponseRecorder(review))
                || !ForwarderResponseTime(review).HasValue))
        {
            blockers.Add("포워더 회신 완료에는 제안 방식, 수량·부피·온도·일정·견적 근거, 회신 업체, 기록자와 회신 시각이 필요합니다.");
            ready = false;
        }

        if (!responseState)
        {
            const string message = "LCL/FCL은 사용자나 플랫폼의 재료 선택값이 아닙니다. 여러 재료의 합산 물류 조건을 받은 포워더·물류대행업체의 회신 제안으로 기록해야 합니다.";
            warnings.Add(message);
            unresolved.Add("여러 재료 합산 조건을 전달받은 포워더·물류대행업체의 LCL/FCL 회신");
        }

        return ready;
    }

    private static (bool StructureReady, bool HandoffRecorded) EvaluateForwarderHandoff(
        같이수입준비원장저장요청 request,
        ICollection<string> blockers,
        ICollection<string> warnings,
        ICollection<string> unresolved)
    {
        var handoff = request.포워더인계 ?? new 같이수입준비포워더인계();
        var allowedItems = 같이수입준비포워더전달항목코드.기본집계목록
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var items = (handoff.전달항목코드목록 ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var ready = true;
        if (!같이수입준비포워더인계상태코드.지원목록.Contains(handoff.인계상태코드)
            || !같이수입준비포워더전달정보범위코드.지원목록.Contains(handoff.전달정보범위코드)
            || items.Length == 0
            || items.Any(item => !allowedItems.Contains(item))
            || string.IsNullOrWhiteSpace(handoff.전달범위요약)
            || string.IsNullOrWhiteSpace(handoff.전달패키지버전))
        {
            blockers.Add("포워더 인계 자료에는 유효한 상태, 최소 전달 범위, 집계 항목, 범위 요약과 패키지 버전이 필요합니다.");
            ready = false;
        }

        var individualScope = string.Equals(
            handoff.전달정보범위코드,
            같이수입준비포워더전달정보범위코드.동의된사용자별최소정보,
            StringComparison.OrdinalIgnoreCase);
        if (handoff.개인정보포함여부 && !individualScope)
        {
            blockers.Add("개인정보를 포함하려면 전달 범위를 '동의된 사용자별 최소정보'로 명시해야 합니다.");
            ready = false;
        }
        if ((handoff.개인정보포함여부 || individualScope)
            && (!handoff.정보제공동의확인여부
                || string.IsNullOrWhiteSpace(handoff.정보제공동의근거참조)))
        {
            blockers.Add("사용자별 정보를 전달하려면 명시적인 정보 제공 동의와 철회 가능한 동의 근거 참조가 필요합니다.");
            ready = false;
        }

        var preparedOrLater = !string.Equals(
            handoff.인계상태코드,
            같이수입준비포워더인계상태코드.초안,
            StringComparison.OrdinalIgnoreCase);
        if (preparedOrLater && string.IsNullOrWhiteSpace(handoff.전달대상업체명))
        {
            blockers.Add("인계 준비 이후 상태에는 사람이 정한 포워더 또는 물류대행업체 이름이 필요합니다.");
            ready = false;
        }

        var recorded = string.Equals(
                           handoff.인계상태코드,
                           같이수입준비포워더인계상태코드.인계기록됨,
                           StringComparison.OrdinalIgnoreCase)
                       || string.Equals(
                           handoff.인계상태코드,
                           같이수입준비포워더인계상태코드.회신기록됨,
                           StringComparison.OrdinalIgnoreCase);
        if (recorded
            && (string.IsNullOrWhiteSpace(handoff.전달대상업체명)
                || string.IsNullOrWhiteSpace(handoff.인계기록자표시명)
                || !handoff.인계시각Utc.HasValue))
        {
            blockers.Add("포워더 인계 기록에는 전달 대상 업체, 기록자와 실제 인계 시각이 필요합니다.");
            ready = false;
            recorded = false;
        }

        if (!recorded)
        {
            warnings.Add("OS는 외부 전송을 실행하지 않습니다. 사람이 포워더·물류대행업체에 전달한 뒤 그 사실만 원장에 기록합니다.");
            unresolved.Add("사람이 정한 포워더·물류대행업체에 최소 정보 패키지를 전달하고 인계 사실 기록");
        }

        return (ready, recorded);
    }

    public static bool 포워더회신완료여부(같이수입준비원장저장요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var review = request.국제운송검토 ?? new 같이수입준비국제운송검토();
        return string.Equals(
                   review.검토상태코드,
                   같이수입준비국제운송검토상태코드.포워더회신완료,
                   StringComparison.OrdinalIgnoreCase)
               && 같이수입준비국제운송방식코드.지원여부(ForwarderProposal(review))
               && !string.IsNullOrWhiteSpace(ForwarderResponseSummary(review))
               && !string.IsNullOrWhiteSpace(ForwarderResponseCompany(review))
               && !string.IsNullOrWhiteSpace(ForwarderResponseRecorder(review))
               && ForwarderResponseTime(review).HasValue;
    }

    private static string ForwarderProposal(같이수입준비국제운송검토 review)
        => FirstValue(review.포워더제안방식코드, review.선택방식코드).ToUpperInvariant();

    private static string ForwarderResponseSummary(같이수입준비국제운송검토 review)
        => FirstValue(review.포워더회신요약, review.판단근거);

    private static string ForwarderResponseCompany(같이수입준비국제운송검토 review)
        => FirstValue(review.회신업체표시명, review.검토자표시명);

    private static string ForwarderResponseRecorder(같이수입준비국제운송검토 review)
        => FirstValue(review.회신기록자표시명, review.검토자표시명);

    private static DateTimeOffset? ForwarderResponseTime(같이수입준비국제운송검토 review)
        => review.회신시각Utc ?? review.검토시각Utc;

    private static string FirstValue(string? preferred, string? fallback)
        => !string.IsNullOrWhiteSpace(preferred)
            ? preferred.Trim()
            : fallback?.Trim() ?? string.Empty;

    private static bool EvaluateResponsibilities(
        같이수입준비원장저장요청 request,
        ICollection<string> blockers)
    {
        var ready = true;
        var roles = request.책임초안목록
            .Where(item => !string.IsNullOrWhiteSpace(item.역할코드))
            .Select(item => item.역할코드.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var requiredRole in 같이수입준비책임역할코드.필수초안역할목록)
        {
            if (!roles.Contains(requiredRole))
            {
                blockers.Add($"같이 수입 책임 초안에 '{requiredRole}' 역할이 필요합니다.");
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

    private static string ResolveMaterialKey(
        string? requestedMaterialKey,
        IReadOnlyList<같이수입준비재료품목> materials)
    {
        if (!string.IsNullOrWhiteSpace(requestedMaterialKey))
        {
            return requestedMaterialKey.Trim();
        }

        return materials.Count == 1 ? materials[0].재료키.Trim() : string.Empty;
    }

    private static bool IsSourceUrl(string? value)
        => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static bool IsObservedTime(DateTimeOffset value, DateTimeOffset now)
        => value != default && value <= now.AddMinutes(5);

    private static string NormalizeCountry(string? value)
        => 같이수입준비국가코드.정규화(value);

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
