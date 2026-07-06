using 홍달.도메인.배차;

namespace 홍달.Services.Dispatch.Engine;

public static class 음식배달배차원본유형
{
    public const string 음식점주문 = "RestaurantFoodOrder";
    public const string 홍달마트주문 = "HongdalMartOrder";
    public const string 홍달마트포장완료주문 = "HongdalMartPackedOrder";

    public static bool IsRestaurantOrder(string? sourceType)
        => string.Equals(sourceType, 음식점주문, StringComparison.OrdinalIgnoreCase)
           || string.Equals(sourceType, "FoodOrder", StringComparison.OrdinalIgnoreCase);

    public static bool IsMartOrder(string? sourceType)
        => string.Equals(sourceType, 홍달마트주문, StringComparison.OrdinalIgnoreCase)
           || string.Equals(sourceType, 홍달마트포장완료주문, StringComparison.OrdinalIgnoreCase)
           || string.Equals(sourceType, "MartFoodOrder", StringComparison.OrdinalIgnoreCase);
}

public sealed record 음식배달배차흐름(
    string 흐름코드,
    string 표시명,
    bool 창고선행작업필요,
    bool 배차시작가능,
    string 배차시작조건);

public interface I음식배달배차흐름Resolver
{
    음식배달배차흐름 Resolve(배차대기 queue);
}

public sealed class 음식배달배차흐름Resolver : I음식배달배차흐름Resolver
{
    public 음식배달배차흐름 Resolve(배차대기 queue)
    {
        if (음식배달배차원본유형.IsRestaurantOrder(queue.원본의뢰유형))
        {
            return new 음식배달배차흐름(
                음식배달배차원본유형.음식점주문,
                "음식점 즉시 배달",
                창고선행작업필요: false,
                배차시작가능: true,
                "음식점 주문은 결제 승인과 조리 접수 후 배달기사 배차를 시작합니다.");
        }

        if (string.Equals(queue.원본의뢰유형, 음식배달배차원본유형.홍달마트포장완료주문, StringComparison.OrdinalIgnoreCase))
        {
            return new 음식배달배차흐름(
                음식배달배차원본유형.홍달마트포장완료주문,
                "홍달마트 포장 완료 배달",
                창고선행작업필요: true,
                배차시작가능: true,
                "홍달마트 주문은 피킹과 포장 완료 후 배달기사 배차를 시작합니다.");
        }

        if (음식배달배차원본유형.IsMartOrder(queue.원본의뢰유형))
        {
            return new 음식배달배차흐름(
                음식배달배차원본유형.홍달마트주문,
                "홍달마트 준비 중 배달",
                창고선행작업필요: true,
                배차시작가능: false,
                "홍달마트 주문은 재고 확인, 피킹, 포장 완료 전에는 배차를 시작하지 않습니다.");
        }

        return new 음식배달배차흐름(
            queue.원본의뢰유형,
            "음식 배달 기본 흐름",
            창고선행작업필요: false,
            배차시작가능: true,
            "원본 유형이 명확하지 않은 음식 배달은 기본 즉시 배차 흐름으로 처리합니다.");
    }
}
