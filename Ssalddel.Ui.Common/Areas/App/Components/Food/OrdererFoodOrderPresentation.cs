using MudBlazor;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Ui.Common.Areas.App.Components.Food;

/// <summary>음식 주문 내역 하위 컴포넌트가 공유하는 표시 형식만 제공합니다.</summary>
internal static class OrdererFoodOrderPresentation
{
    public static string RestaurantLabel(주문자음식주문요약응답 order)
        => string.IsNullOrWhiteSpace(order.음식점명)
            ? $"음식점 #{order.음식점Id}"
            : order.음식점명.Trim();

    public static string DispatchLabel(string? value)
        => string.IsNullOrWhiteSpace(value) || value == 음식주문배차상태코드.미요청
            ? "배차 전"
            : $"배차 {value.Trim()}";

    public static Color StatusColor(string? status)
        => 음식주문상태코드.Normalize(status) switch
        {
            음식주문상태코드.전달완료 => Color.Success,
            음식주문상태코드.취소 => Color.Error,
            음식주문상태코드.조리중 or 음식주문상태코드.픽업대기 => Color.Info,
            음식주문상태코드.기사배정 or 음식주문상태코드.픽업완료 => Color.Primary,
            _ => Color.Warning
        };

    public static string Address(string? address, string? detailAddress)
        => string.Join(" ", new[] { address, detailAddress }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())) is { Length: > 0 } value
            ? value
            : "—";

    public static string FormatDate(DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    public static string FormatOptionalDate(DateTime? value)
        => value.HasValue ? FormatDate(value.Value) : "—";

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    public static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
