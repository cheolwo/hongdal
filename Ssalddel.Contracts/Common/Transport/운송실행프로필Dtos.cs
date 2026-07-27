using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Transport;

public static class 운송실행유형코드
{
    public const string 음식배달 = "FoodDelivery";
    public const string 화물운송 = "CargoTransport";

    public static IReadOnlyList<string> 전체 { get; } =
    [
        음식배달,
        화물운송
    ];
}

public static class 운송실행상세유형코드
{
    public const string 음식점음식배달 = "RestaurantFoodDelivery";
    public const string 마트라스트마일배송 = "MartLastMileDelivery";
    public const string 일반화물운송 = "GeneralCargoTransport";
    public const string 창고출고화물운송 = "WarehouseOutboundCargo";
    public const string 같이주문국내배송 = "TogetherOrderDomesticDelivery";
    public const string 같이수입국내인계 = "TogetherImportDomesticHandoff";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TransportExecutionProfile,
    SsalddelCodeLayer.Contract,
    "공통 운송 실행 원장에서 음식 배달과 화물 운송의 업무 용어와 필수 상세 경계를 구분한다.",
    FlowOrder = 10,
    Boundary = "배차·이동 생명주기만 공통화하며 음식 조리 상태와 화물 제원 같은 유형별 업무 의미를 합치지 않는다.")]
public sealed class 운송실행프로필Dto
{
    public string 실행유형코드 { get; set; } = string.Empty;

    public string 상세유형코드 { get; set; } = string.Empty;

    public string 표시명 { get; set; } = string.Empty;

    public string 픽업행동명 { get; set; } = string.Empty;

    public string 완료행동명 { get; set; } = string.Empty;

    public bool 조리상태필요 { get; set; }

    public bool 화물제원필요 { get; set; }

    public bool 창고선행작업필요 { get; set; }
}
