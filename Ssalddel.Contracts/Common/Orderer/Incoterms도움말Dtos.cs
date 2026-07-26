using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Orderer;

public static class Incoterms도움말역할코드
{
    public const string 판매자 = "Seller";
    public const string 구매자 = "Buyer";
}

public static class Incoterms도움말구간코드
{
    public const string 판매자출고 = "SellerDispatch";
    public const string 수출통관선적항 = "ExportAndOriginPort";
    public const string 본선적재 = "LoadedOnBoard";
    public const string 주운송 = "MainCarriage";
    public const string 수입통관 = "ImportClearance";
    public const string 지정목적지 = "NamedDestination";
    public const string 하역 = "Unloading";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Contract,
    "주문자가 FOB, CIF, DDP의 비용·위험 이전 구조를 그림으로 이해할 수 있게 하는 Incoterms 도움말 계약입니다.",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "교육용 요약이며 계약 해석, 가격·대금 지급, 소유권 이전, 품질 조건 또는 전문가 검토를 대신하지 않습니다.")]
public sealed class Incoterms도움말응답
{
    public string 언어코드 { get; init; } = DisplayLanguageCodes.Korean;
    public string 선택코드 { get; init; } = 공동수입준비Incoterms코드.Fob;
    public string 화면제목 { get; init; } = string.Empty;
    public string 소개 { get; init; } = string.Empty;
    public string 버전표시 { get; init; } = "Incoterms® 2020";
    public string 장소표기안내 { get; init; } = string.Empty;
    public IReadOnlyList<Incoterms도움말항목> 항목목록 { get; init; } = [];
    public IReadOnlyList<Incoterms도움말출처> 공식출처목록 { get; init; } = [];
    public string 면책안내 { get; init; } = string.Empty;
}

public sealed class Incoterms도움말항목
{
    public string 코드 { get; init; } = string.Empty;
    public string 영문명 { get; init; } = string.Empty;
    public string 한줄요약 { get; init; } = string.Empty;
    public string 적용운송범위 { get; init; } = string.Empty;
    public string 판매자책임요약 { get; init; } = string.Empty;
    public string 구매자책임요약 { get; init; } = string.Empty;
    public string 비용이전설명 { get; init; } = string.Empty;
    public string 위험이전설명 { get; init; } = string.Empty;
    public bool 판매자보험부보여부 { get; init; }
    public string 보험설명 { get; init; } = string.Empty;
    public IReadOnlyList<Incoterms도움말그림구간> 그림구간목록 { get; init; } = [];
}

public sealed class Incoterms도움말그림구간
{
    public int 순서 { get; init; }
    public string 구간코드 { get; init; } = string.Empty;
    public string 표시명 { get; init; } = string.Empty;
    public string 비용부담역할코드 { get; init; } = string.Empty;
    public string 위험부담역할코드 { get; init; } = string.Empty;
    public bool 위험이전지점여부 { get; init; }
    public bool 보험표시여부 { get; init; }
}

public sealed class Incoterms도움말출처
{
    public string 출처명 { get; init; } = string.Empty;
    public string 출처Url { get; init; } = string.Empty;
    public string 확인기준일 { get; init; } = string.Empty;
}
