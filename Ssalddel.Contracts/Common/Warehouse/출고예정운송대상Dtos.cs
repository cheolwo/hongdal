namespace Ssalddel.Contracts.Common.Warehouse;

public static class 출고예정운송대상원천유형
{
    public const string 화주운송의뢰 = "ShipperTransportRequest";
    public const string 창고출고예정 = "WarehouseOutboundPlanned";
    public const string 판매채널주문 = "SalesChannelOrder";
    public const string 공동주문수입 = "GroupPurchaseImport";
    public const string 음식주문 = "FoodOrder";
}

public sealed class 출고예정운송대상
{
    public string 원천유형 { get; set; } = string.Empty;

    public string 원천참조번호 { get; set; } = string.Empty;

    public string 표시명 { get; set; } = string.Empty;

    public long? 출고예정Id { get; set; }

    public string? 운송의뢰Id { get; set; }

    public string 판매자UserId { get; set; } = string.Empty;

    public string 주문자UserId { get; set; } = string.Empty;

    public string 상차주소 { get; set; } = string.Empty;

    public decimal? 상차위도 { get; set; }

    public decimal? 상차경도 { get; set; }

    public string 하차주소 { get; set; } = string.Empty;

    public decimal? 하차위도 { get; set; }

    public decimal? 하차경도 { get; set; }

    public string 온도조건 { get; set; } = "상온";

    public bool 파손주의 { get; set; }

    public IReadOnlyList<출고예정운송대상라인> Lines { get; set; } = [];
}

public sealed class 출고예정운송대상라인
{
    public string LineKey { get; set; } = string.Empty;

    public long? SalesProductId { get; set; }

    public long? InboundProductId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal? WeightKg { get; set; }
}
