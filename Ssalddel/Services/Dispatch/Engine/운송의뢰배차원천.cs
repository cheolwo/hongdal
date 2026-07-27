using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Transport;
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
    bool 창고선행작업필요,
    운송실행프로필Dto 실행프로필);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TransportExecutionProfile,
    SsalddelCodeLayer.Domain,
    "운송 실행 투영의 원천 유형과 배차 업무 유형을 음식·마트 라스트마일·화물 상세 프로필로 분류한다.",
    FlowOrder = 20,
    Boundary = "프로필은 표시와 유형별 Policy 입력을 제공할 뿐 배차 확정이나 원장 상태를 변경하지 않는다.")]
public static class 운송실행프로필Factory
{
    public static 운송실행프로필Dto Create(운송원장 transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        return Create(transport.원본의뢰유형, transport.배차업무유형);
    }

    public static 운송실행프로필Dto Create(string? sourceType, int? dispatchBusinessType = null)
    {
        if (운송의뢰배차원천유형.Is살뜰마트음식주문(sourceType))
        {
            return Profile(
                운송실행유형코드.음식배달,
                운송실행상세유형코드.마트라스트마일배송,
                "마트 라스트마일 배송",
                "포장 상품 픽업",
                "주문 전달",
                cookingRequired: false,
                cargoSpecificationRequired: false,
                warehousePreworkRequired: true);
        }

        if (운송의뢰배차원천유형.Is음식점주문(sourceType)
            || dispatchBusinessType == 상태값.배차업무유형.음식배달)
        {
            return Profile(
                운송실행유형코드.음식배달,
                운송실행상세유형코드.음식점음식배달,
                "음식점 음식 배달",
                "음식점 픽업",
                "고객 전달",
                cookingRequired: true,
                cargoSpecificationRequired: false,
                warehousePreworkRequired: false);
        }

        if (운송의뢰배차원천유형.IsAny(sourceType, 운송의뢰배차원천유형.공동주문국내운송))
        {
            return Profile(
                운송실행유형코드.화물운송,
                운송실행상세유형코드.같이주문국내배송,
                "같이 주문 국내 배송",
                "공동 주문 화물 상차",
                "집결지·세대 인계",
                cookingRequired: false,
                cargoSpecificationRequired: true,
                warehousePreworkRequired: false);
        }

        if (운송의뢰배차원천유형.IsAny(
                sourceType,
                운송의뢰배차원천유형.수입화물운송,
                운송의뢰배차원천유형.Fcl연계운송,
                운송의뢰배차원천유형.Lcl연계운송))
        {
            return Profile(
                운송실행유형코드.화물운송,
                운송실행상세유형코드.같이수입국내인계,
                "같이 수입 국내 인계",
                "보세·창고 화물 상차",
                "국내 목적지 인계",
                cookingRequired: false,
                cargoSpecificationRequired: true,
                warehousePreworkRequired: false);
        }

        if (운송의뢰배차원천유형.IsAny(
                sourceType,
                운송의뢰배차원천유형.창고출고연계운송,
                운송의뢰배차원천유형.판매채널출고,
                운송의뢰배차원천유형.살뜰마트출고))
        {
            return Profile(
                운송실행유형코드.화물운송,
                운송실행상세유형코드.창고출고화물운송,
                "창고 출고 화물 운송",
                "출고 화물 상차",
                "화물 하차",
                cookingRequired: false,
                cargoSpecificationRequired: true,
                warehousePreworkRequired: true);
        }

        return Profile(
            운송실행유형코드.화물운송,
            운송실행상세유형코드.일반화물운송,
            "일반 화물 운송",
            "화물 상차",
            "화물 하차",
            cookingRequired: false,
            cargoSpecificationRequired: true,
            warehousePreworkRequired: false);
    }

    private static 운송실행프로필Dto Profile(
        string executionType,
        string detailType,
        string displayName,
        string pickupAction,
        string completionAction,
        bool cookingRequired,
        bool cargoSpecificationRequired,
        bool warehousePreworkRequired)
        => new()
        {
            실행유형코드 = executionType,
            상세유형코드 = detailType,
            표시명 = displayName,
            픽업행동명 = pickupAction,
            완료행동명 = completionAction,
            조리상태필요 = cookingRequired,
            화물제원필요 = cargoSpecificationRequired,
            창고선행작업필요 = warehousePreworkRequired
        };
}

public interface I운송의뢰배차원천분류Service
{
    운송의뢰배차원천분류 분류(운송원장 queue);
}

public sealed class 운송의뢰배차원천분류Service : I운송의뢰배차원천분류Service
{
    public 운송의뢰배차원천분류 분류(운송원장 queue)
    {
        var executionProfile = 운송실행프로필Factory.Create(queue);
        if (executionProfile.실행유형코드 == 운송실행유형코드.음식배달)
        {
            return 분류음식배달(queue, executionProfile);
        }

        return 분류화물용달(queue, executionProfile);
    }

    private static 운송의뢰배차원천분류 분류화물용달(
        운송원장 queue,
        운송실행프로필Dto executionProfile)
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
                창고선행작업필요: false,
                실행프로필: executionProfile);
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
                창고선행작업필요: true,
                실행프로필: executionProfile);
        }

        return new 운송의뢰배차원천분류(
            string.IsNullOrWhiteSpace(queue.원본의뢰유형) ? 운송의뢰배차원천유형.화주운송의뢰 : queue.원본의뢰유형,
            상태값.배차업무유형.용달운송,
            "일반 운송 의뢰",
            "화물/용달 운송 의뢰",
            "화주 또는 주문자가 등록한 상하차 운송 의뢰",
            출고예정대상여부: false,
            창고선행작업필요: false,
            실행프로필: executionProfile);
    }

    private static 운송의뢰배차원천분류 분류음식배달(
        운송원장 queue,
        운송실행프로필Dto executionProfile)
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
                창고선행작업필요: true,
                실행프로필: executionProfile);
        }

        return new 운송의뢰배차원천분류(
            string.IsNullOrWhiteSpace(queue.원본의뢰유형) ? 운송의뢰배차원천유형.음식점주문 : queue.원본의뢰유형,
            상태값.배차업무유형.음식배달,
            "음식점 즉시 배달",
            "음식점 주문 배달",
            "조리 완료 또는 조리 예정 음식 주문",
            출고예정대상여부: false,
            창고선행작업필요: false,
            실행프로필: executionProfile);
    }
}
