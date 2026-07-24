using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Orderer;

public abstract class 무역확장원장생성요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public long? 기대원천Revision { get; set; }
    public string 제목 { get; set; } = string.Empty;
    public string 거래문맥 { get; set; } = "B2C";
    public string 메모 { get; set; } = string.Empty;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Contract,
    "개별주문을 원천으로 개별수입 확장 원장을 만드는 요청 계약입니다.",
    FlowOrder = 10,
    Boundary = "상품·수량·가격·계약·서명 원본은 복제하지 않고 원천 주문 원장 ID만 참조합니다.")]
public sealed class 개별수입원장생성요청 : 무역확장원장생성요청
{
    public string 수입주체 { get; set; } = string.Empty;
    public string 해외판매자 { get; set; } = string.Empty;
    public string Incoterms후보 { get; set; } = string.Empty;
    public string 통관검토메모 { get; set; } = string.Empty;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Contract,
    "개별주문을 원천으로 개별수출 확장 원장을 만드는 요청 계약입니다.",
    FlowOrder = 10,
    Boundary = "수출 신고·허가·포워더 전송을 실행하지 않고 검토 상태와 근거만 저장합니다.")]
public sealed class 개별수출원장생성요청 : 무역확장원장생성요청
{
    public string 수출자 { get; set; } = string.Empty;
    public string 해외구매자 { get; set; } = string.Empty;
    public string 목적국가코드 { get; set; } = string.Empty;
    public string Incoterms후보 { get; set; } = string.Empty;
    public string 규정검토메모 { get; set; } = string.Empty;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.Contract,
    "여러 개별수출 원장을 물류 집계 대상으로 묶는 공동수출 원장 생성 계약입니다.",
    FlowOrder = 10,
    Boundary = "개별수출 원장의 수출자·신고·서류·적재 실적을 공동 원장으로 복제하거나 덮어쓰지 않습니다.")]
public sealed class 공동수출원장생성요청
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public IReadOnlyList<string> 개별수출원장Ids { get; set; } = [];
    public string 집하마감 { get; set; } = string.Empty;
    public string 포워더인계메모 { get; set; } = string.Empty;
    public string 공통비배부근거 { get; set; } = string.Empty;
}

public sealed class 무역확장원장응답
{
    public 무역확장원장요약응답 원장 { get; set; } = new();
    public IReadOnlyList<무역확장원장요약응답> 원천원장목록 { get; set; } = [];
    public bool 이미처리됨 { get; set; }
    public bool 외부실행발생여부 { get; set; }
    public string 실행모드 { get; set; } = "Simulation";
}

public sealed class 무역확장원장요약응답
{
    public string 원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public IReadOnlyList<string> 원천원장Ids { get; set; } = [];
}
