using Ssalddel.Contracts.Mart;

namespace Ssalddel.Ui.Common.Areas.App.Components.Mart;

public static class OrdererMartOrderRequestPresentation
{
    public static decimal EstimatedTotal(마트공개상품상세응답 product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.판매가 * Math.Clamp(quantity, 0, 100);
    }

    public static string ShortId(Guid value)
        => value.ToString("N")[..12].ToUpperInvariant();

    public static string FormatDate(DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    public static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
