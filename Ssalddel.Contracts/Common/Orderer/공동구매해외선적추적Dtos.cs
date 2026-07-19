namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동구매선적운송수단코드
{
    public const string 해상 = "Ocean";
    public const string 항공 = "Air";
}

public static class 공동구매선적문서유형코드
{
    public const string 선하증권 = "BillOfLading";
    public const string 항공화물운송장 = "AirWaybill";
}

public static class 공동구매선적상태코드
{
    public const string 문서등록 = "DocumentRegistered";
    public const string 해외포장완료 = "OverseasPacked";
    public const string 선박항공편적재 = "LoadedOnVesselOrFlight";
    public const string 운송중 = "InTransit";
    public const string 항만도착 = "ArrivedAtPort";
    public const string 통관진행중 = "CustomsInProgress";
    public const string 통관완료 = "CustomsCleared";
    public const string 물류대행입고준비 = "LogisticsProxyInboundReady";
    public const string 물류대행입고완료 = "LogisticsProxyInboundCompleted";
    public const string 출품준비 = "SalesListingReady";
    public const string 판매채널출품완료 = "SalesChannelListed";
    public const string 출고배치준비 = "OutboundBatchReady";
    public const string 국내창고입고 = "DomesticWarehouseReceived";
    public const string 국내기사상차 = "DomesticCarrierPickup";
    public const string 공동주택하차 = "ApartmentDropoff";
    public const string 분배진행중 = "DistributionInProgress";
    public const string 완료 = "Completed";
    public const string 예외 = "Exception";
}

public sealed class 공동구매해외선적추적조회조건
{
    public string? 공동구매Id { get; set; }
    public string? 주문자집단배송권키 { get; set; }
    public string? 문서관리번호 { get; set; }
    public string? 운송문서번호 { get; set; }
    public string? 현재상태코드 { get; set; }
}

public sealed class 공동구매해외선적추적Dto
{
    public string 추적Id { get; set; } = string.Empty;
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 상품요약 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 운송문서유형 { get; set; } = 공동구매선적문서유형코드.선하증권;
    public string 운송문서번호 { get; set; } = string.Empty;
    public string 운송수단 { get; set; } = 공동구매선적운송수단코드.해상;
    public string 운송사명 { get; set; } = string.Empty;
    public string 선박명 { get; set; } = string.Empty;
    public string 항차번호 { get; set; } = string.Empty;
    public string 항공편번호 { get; set; } = string.Empty;
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 출발항코드 { get; set; } = string.Empty;
    public string 도착항코드 { get; set; } = string.Empty;
    public DateTime? 예상출발시각Utc { get; set; }
    public DateTime? 실제출발시각Utc { get; set; }
    public DateTime? 예상도착시각Utc { get; set; }
    public DateTime? 실제도착시각Utc { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매선적상태코드.문서등록;
    public string 현재위치요약 { get; set; } = string.Empty;
    public DateTime? 마지막단계시각Utc { get; set; }
    public IReadOnlyList<공동구매해외선적추적이벤트Dto> 이벤트목록 { get; set; } = [];
    public string 관리자메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매해외선적공개Dto
{
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 상품요약 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 운송문서유형 { get; set; } = 공동구매선적문서유형코드.선하증권;
    public string 운송문서번호 { get; set; } = string.Empty;
    public string 운송수단 { get; set; } = 공동구매선적운송수단코드.해상;
    public string 운송사명 { get; set; } = string.Empty;
    public string 선박명 { get; set; } = string.Empty;
    public string 항차번호 { get; set; } = string.Empty;
    public string 항공편번호 { get; set; } = string.Empty;
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 출발항코드 { get; set; } = string.Empty;
    public string 도착항코드 { get; set; } = string.Empty;
    public DateTime? 예상출발시각Utc { get; set; }
    public DateTime? 실제출발시각Utc { get; set; }
    public DateTime? 예상도착시각Utc { get; set; }
    public DateTime? 실제도착시각Utc { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매선적상태코드.문서등록;
    public string 현재위치요약 { get; set; } = string.Empty;
    public DateTime? 마지막단계시각Utc { get; set; }
    public IReadOnlyList<공동구매해외선적공개이벤트Dto> 이벤트목록 { get; set; } = [];
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매해외선적공개이벤트Dto
{
    public string 이벤트코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 위치요약 { get; set; } = string.Empty;
    public DateTime 발생시각Utc { get; set; }
    public string 출처주체코드 { get; set; } = string.Empty;
}

public sealed class 공동구매해외선적추적이벤트Dto
{
    public string 이벤트코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 위치요약 { get; set; } = string.Empty;
    public DateTime 발생시각Utc { get; set; }
    public string 출처주체코드 { get; set; } = string.Empty;
    public string 증빙참조 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public bool 주문자공개여부 { get; set; } = true;
}

public sealed class 공동구매해외선적추적저장요청
{
    public string? 추적Id { get; set; }
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 상품요약 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 운송문서유형 { get; set; } = 공동구매선적문서유형코드.선하증권;
    public string 운송문서번호 { get; set; } = string.Empty;
    public string 운송수단 { get; set; } = 공동구매선적운송수단코드.해상;
    public string 운송사명 { get; set; } = string.Empty;
    public string 선박명 { get; set; } = string.Empty;
    public string 항차번호 { get; set; } = string.Empty;
    public string 항공편번호 { get; set; } = string.Empty;
    public string 출발국가코드 { get; set; } = string.Empty;
    public string 출발항코드 { get; set; } = string.Empty;
    public string 도착항코드 { get; set; } = string.Empty;
    public DateTime? 예상출발시각Utc { get; set; }
    public DateTime? 실제출발시각Utc { get; set; }
    public DateTime? 예상도착시각Utc { get; set; }
    public DateTime? 실제도착시각Utc { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매선적상태코드.문서등록;
    public string 현재위치요약 { get; set; } = string.Empty;
    public DateTime? 마지막단계시각Utc { get; set; }
    public IReadOnlyList<공동구매해외선적추적이벤트Dto> 이벤트목록 { get; set; } = [];
    public string 관리자메모 { get; set; } = string.Empty;
}

public sealed class 공동구매해외선적추적이벤트추가요청
{
    public string 이벤트코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 위치요약 { get; set; } = string.Empty;
    public DateTime? 발생시각Utc { get; set; }
    public string 출처주체코드 { get; set; } = string.Empty;
    public string 증빙참조 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public bool 주문자공개여부 { get; set; } = true;
}

public sealed class 공동구매해외선적통관동기화요청
{
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 통관화물관리번호 { get; set; } = string.Empty;
    public string 마스터선하증권번호 { get; set; } = string.Empty;
    public string 하우스선하증권번호 { get; set; } = string.Empty;
    public int? 선하증권연도 { get; set; }
    public bool 주문자공개여부 { get; set; } = true;
}

public sealed class 공동구매해외선적통관동기화결과
{
    public bool 동기화됨 { get; set; }
    public string 메시지 { get; set; } = string.Empty;
    public string 통관단계명 { get; set; } = string.Empty;
    public string Customs위치요약 { get; set; } = string.Empty;
    public DateTimeOffset 조회시각Utc { get; set; }
    public 공동구매해외선적추적Dto? 선적 { get; set; }
}
