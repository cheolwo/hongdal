using 살뜰.도메인.공통;
using 살뜰.도메인.배차;

namespace 살뜰.Services.Dispatch.Engine;

public static class 운송의뢰배차원천유형
{
    public const string 화주운송의뢰 = "CargoTransport";
    public const string 주문자화물주문 = "OrdererCargoOrder";
    public const string 수입화물운송 = "ImportCargoTransport";
    public const string 창고출고연계운송 = "WarehouseOutboundCargo";
    public const string 판매채널출고 = "SalesChannelOutboundCargo";
    public const string 살뜰마트출고 = "SsalddelMartOutboundCargo";
    public const string 공동주문국내운송 = "GroupPurchaseCargoTransport";
    public const string Fcl연계운송 = "FclCargoTransport";
    public const string Lcl연계운송 = "LclCargoTransport";
    public const string 음식점주문 = "RestaurantFoodOrder";
    public const string 음식주문 = "FoodOrder";
    public const string 살뜰마트주문 = "SsalddelMartOrder";
    public const string 살뜰마트음식주문 = "MartFoodOrder";
    public const string 살뜰마트포장완료주문 = "SsalddelMartPackedOrder";

    public static bool Is화물용달운송(string? sourceType)
        => IsAny(
            sourceType,
            화주운송의뢰,
            주문자화물주문,
            수입화물운송,
            창고출고연계운송,
            판매채널출고,
            살뜰마트출고,
            공동주문국내운송,
            Fcl연계운송,
            Lcl연계운송);

    public static bool Is창고출고연계운송(string? sourceType)
        => IsAny(sourceType, 창고출고연계운송, 판매채널출고, 살뜰마트출고, 화주운송의뢰, 주문자화물주문);

    public static bool Is수입통관연계운송(string? sourceType)
        => IsAny(sourceType, 수입화물운송, 공동주문국내운송, Fcl연계운송, Lcl연계운송);

    public static bool Is음식배달운송(string? sourceType)
        => Is음식점주문(sourceType) || Is살뜰마트음식주문(sourceType);

    public static bool Is음식점주문(string? sourceType)
        => IsAny(sourceType, 음식점주문, 음식주문);

    public static bool Is살뜰마트음식주문(string? sourceType)
        => IsAny(sourceType, 살뜰마트주문, 살뜰마트음식주문, 살뜰마트포장완료주문);

    public static bool IsAny(string? sourceType, params string[] candidates)
        => !string.IsNullOrWhiteSpace(sourceType)
           && candidates.Any(candidate => string.Equals(sourceType, candidate, StringComparison.OrdinalIgnoreCase));
}

public sealed record 운송의뢰배차원천분류(
    string 원천유형,
    int 배차업무유형,
    string 상위흐름,
    string 표시명,
    string 배차대상,
    bool 출고예정대상여부,
    bool 창고선행작업필요);

public interface I운송의뢰배차원천분류Service
{
    운송의뢰배차원천분류 분류(운송원장 queue);
}

public sealed class 운송의뢰배차원천분류Service : I운송의뢰배차원천분류Service
{
    public 운송의뢰배차원천분류 분류(운송원장 queue)
    {
        if (queue.배차업무유형 == 상태값.배차업무유형.음식배달
            || 운송의뢰배차원천유형.Is음식배달운송(queue.원본의뢰유형))
        {
            return 분류음식배달(queue);
        }

        return 분류화물용달(queue);
    }

    private static 운송의뢰배차원천분류 분류화물용달(운송원장 queue)
    {
        if (운송의뢰배차원천유형.Is수입통관연계운송(queue.원본의뢰유형))
        {
            return new 운송의뢰배차원천분류(
                queue.원본의뢰유형,
                상태값.배차업무유형.용달운송,
                "수입/통관 연계 운송",
                "같이 주문 또는 수입 화물 국내 운송",
                "보세구역, 항만, 공항, 국내 3PL 또는 세대 직배송 화물",
                출고예정대상여부: true,
                창고선행작업필요: false);
        }

        if (운송의뢰배차원천유형.Is창고출고연계운송(queue.원본의뢰유형))
        {
            return new 운송의뢰배차원천분류(
                queue.원본의뢰유형,
                상태값.배차업무유형.용달운송,
                "창고 출고 연계 운송",
                "출고 예정 상품 운송",
                "화주 의뢰, 판매채널 출고, 창고 출고, 알뜰살뜰 마트 출고 화물",
                출고예정대상여부: true,
                창고선행작업필요: true);
        }

        return new 운송의뢰배차원천분류(
            string.IsNullOrWhiteSpace(queue.원본의뢰유형) ? 운송의뢰배차원천유형.화주운송의뢰 : queue.원본의뢰유형,
            상태값.배차업무유형.용달운송,
            "일반 운송 의뢰",
            "화물/용달 운송 의뢰",
            "화주 또는 주문자가 등록한 상하차 운송 의뢰",
            출고예정대상여부: false,
            창고선행작업필요: false);
    }

    private static 운송의뢰배차원천분류 분류음식배달(운송원장 queue)
    {
        if (운송의뢰배차원천유형.Is살뜰마트음식주문(queue.원본의뢰유형))
        {
            return new 운송의뢰배차원천분류(
                queue.원본의뢰유형,
                상태값.배차업무유형.음식배달,
                "알뜰살뜰 마트 즉시배송",
                "알뜰살뜰 마트 주문 배달",
                "피킹/포장 완료 또는 완료 예정 상품",
                출고예정대상여부: true,
                창고선행작업필요: true);
        }

        return new 운송의뢰배차원천분류(
            string.IsNullOrWhiteSpace(queue.원본의뢰유형) ? 운송의뢰배차원천유형.음식점주문 : queue.원본의뢰유형,
            상태값.배차업무유형.음식배달,
            "음식점 즉시 배달",
            "음식점 주문 배달",
            "조리 완료 또는 조리 예정 음식 주문",
            출고예정대상여부: false,
            창고선행작업필요: false);
    }
}
