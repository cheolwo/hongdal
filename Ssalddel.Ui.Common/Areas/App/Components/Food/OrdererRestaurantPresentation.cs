using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Food;

public static class OrdererRestaurantPresentation
{
    public static string EmptyMessage(음식점공개목록ViewModel list)
        => list.검색조건사용중
            ? "검색어나 주문 가능 조건을 바꿔 다시 조회해 주세요."
            : "운영자가 공개 프로필과 메뉴를 등록하면 이 권역에 표시됩니다.";

    public static string DistanceLabel(decimal? distanceKm)
        => distanceKm.HasValue ? $"기준점 {distanceKm.Value:0.##}km" : "거리 기준 없음";

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    public static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
