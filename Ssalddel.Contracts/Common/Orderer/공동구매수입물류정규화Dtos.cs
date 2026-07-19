namespace Ssalddel.Contracts.Common.Orderer;

public static class 수입물류참조코드유형
{
    public const string 항만 = "Port";
    public const string 공항 = "Airport";
    public const string 세관 = "CustomsOffice";
    public const string 보세구역 = "BondedArea";
    public const string 알수없음 = "Unknown";
}

public static class 수입물류시뮬레이션위험코드
{
    public const string 낮음 = "Low";
    public const string 중간 = "Medium";
    public const string 높음 = "High";
    public const string 검토필요 = "NeedsReview";
}

public sealed class 수입물류참조조회요청
{
    public string? 검색어 { get; set; }
    public string? 운송수단 { get; set; }
    public string? 코드유형 { get; set; }
    public int 페이지크기 { get; set; } = 20;
}

public sealed class 수입물류참조항목
{
    public string Code { get; set; } = string.Empty;
    public string 코드유형 { get; set; } = 수입물류참조코드유형.알수없음;
    public string Name { get; set; } = string.Empty;
    public string 지역명 { get; set; } = string.Empty;
    public string 운송수단 { get; set; } = string.Empty;
    public string 관련항만공항코드 { get; set; } = string.Empty;
    public string 관련세관코드 { get; set; } = string.Empty;
    public string 출처명 { get; set; } = string.Empty;
    public string 출처Url { get; set; } = string.Empty;
    public bool 공식검증필요 { get; set; }
}

public sealed class 수입물류정규화시뮬레이션요청
{
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 운송문서유형 { get; set; } = 공동구매선적문서유형코드.선하증권;
    public string 운송문서번호 { get; set; } = string.Empty;
    public string 운송수단 { get; set; } = 공동구매선적운송수단코드.해상;
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 출발항코드 { get; set; } = string.Empty;
    public string 도착항코드 { get; set; } = string.Empty;
    public string 도착항만공항명 { get; set; } = string.Empty;
    public string 세관코드 { get; set; } = string.Empty;
    public string 세관명 { get; set; } = string.Empty;
    public string 보세구역코드 { get; set; } = string.Empty;
    public string 보세구역명 { get; set; } = string.Empty;
    public string 현재위치요약 { get; set; } = string.Empty;
    public string 통관단계명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public decimal? 화물인보이스금액Usd { get; set; }
    public decimal? 화물중량Kg { get; set; }
    public decimal? 예상국내입고비용Krw { get; set; }
    public int? 예상보세보관일수 { get; set; }
}

public sealed class 수입물류정규화시뮬레이션결과
{
    public bool Success { get; set; }
    public string 문서관리번호 { get; set; } = string.Empty;
    public IReadOnlyList<수입물류참조항목> 정규화참조목록 { get; set; } = [];
    public IReadOnlyList<수입물류흐름단계Dto> 제안흐름목록 { get; set; } = [];
    public 수입물류비용위험시뮬레이션Dto Simulation { get; set; } = new();
    public IReadOnlyList<string> 경고목록 { get; set; } = [];
    public IReadOnlyList<수입물류출처Dto> 출처목록 { get; set; } = [];
}

public sealed class 수입물류흐름단계Dto
{
    public int 순서 { get; set; }
    public string 단계코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 책임주체코드 { get; set; } = string.Empty;
    public string 참조코드 { get; set; } = string.Empty;
    public string 참조명 { get; set; } = string.Empty;
    public bool 공식코드확인됨 { get; set; }
}

public sealed class 수입물류비용위험시뮬레이션Dto
{
    public decimal? 인보이스단가UsdPerKg { get; set; }
    public decimal? 예상국내입고비용KrwPerKg { get; set; }
    public string 통관경로위험코드 { get; set; } = 수입물류시뮬레이션위험코드.검토필요;
    public string 신뢰도코드 { get; set; } = 수입물류시뮬레이션위험코드.검토필요;
    public string 요약 { get; set; } = string.Empty;
}

public sealed class 수입물류출처Dto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string 용도 { get; set; } = string.Empty;
}
