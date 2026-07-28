using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 같이 수입 준비 원장에서 주문자가 확인할 수 있는 공개 표시값만 투영한 읽기 계약입니다.
/// 원장·수요·업체 내부 키, revision, 멱등 요청, 담당자와 동의 근거 원문은 포함하지 않습니다.
/// </summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Contract,
    "참여 주문자에게 같이 수입 준비의 집계·상태·공식 근거만 제공하는 1.5 최소 읽기 계약입니다.",
    FlowOrder = 12,
    Effects = SsalddelCodeEffect.None,
    Boundary = "내부 원장 키, 요청 멱등키, revision, 공급자·포워더 내부 키, 담당자 표시명과 정보제공 동의 근거 원문을 노출하지 않습니다.")]
public sealed class 같이수입준비주문자조회응답
{
    public string 상품명 { get; set; } = string.Empty;
    public string 상태코드 { get; set; } = 같이수입준비원장상태코드.초안;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public 주문자해외구매통관안내 통관목적안내 { get; set; } = new();
    public DateTimeOffset 기준시각Utc { get; set; }
    public IReadOnlyList<같이수입준비주문자재료집계응답> 재료집계목록 { get; set; } = [];
    public 같이수입준비주문자진행상태응답 준비현황 { get; set; } = new();
    public IReadOnlyList<같이수입준비주문자공급자근거응답> 공급자근거목록 { get; set; } = [];
    public IReadOnlyList<같이수입준비주문자견적근거응답> 견적목록 { get; set; } = [];
    public IReadOnlyList<같이수입준비주문자예상비용응답> 예상비용목록 { get; set; } = [];
    public IReadOnlyList<같이수입준비주문자품목분류응답> 품목분류목록 { get; set; } = [];
    public IReadOnlyList<같이수입준비주문자국가별검토응답> 국가별검토목록 { get; set; } = [];
    public 같이수입준비주문자포워더인계응답 포워더인계 { get; set; } = new();
    public 같이수입준비주문자국제운송응답 국제운송검토 { get; set; } = new();
}

public sealed class 같이수입준비주문자재료집계응답
{
    public string 재료명 { get; set; } = string.Empty;
    public decimal 모인수요수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
}

public sealed class 같이수입준비주문자진행상태응답
{
    public bool 재료집계완료 { get; set; }
    public bool 공급자근거있음 { get; set; }
    public bool 견적근거있음 { get; set; }
    public bool 예상비용근거있음 { get; set; }
    public bool 품목분류근거있음 { get; set; }
    public bool 국가별검토근거있음 { get; set; }
    public bool 전문검토준비됨 { get; set; }
    public bool 포워더인계준비됨 { get; set; }
}

public sealed class 같이수입준비주문자공급자근거응답
{
    public string 조직명 { get; set; } = string.Empty;
    public string 국가코드 { get; set; } = string.Empty;
    public string 관계코드 { get; set; } = string.Empty;
    public string 근거요약 { get; set; } = string.Empty;
    public string 원출처명 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public bool 최신상태재확인필요 { get; set; }
}

public sealed class 같이수입준비주문자견적근거응답
{
    public string 재료명 { get; set; } = string.Empty;
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

public sealed class 같이수입준비주문자예상비용응답
{
    public string 재료명 { get; set; } = string.Empty;
    public string 범주코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 통화코드 { get; set; } = string.Empty;
    public decimal 예상금액 { get; set; }
    public string 계산근거 { get; set; } = string.Empty;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public DateTimeOffset? 유효기한Utc { get; set; }
}

public sealed class 같이수입준비주문자품목분류응답
{
    public string 재료명 { get; set; } = string.Empty;
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 분류체계코드 { get; set; } = string.Empty;
    public string 품목코드 { get; set; } = string.Empty;
    public string 분류근거 { get; set; } = string.Empty;
    public decimal 신뢰도 { get; set; }
    public string 검토상태코드 { get; set; } = 같이수입준비검토상태코드.전문가검토필요;
    public string 원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public bool 전문가검토필요 { get; set; }
}

public sealed class 같이수입준비주문자국가별검토응답
{
    public string 관할국가코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 검토상태코드 { get; set; } = 같이수입준비검토상태코드.미확인;
    public string 책임역할코드 { get; set; } = string.Empty;
    public string 공식원출처Url { get; set; } = string.Empty;
    public DateTimeOffset 확인시각Utc { get; set; }
    public string 미확인사유 { get; set; } = string.Empty;
}

public sealed class 같이수입준비주문자포워더인계응답
{
    public string 인계상태코드 { get; set; } = 같이수입준비포워더인계상태코드.초안;
    public string 전달대상업체명 { get; set; } = string.Empty;
    public string 전달정보범위코드 { get; set; } = 같이수입준비포워더전달정보범위코드.집계수요전용;
    public IReadOnlyList<string> 전달항목코드목록 { get; set; } = [];
    public string 전달범위요약 { get; set; } = string.Empty;
    public bool 개인정보포함여부 { get; set; }
    public bool 운영자기록정보제공조건확인여부 { get; set; }
    public DateTimeOffset? 인계시각Utc { get; set; }
}

public sealed class 같이수입준비주문자국제운송응답
{
    public string 검토상태코드 { get; set; } = 같이수입준비국제운송검토상태코드.검토필요;
    public IReadOnlyList<string> 방식후보목록 { get; set; } = [];
    public string 포워더제안방식코드 { get; set; } = string.Empty;
    public string 포워더회신요약 { get; set; } = string.Empty;
    public string 회신업체표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 회신시각Utc { get; set; }
}
