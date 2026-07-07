namespace Hongdal.Contracts.Common.Orderer;

public static class 공동구매커머스이행상태코드
{
    public const string 초안 = "Draft";
    public const string 물류대행선택 = "LogisticsProxySelected";
    public const string 입고요청 = "InboundRequested";
    public const string 입고완료 = "InboundCompleted";
    public const string 출품준비 = "SalesListingReady";
    public const string 판매채널출품완료 = "SalesChannelListed";
    public const string 출고배치준비 = "OutboundBatchReady";
    public const string 보류 = "Paused";
}

public static class 공동구매판매채널유형코드
{
    public const string 네이버스마트스토어 = "NaverSmartStore";
    public const string 쿠팡 = "Coupang";
    public const string 기타 = "Other";
}

public sealed class 공동구매커머스이행계획조회조건
{
    public string? 공동구매Id { get; set; }
    public string? 주문자집단배송권키 { get; set; }
    public string? 문서관리번호 { get; set; }
    public string? 현재상태코드 { get; set; }
    public string? 판매채널유형 { get; set; }
    public long? 창고Id { get; set; }
    public long? 입고상품Id { get; set; }
    public bool? 플랫폼물류대행사용 { get; set; }
}

public sealed class 공동구매커머스이행계획Dto
{
    public string 계획Id { get; set; } = string.Empty;
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public bool 플랫폼물류대행사용 { get; set; } = true;
    public string 물류대행사명 { get; set; } = string.Empty;
    public string 물류대행거점명 { get; set; } = string.Empty;
    public long? 창고Id { get; set; }
    public string 창고명 { get; set; } = string.Empty;
    public long? 입고요청Id { get; set; }
    public long? 입고상품Id { get; set; }
    public long? 판매상품Id { get; set; }
    public string 재고로트코드 { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public int 예상입고수량 { get; set; }
    public int 판매가능수량 { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매커머스이행상태코드.초안;
    public string 입고상태코드 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 출고배치상태코드 { get; set; } = string.Empty;
    public IReadOnlyList<공동구매판매채널계획Dto> 판매채널목록 { get; set; } = [];
    public string 출고배치정책코드 { get; set; } = string.Empty;
    public string 관리자메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매커머스이행계획공개Dto
{
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public bool 플랫폼물류대행사용 { get; set; }
    public string 물류대행사명 { get; set; } = string.Empty;
    public string 물류대행거점명 { get; set; } = string.Empty;
    public string 창고명 { get; set; } = string.Empty;
    public string 재고로트코드 { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public int 예상입고수량 { get; set; }
    public int 판매가능수량 { get; set; }
    public string 현재상태코드 { get; set; } = string.Empty;
    public string 입고상태코드 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 출고배치상태코드 { get; set; } = string.Empty;
    public IReadOnlyList<공동구매판매채널계획공개Dto> 판매채널목록 { get; set; } = [];
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매판매채널계획Dto
{
    public string 채널유형 { get; set; } = 공동구매판매채널유형코드.네이버스마트스토어;
    public long? 판매채널계정Id { get; set; }
    public string 스토어명 { get; set; } = string.Empty;
    public long? 출품Id { get; set; }
    public string 채널상품번호 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 외부상품Url { get; set; } = string.Empty;
}

public sealed class 공동구매판매채널계획공개Dto
{
    public string 채널유형 { get; set; } = string.Empty;
    public string 스토어명 { get; set; } = string.Empty;
    public string 채널상품번호 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 외부상품Url { get; set; } = string.Empty;
}

public sealed class 공동구매커머스이행계획저장요청
{
    public string? 계획Id { get; set; }
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public bool 플랫폼물류대행사용 { get; set; } = true;
    public string 물류대행사명 { get; set; } = string.Empty;
    public string 물류대행거점명 { get; set; } = string.Empty;
    public long? 창고Id { get; set; }
    public string 창고명 { get; set; } = string.Empty;
    public long? 입고요청Id { get; set; }
    public long? 입고상품Id { get; set; }
    public long? 판매상품Id { get; set; }
    public string 재고로트코드 { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public int 예상입고수량 { get; set; }
    public int 판매가능수량 { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매커머스이행상태코드.초안;
    public string 입고상태코드 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 출고배치상태코드 { get; set; } = string.Empty;
    public IReadOnlyList<공동구매판매채널계획Dto> 판매채널목록 { get; set; } = [];
    public string 출고배치정책코드 { get; set; } = string.Empty;
    public string 관리자메모 { get; set; } = string.Empty;
}
