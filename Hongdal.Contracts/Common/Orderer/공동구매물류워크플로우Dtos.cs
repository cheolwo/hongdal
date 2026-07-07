namespace Hongdal.Contracts.Common.Orderer;

public static class 공동구매물류워크플로우주체코드
{
    public const string 플랫폼 = "Platform";
    public const string 판매자 = "Seller";
    public const string 해외판매자 = "OverseasSeller";
    public const string 수입자 = "Importer";
    public const string 관세사 = "CustomsBroker";
    public const string 국내창고 = "DomesticWarehouse";
    public const string 국내물류대행 = "DomesticLogisticsProxy";
    public const string 판매채널운영자 = "SalesChannelOperator";
    public const string 운송주체 = "Carrier";
    public const string 집단대표 = "GroupRepresentative";
    public const string 개별주문자 = "IndividualOrderer";
}

public static class 공동구매판매자출처유형코드
{
    public const string 국내 = "Domestic";
    public const string 해외 = "Overseas";
}

public static class 공동구매물류증빙코드
{
    public const string 판매자포장명세 = "SellerPackingList";
    public const string 해외판매자포장명세 = "OverseasSellerPackingList";
    public const string 수출인보이스 = "ExportInvoice";
    public const string 수입신고서 = "CustomsDeclaration";
    public const string 수입검사결과 = "ImportInspectionResult";
    public const string 국내창고입고보고 = "DomesticWarehouseReceivingReport";
    public const string 물류대행입고확인서 = "LogisticsProxyInboundReceipt";
    public const string 재고로트스냅샷 = "InventoryLotSnapshot";
    public const string 판매채널출품스냅샷 = "SalesChannelListingSnapshot";
    public const string 출고배치계획스냅샷 = "OutboundBatchPlanSnapshot";
    public const string 상차사진 = "PickupPhoto";
    public const string 상차인계인수증 = "PickupHandoverReceipt";
    public const string 하차사진 = "DropoffPhoto";
    public const string 집단대표수령확인서 = "GroupRepresentativeReceipt";
    public const string 세대별분배체크리스트 = "UnitDistributionChecklist";
    public const string 개별수령확인 = "IndividualReceiptConfirmation";
    public const string 온도로그 = "TemperatureLog";
}

public sealed class 공동구매물류워크플로우단계Dto
{
    public string 단계코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public int 순서 { get; set; }
    public string 책임주체코드 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public IReadOnlyList<string> 필요증빙코드목록 { get; set; } = [];
    public IReadOnlyList<string> 오류대응코드목록 { get; set; } = [];
}

public sealed class 공동구매책임구간Dto
{
    public string 구간코드 { get; set; } = string.Empty;
    public string From단계코드 { get; set; } = string.Empty;
    public string To단계코드 { get; set; } = string.Empty;
    public string 책임주체코드 { get; set; } = string.Empty;
    public string 책임범위 { get; set; } = string.Empty;
    public IReadOnlyList<string> 필요증빙코드목록 { get; set; } = [];
}

public sealed class 공동구매물류워크플로우정의Dto
{
    public string 워크플로우Id { get; set; } = string.Empty;
    public string 버전 { get; set; } = "1.0";
    public string 표시명 { get; set; } = string.Empty;
    public string 품목분류코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 판매자출처유형 { get; set; } = 공동구매판매자출처유형코드.국내;
    public string 주문자집단배송권유형 { get; set; } = string.Empty;
    public bool 활성여부 { get; set; } = true;
    public IReadOnlyList<공동구매물류워크플로우단계Dto> 단계목록 { get; set; } = [];
    public IReadOnlyList<공동구매책임구간Dto> 책임구간목록 { get; set; } = [];
    public IReadOnlyList<string> 태그목록 { get; set; } = [];
    public string 메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매물류워크플로우조회조건
{
    public string? 품목분류코드 { get; set; }
    public string? 온도코드 { get; set; }
    public string? 물류방식 { get; set; }
    public string? 판매자출처유형 { get; set; }
    public string? 주문자집단배송권유형 { get; set; }
    public bool 활성만 { get; set; } = true;
}

public sealed class 공동구매물류워크플로우저장요청
{
    public string? 워크플로우Id { get; set; }
    public string? 버전 { get; set; }
    public string 표시명 { get; set; } = string.Empty;
    public string 품목분류코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 판매자출처유형 { get; set; } = 공동구매판매자출처유형코드.국내;
    public string 주문자집단배송권유형 { get; set; } = string.Empty;
    public bool 활성여부 { get; set; } = true;
    public IReadOnlyList<공동구매물류워크플로우단계Dto> 단계목록 { get; set; } = [];
    public IReadOnlyList<공동구매책임구간Dto> 책임구간목록 { get; set; } = [];
    public IReadOnlyList<string> 태그목록 { get; set; } = [];
    public string 메모 { get; set; } = string.Empty;
}
